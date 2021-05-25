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

    }

    public class StockRepository : IStockRepository
    {

        private readonly IMongoCollection<BsonDocument> _Collection;
        public StockRepository(IPortfolioManagerDatabase database)
        {
            _Collection = database.GetCollection("Stocks");
        }

        public Stock Get(Guid id)
        {
            var bson = _Collection.Find(Builders<BsonDocument>.Filter.Eq("_id", id)).SingleOrDefault();
            if (bson == null)
                return null;

            var stock = BsonSerializer.Deserialize<Stock>(bson);

            return stock;
        }

        public IEnumerable<Stock> All()
        {
            var bsonElements = _Collection.Find("{}").ToList();

            foreach (var bson in bsonElements)
            {
                var stock = BsonSerializer.Deserialize<Stock>(bson);

                yield return stock;
            }
        }

        public void Add(Stock entity)
        {
            var bson = entity.ToBsonDocument();

            _Collection.InsertOne(bson);
        }

        public void Update(Stock entity)
        {
            var bson = Builders<BsonDocument>.Update
            .Set("listingDate", entity.EffectivePeriod.FromDate)            
            .Set("trust", entity.Trust);

            if (entity.EffectivePeriod.ToDate != Date.MaxValue)
                bson.Set("delistingDate", entity.EffectivePeriod.ToDate);

            _Collection.UpdateOne(Builders<BsonDocument>.Filter.Eq("_id", entity.Id), bson);
        }

        public void Delete(Guid id)
        {
            _Collection.DeleteOne(Builders<BsonDocument>.Filter.Eq("_id", id));
        }

        public void UpdateProperties(Stock stock, Date date)
        {
            var property = stock.Properties[date];

            var existsFilter = Builders<BsonDocument>.Filter
                .And(new[]
                    {
                    Builders<BsonDocument>.Filter.Eq("_id", stock.Id),
                    Builders<BsonDocument>.Filter.Eq("properties.date", date)
                    }
                );

            var updateValue = Builders<BsonDocument>.Update
                 .Set("properties.$.properties", property);

            var notExistsFilter = Builders<BsonDocument>.Filter
                .And(new[]
                    {
                    Builders<BsonDocument>.Filter.Eq("_id", stock.Id),
                    Builders<BsonDocument>.Filter.Ne("properties.date", date)
                    }
                );

            var addValue = Builders<BsonDocument>.Update
                .Push("properties", property); 

           _Collection.BulkWrite(new[]
            {
                new UpdateOneModel<BsonDocument>(existsFilter, updateValue),
                new UpdateOneModel<BsonDocument>(notExistsFilter, addValue)
            });  
        }

        public void UpdateDividendRules(Stock stock, Date date)
        {
            var rules = stock.DividendRules[date];

            var existsFilter = Builders<BsonDocument>.Filter
                .And(new[]
                    {
                    Builders<BsonDocument>.Filter.Eq("_id", stock.Id),
                    Builders<BsonDocument>.Filter.Eq("dividendRules.date", date)
                    }
                );


            var updateValue = Builders<BsonDocument>.Update
                 .Set("dividendRules.$.properties", rules);

            var notExistsFilter = Builders<BsonDocument>.Filter
                .And(new[]
                    {
                    Builders<BsonDocument>.Filter.Eq("_id", stock.Id),
                    Builders<BsonDocument>.Filter.Ne("dividendRules.date", date)
                    }
                );

            var addValue = Builders<BsonDocument>.Update
                .Push("properties", rules);

            _Collection.BulkWrite(new[]
             {
                new UpdateOneModel<BsonDocument>(existsFilter, updateValue),
                new UpdateOneModel<BsonDocument>(notExistsFilter, addValue)
            });
        }
        public void AddCorporateAction(Stock stock, Guid id)
        {
            var action = stock.CorporateActions[id];

            var filter = Builders<BsonDocument>.Filter
                .And(new[]
                    {
                    Builders<BsonDocument>.Filter.Eq("_id", stock.Id),
                    Builders<BsonDocument>.Filter.Eq("corporateActions._id", id)
                    }
                );


            var addValue = Builders<BsonDocument>.Update
                .Push("corporateActions", action);

            _Collection.UpdateOne(filter, addValue);
        }

        public void DeleteCorporateAction(Stock stock, Guid id)
        {
            var action = stock.CorporateActions[id];

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
