using System;
using System.Collections.Generic;
using System.Threading.Tasks;


using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using Booth.Common;
using Booth.PortfolioManager.Domain;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Repository.Serialization;
using System.Threading.Tasks;

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

        Task UpdatePropertiesAsync(Stock stock, Date date);
        Task UpdateDividendRulesAsync(Stock stock, Date date);
        Task AddCorporateActionAsync(Stock stock, Guid id);
        Task DeleteCorporateActionAsync(Stock stock, Guid id);
        Task UpdateCorporateActionAsync(Stock stock, Guid id);
        Task UpdateRelativeNTAsAsync(Stock stock, Date date);
    }

    public class StockRepository : Repository<Stock>, IStockRepository
    {
        public StockRepository(IPortfolioManagerDatabase database)
            : base(database, "Stocks")
        {
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

        public override Task UpdateAsync(Stock entity)
        {
            throw new NotImplementedException();
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

            var addValue = Builders<BsonDocument>.Update
                .Push("corporateActions", action);

            _Collection.UpdateOne(Builders<BsonDocument>.Filter.Eq("_id", stock.Id), addValue);
        }

        public void DeleteCorporateAction(Stock stock, Guid id)
        {
            var deleteValue = Builders<BsonDocument>.Update
                .Pull("corporateActions", Builders<BsonDocument>.Filter.Eq("corporateActions._id", id));

            _Collection.UpdateOne(Builders<BsonDocument>.Filter.Eq("_id", stock.Id), deleteValue);
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

        public Task UpdatePropertiesAsync(Stock stock, Date date)
        {
            throw new NotImplementedException();
        }

        public Task UpdateDividendRulesAsync(Stock stock, Date date)
        {
            throw new NotImplementedException();
        }

        public Task AddCorporateActionAsync(Stock stock, Guid id)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCorporateActionAsync(Stock stock, Guid id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateCorporateActionAsync(Stock stock, Guid id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRelativeNTAsAsync(Stock stock, Date date)
        {
            throw new NotImplementedException();
        }
    }
}
