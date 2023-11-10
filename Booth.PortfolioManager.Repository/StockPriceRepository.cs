using System;
using System.Threading.Tasks;

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
        Task UpdatePriceAsync(StockPriceHistory stockPriceHistory, Date date);
        Task UpdatePricesAsync(StockPriceHistory stockPriceHistory, DateRange dateRange);
    }

    public class StockPriceRepository : Repository<StockPriceHistory>, IStockPriceRepository
    {
        public StockPriceRepository(IPortfolioManagerDatabase database)
            : base(database, "StockPriceHistory")
        {
        }

        public override Task UpdateAsync(StockPriceHistory entity)
        {
            throw new NotSupportedException();
        }

        public async Task UpdatePriceAsync(StockPriceHistory stockPriceHistory, Date date)
        {
            var removePrice = Builders<BsonDocument>.Update
                .PullFilter("closingPrices", Builders<BsonDocument>.Filter.Eq("date", date));

            var addPrice = Builders<BsonDocument>.Update
                .Push("closingPrices", new StockPrice(date, stockPriceHistory.GetPrice(date)));

            await _Collection.BulkWriteAsync(new[]
            {
                new UpdateOneModel<BsonDocument>(Builders<BsonDocument>.Filter.Eq("_id", stockPriceHistory.Id), removePrice),
                new UpdateOneModel<BsonDocument>(Builders<BsonDocument>.Filter.Eq("_id", stockPriceHistory.Id), addPrice),
            }); 
        }

        public async Task UpdatePricesAsync(StockPriceHistory stockPriceHistory, DateRange dateRange)
        {
            var removePrices = Builders<BsonDocument>.Update
                .PullFilter("closingPrices", Builders<BsonDocument>.Filter.And(new[]
                    {
                        Builders<BsonDocument>.Filter.Gte("date", dateRange.FromDate),
                        Builders<BsonDocument>.Filter.Lte("date", dateRange.ToDate)
                    }));

            var addPrices = Builders<BsonDocument>.Update
                .PushEach("closingPrices", stockPriceHistory.GetPrices(dateRange));

            await _Collection.BulkWriteAsync(new[]
            {
                new UpdateOneModel<BsonDocument>(Builders<BsonDocument>.Filter.Eq("_id", stockPriceHistory.Id), removePrices),
                new UpdateOneModel<BsonDocument>(Builders<BsonDocument>.Filter.Eq("_id", stockPriceHistory.Id), addPrices),
            });
        }

    }
}
