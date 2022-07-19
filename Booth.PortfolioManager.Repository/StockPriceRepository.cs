using System;
using System.Collections.Generic;
using System.Text;

using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Repository.Serialization;

namespace Booth.PortfolioManager.Repository
{

    public interface IStockPriceRepository : IRepository<StockPriceHistory>
    {
        void UpdatePrice(StockPriceHistory stockPriceHistory, Date date);

        void UpdatePrices(StockPriceHistory stockPriceHistory, DateRange dateRange);
    }

    public class StockPriceRepository : Repository<StockPriceHistory>, IStockPriceRepository
    {
        public StockPriceRepository(IPortfolioManagerDatabase database)
            : base(database, "StockPriceHistory")
        {
        }

        public override void Update(StockPriceHistory entity)
        {
            throw new NotSupportedException();
        }

        public void UpdatePrice(StockPriceHistory stockPriceHistory, Date date)
        {
            var removePrice = Builders<BsonDocument>.Update
                .PullFilter("closingPrices", Builders<BsonDocument>.Filter.Eq("date", date));

            var addPrice = Builders<BsonDocument>.Update
                .Push("closingPrices", new StockPrice(date, stockPriceHistory.GetPrice(date)));

            _Collection.BulkWrite(new[]
            {
                new UpdateOneModel<BsonDocument>(Builders<BsonDocument>.Filter.Eq("_id", stockPriceHistory.Id), removePrice),
                new UpdateOneModel<BsonDocument>(Builders<BsonDocument>.Filter.Eq("_id", stockPriceHistory.Id), addPrice),
            }); 
        }

        public void UpdatePrices(StockPriceHistory stockPriceHistory, DateRange dateRange)
        {
            var removePrices = Builders<BsonDocument>.Update
                .PullFilter("closingPrices", Builders<BsonDocument>.Filter.And(new[]
                    {
                        Builders<BsonDocument>.Filter.Gte("date", dateRange.FromDate),
                        Builders<BsonDocument>.Filter.Lte("date", dateRange.ToDate)
                    }));

            var addPrices = Builders<BsonDocument>.Update
                .PushEach("closingPrices", stockPriceHistory.GetPrices(dateRange));

            _Collection.BulkWrite(new[]
            {
                new UpdateOneModel<BsonDocument>(Builders<BsonDocument>.Filter.Eq("_id", stockPriceHistory.Id), removePrices),
                new UpdateOneModel<BsonDocument>(Builders<BsonDocument>.Filter.Eq("_id", stockPriceHistory.Id), addPrices),
            });
        }

        public static void ConfigureSerializaton()
        {
            BsonSerializer.RegisterSerializer<StockPriceHistory>(new StockPriceHistorySerializer());
            BsonSerializer.RegisterSerializer<StockPrice>(new StockPriceSerializer());
        }
    }
}
