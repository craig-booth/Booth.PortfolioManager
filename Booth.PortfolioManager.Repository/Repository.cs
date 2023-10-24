﻿using System;
using System.Linq.Expressions;
using System.Collections.Generic;

using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using Booth.Common;
using Booth.PortfolioManager.Domain;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Operations;

namespace Booth.PortfolioManager.Repository
{
    public interface IRepository<T> where T : IEntity
    {
        T Get(Guid id);
        IEnumerable<T> All();
        void Add(T entity);
        void Update(T entity);
        void Delete(Guid id);

        Task<T> GetAsync(Guid id);
        IAsyncEnumerable<T> AllAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
    }

    public abstract class Repository<T> : IRepository<T> where T : IEntity
    {
        protected readonly IPortfolioManagerDatabase _Database;
        protected readonly IMongoCollection<BsonDocument> _Collection;
        protected Repository(IPortfolioManagerDatabase database, string collectionName)
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

        public async virtual Task<T> GetAsync(Guid id)
        {
            var bson = await _Collection.Find(Builders<BsonDocument>.Filter.Eq("_id", id)).SingleOrDefaultAsync();
            if (bson == null)
                return default(T);

            var entity = BsonSerializer.Deserialize<T>(bson);

            return entity;
        }

        public virtual T FindFirst(string property, string value)
        {
            var bson = _Collection.Find(Builders<BsonDocument>.Filter.Eq(property, value)).SingleOrDefault();
            if (bson == null)
                return default(T);

            var entity = BsonSerializer.Deserialize<T>(bson);

            return entity;
        }
        public async virtual Task<T> FindFirstAsync(string property, string value)
        {
            var bson = await _Collection.Find(Builders<BsonDocument>.Filter.Eq(property, value)).SingleOrDefaultAsync();
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

        public async virtual IAsyncEnumerable<T> AllAsync()
        {
            using (var asyncCursor = await _Collection.FindAsync("{}"))
            {
                while (await asyncCursor.MoveNextAsync())
                {
                    foreach (var bson in asyncCursor.Current)
                    {
                        var entity = BsonSerializer.Deserialize<T>(bson);

                        yield return entity;
                    }
                }
            }
        }
 

        public virtual void Add(T entity)
        {
            var bson = entity.ToBsonDocument();

            _Collection.InsertOne(bson);
        }
        public async virtual Task AddAsync(T entity)
        {
            var bson = entity.ToBsonDocument();

            await _Collection.InsertOneAsync(bson);
        }

        public virtual void Update(T entity)
        {
            var bson = entity.ToBsonDocument();

            _Collection.ReplaceOne(Builders<BsonDocument>.Filter.Eq("_id", entity.Id), bson);
        }

        public async virtual Task UpdateAsync(T entity)
        {
            var bson = entity.ToBsonDocument();

            await _Collection.ReplaceOneAsync(Builders<BsonDocument>.Filter.Eq("_id", entity.Id), bson);
        }

        public virtual void Delete(Guid id)
        {
            _Collection.DeleteOne(Builders<BsonDocument>.Filter.Eq("_id", id));
        }

        public async virtual Task DeleteAsync(Guid id)
        {
            await _Collection.DeleteOneAsync(Builders<BsonDocument>.Filter.Eq("_id", id));
        }


        protected void UpdateEffectiveProperties<P>(T entity, Date date, P property, string propertyName) where P: struct
        {
            var existsFilter = Builders<BsonDocument>.Filter
                .And(new[]
                    {
                    Builders<BsonDocument>.Filter.Eq("_id", entity.Id),
                    Builders<BsonDocument>.Filter.Eq(propertyName + ".date", date)
                    }
                );


            var updateValue = Builders<BsonDocument>.Update
                 .Set(propertyName + ".$.properties", property);

            var notExistsFilter = Builders<BsonDocument>.Filter
                .And(new[]
                    {
                    Builders<BsonDocument>.Filter.Eq("_id", entity.Id),
                    Builders<BsonDocument>.Filter.Ne(propertyName + ".date", date)
                    }
                );

            var addValue = Builders<BsonDocument>.Update
                .Push(propertyName, property);

            _Collection.BulkWrite(new[]
             {
                new UpdateOneModel<BsonDocument>(existsFilter, updateValue),
                new UpdateOneModel<BsonDocument>(notExistsFilter, addValue)
            });
        }

        protected Task UpdateEffectivePropertiesAsync<P>(T entity, Date date, P property, string propertyName) where P: struct
        {
            throw new NotImplementedException();
        }
    }
}
