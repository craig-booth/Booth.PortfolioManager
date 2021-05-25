using System;
using System.Collections.Generic;
using System.Globalization;

using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

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

    public abstract class Repository<T> : IRepository<T> where T : IEntity
    {
        protected readonly IPortfolioManagerDatabase _Database;
        protected readonly IMongoCollection<BsonDocument> _Collection;
        public Repository(IPortfolioManagerDatabase database, string collectionName)
        {
            _Database = database;
            _Collection = database.GetCollection(collectionName);
        }
        public virtual T Get(Guid id)
        {
            var bson = _Collection.Find(Builders<BsonDocument>.Filter.Eq("_id", id)).SingleOrDefault();
            if (bson == null)
                return default(T);

            var entity = BsonSerializer.Deserialize<T>(bson);

            return entity;
        }

        public virtual IEnumerable<T> All()
        {
            var bsonElements = _Collection.Find("{}").ToList();

            foreach (var bson in bsonElements)
            {
                var entity = BsonSerializer.Deserialize<T>(bson);

                yield return entity;
            }
        }

        public virtual void Add(T entity)
        {
            var bson = entity.ToBsonDocument();

            _Collection.InsertOne(bson);
        }

        public virtual void Update(T entity)
        {
            var bson = entity.ToBsonDocument();

            _Collection.ReplaceOne(Builders<BsonDocument>.Filter.Eq("_id", entity.Id), bson);
        }

        public virtual void Delete(Guid id)
        {
            _Collection.DeleteOne(Builders<BsonDocument>.Filter.Eq("_id", id));
        }
    }
}
