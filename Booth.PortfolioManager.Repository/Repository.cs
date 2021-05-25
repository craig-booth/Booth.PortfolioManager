using System;
using System.Collections.Generic;
using System.Globalization;

using MongoDB.Driver;
using MongoDB.Bson;

using Booth.Common;
using Booth.EventStore;


namespace Booth.PortfolioManager.Repository
{
    public interface IRepository<T> where T : IEntity
    {
        T Get(Guid id);
        IEnumerable<T> All();
        void Add(T entity);
        void Update(T entity);
        void Delete(Guid id);
    }

    static class BsonSerializaton
    {
        public static Date ToDate(this BsonValue bsonValue)
        {
            if (bsonValue.BsonType == BsonType.String)
            {
                var value = bsonValue.AsString;
                if (string.IsNullOrWhiteSpace(value))
                {
                    return Date.MinValue;
                }
                if (Date.TryParseExact(value, "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out Date date))
                    return date;
                else
                    return Date.MinValue;
            }
            else if (bsonValue.BsonType == BsonType.DateTime)
            {
                var value = bsonValue.AsInt64;
                var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(value).DateTime;

                return new Date(dateTime);
            }
            else if (bsonValue.BsonType == BsonType.Timestamp)
            {
                var value = bsonValue.AsInt64;
                var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(value).DateTime;

                return new Date(dateTime);
            }
            else
            {
                return Date.MinValue;
            }
        }

        public static BsonValue ToBsonValue(this Date date)
        {
            return new BsonString(date.ToString("yyyy-MM-dd"));
        }
    }

    public abstract class Repository<T> : IRepository<T> where T : IEntity
    {
        protected readonly IPortfolioManagerDatabase _Database;
        protected readonly IMongoCollection<BsonDocument> _Collection;
        public Repository(IPortfolioManagerDatabase database, string collectionName)
        {
            _Database = database;
            _Collection = database.GetCollection(collectionName);
        }

        protected abstract T CreateFromBson(BsonDocument document);

        protected abstract BsonDocument InsertBson(T entity);

        protected abstract UpdateDefinition<BsonDocument> UpdateBson(T entity);

        public T Get(Guid id)
        {
            var bson = _Collection.Find(Builders<BsonDocument>.Filter.Eq("_id", id.ToString())).SingleOrDefault();
            if (bson == null)
                return default(T);

            return CreateFromBson(bson);
        }

        public IEnumerable<T> All()
        {
            var bsonElements = _Collection.Find("{}").ToList();

            foreach (var bson in bsonElements)
            {
                yield return CreateFromBson(bson);
            }
        }

        public void Add(T entity)
        {
            var bson = InsertBson(entity);

            _Collection.InsertOne(bson);
        }

        public void Update(T entity)
        {
            var bson = UpdateBson(entity);

            _Collection.UpdateOne(Builders<BsonDocument>.Filter.Eq("_id", entity.Id.ToString()), bson);
        }

        public void Delete(Guid id)
        {
            _Collection.DeleteOne(Builders<BsonDocument>.Filter.Eq("_id", id.ToString()));
        }
    }
}
