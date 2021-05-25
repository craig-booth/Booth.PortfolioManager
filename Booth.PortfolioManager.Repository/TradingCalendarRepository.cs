using System;
using System.Collections.Generic;
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
    }

    public class TradingCalendarRepository : ITradingCalendarRepository
    {

        private readonly IMongoCollection<BsonDocument> _Collection;
        public TradingCalendarRepository(IPortfolioManagerDatabase database)
        {
            _Collection = database.GetCollection("TradingCalendar");
        }

        public TradingCalendar Get(Guid id)
        {
            var bson = _Collection.Find(Builders<BsonDocument>.Filter.Eq("_id", id)).SingleOrDefault();
            if (bson == null)
                return null;

            var calendar = BsonSerializer.Deserialize<TradingCalendar>(bson);

            return calendar;
        }

        public IEnumerable<TradingCalendar> All()
        {
            var bsonElements = _Collection.Find("{}").ToList();

            foreach (var bson in bsonElements)
            {
                var calendar = BsonSerializer.Deserialize<TradingCalendar>(bson);

                yield return calendar;
            }
        }

        public void Add(TradingCalendar entity)
        {
            var bson = entity.ToBsonDocument();

            _Collection.InsertOne(bson);
        }

        public void Update(TradingCalendar entity)
        {
            throw new NotSupportedException();
        }

        public void Delete(Guid id)
        {
            _Collection.DeleteOne(Builders<BsonDocument>.Filter.Eq("_id", id));
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
                        { "days", calendar.NonTradingDays(year).ToBsonDocument()},
                });

            _Collection.BulkWrite(new[]
            {
                new UpdateOneModel<BsonDocument>(existsFilter, updateYear),
                new UpdateOneModel<BsonDocument>(notExistsFilter, addYear)
            });  
        }

        public static void ConfigureSerializaton()
        {
            BsonSerializer.RegisterSerializer<TradingCalendar>(new TradingCalendarSerializer());
        }

    }
}