using System;
using System.Collections.Generic;
using System.Linq;


using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using Booth.Common;
using Booth.PortfolioManager.Domain;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Repository.Serialization;


namespace Booth.PortfolioManager.Repository
{
    public interface IStockRepository : IRepository<Stock>
    {
        void UpdateProperties(Stock stock, Date date);

        void UpdateDividendRules(Stock stock, Date date);
        void AddCorporateAction(Stock stock, Guid id);
        void DeleteCorporateAction(Stock stock, Guid id);
        void UpdateCorporateAction(Stock stock, Guid id);

        void UpdateRelativeNTAs(Stock stock, Date date);
    }

    public class StockRepository : Repository<Stock>, IStockRepository
    {
        public StockRepository(IPortfolioManagerDatabase database)
            : base(database, "Stocks")
        {
        }

        public void Test(Stock entity)
        {
            var bson = entity.ToBsonDocument();

            var entity2 = BsonSerializer.Deserialize<Stock>(bson);
        }

        public override void Update(Stock entity)
        {
            var bson = Builders<BsonDocument>.Update
            .Set("listingDate", entity.EffectivePeriod.FromDate)            
            .Set("trust", entity.Trust);

            if (entity.EffectivePeriod.ToDate != Date.MaxValue)
                bson.Set("delistingDate", entity.EffectivePeriod.ToDate);

            _Collection.UpdateOne(Builders<BsonDocument>.Filter.Eq("_id", entity.Id), bson);
        }

        public void UpdateProperties(Stock stock, Date date)
        {
            base.UpdateEffectiveProperties<StockProperties>(stock, date, stock.Properties[date], "properties");
        }

        public void UpdateDividendRules(Stock stock, Date date)
        {
            base.UpdateEffectiveProperties<DividendRules>(stock, date, stock.DividendRules[date], "dividendRules");
        }

        public void AddCorporateAction(Stock stock, Guid id)
        {
            var action = stock.CorporateActions[id];

            var filter = Builders<BsonDocument>.Filter.Eq("_id", stock.Id);

            var addValue = Builders<BsonDocument>.Update
                .Push("corporateActions", action);

            _Collection.UpdateOne(filter, addValue);
        }

        public void DeleteCorporateAction(Stock stock, Guid id)
        {
            var filter = Builders<BsonDocument>.Filter
                .And(new[]
                    {
                    Builders<BsonDocument>.Filter.Eq("_id", stock.Id),
                    Builders<BsonDocument>.Filter.Eq("corporateActions._id", id)
                    }
                );

            _Collection.DeleteOne(filter);
        }

        public void UpdateCorporateAction(Stock stock, Guid id)
        {
            var action = stock.CorporateActions[id];

            var filter = Builders<BsonDocument>.Filter
                .And(new[]
                    {
                    Builders<BsonDocument>.Filter.Eq("_id", stock.Id),
                    Builders<BsonDocument>.Filter.Eq("corporateActions._id", id)
                    }
                );
            
            var updateValue = Builders<BsonDocument>.Update
                .Set("corporateActions.$", action);

            _Collection.UpdateOne(filter, updateValue);
        }

        public void UpdateRelativeNTAs(Stock stock, Date date)
        {
            var stapledSecurity = stock as StapledSecurity;
            if (stapledSecurity == null)
                throw new Exception("Can only update Relative NTAs on stapled securities");

            base.UpdateEffectiveProperties<RelativeNTA>(stock, date, stapledSecurity.RelativeNTAs[date], "relativeNTAs");
        }

        public static void ConfigureSerializaton()
        {
            BsonSerializer.RegisterSerializer<Stock>(new StockSerializer());

            BsonClassMap.RegisterClassMap<CorporateAction>(cm =>
            {
                cm.AutoMap();
                cm.UnmapProperty(c => c.Stock);
            });

            var actionTypes = typeof(CorporateAction).GetSubclassesOf(true);
            foreach (var actionType in actionTypes)
                BsonClassMap.LookupClassMap(actionType);
        }
    }
}
