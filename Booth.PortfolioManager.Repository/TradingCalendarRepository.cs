using System;
using System.Threading.Tasks;
using System.Linq;


using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using Booth.Common;
using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.Repository.Serialization;

namespace Booth.PortfolioManager.Repository
{

    public interface ITradingCalendarRepository : IRepository<TradingCalendar>
    {
        void UpdateYear(TradingCalendar calendar, int year);
        Task UpdateYearAsync(TradingCalendar calendar, int year);
    }

    public class TradingCalendarRepository : Repository<TradingCalendar>, ITradingCalendarRepository
    {
        public TradingCalendarRepository(IPortfolioManagerDatabase database)
            :base(database, "TradingCalendar")
        {
        }
        public override void Update(TradingCalendar entity)
        {
            throw new NotSupportedException();
        }

        public override Task UpdateAsync(TradingCalendar entity)
        {
            throw new NotSupportedException();
        }

        public void UpdateYear(TradingCalendar calendar, int year)
        {
            var existsFilter = Builders<BsonDocument>.Filter
                .And(new[]
                    {
                    Builders<BsonDocument>.Filter.Eq("_id", calendar.Id),
                    Builders<BsonDocument>.Filter.Eq("years.year", year)
                    }
                );

            var updateYear = Builders<BsonDocument>.Update
                .Set("years.$.days", calendar.NonTradingDays(year));

            var notExistsFilter = Builders<BsonDocument>.Filter
                .And(new[]
                    {
                        Builders<BsonDocument>.Filter.Eq("_id", calendar.Id),
                        Builders<BsonDocument>.Filter.Ne("years.year", year)
                    }
                );

            var addYear = Builders<BsonDocument>.Update
                .Push("years", new BsonDocument()
                    {
                        { "year", year},
                        { "days", new BsonArray(calendar.NonTradingDays(year).Select(x => x.ToBsonDocument()))},
                }); 

            _Collection.BulkWrite(new[]
            {
                new UpdateOneModel<BsonDocument>(existsFilter, updateYear),
                new UpdateOneModel<BsonDocument>(notExistsFilter, addYear)
            });  
        }

        public Task UpdateYearAsync(TradingCalendar calendar, int year)
        {
            throw new NotImplementedException();
        }
    }
}