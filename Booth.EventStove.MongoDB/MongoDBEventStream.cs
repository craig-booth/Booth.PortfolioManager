using System;
using System.Collections.Generic;
using System.Text;

using MongoDB.Driver;

using Booth.EventStore;


namespace Booth.EventStore.MongoDB
{
    public class MongodbEventStream<T> :
       IEventStream,
       IEventStream<T>
    {
        public string Collection { get; private set; }

        private readonly string _ConnectionString;
        private readonly string _Database;

        public MongodbEventStream(string collection, string connectionString, string database)
        {
            Collection = collection;

            _ConnectionString = connectionString;
            _Database = database;
        }

        public StoredEntity Get(Guid entityId)
        {
            var client = new MongoClient(_ConnectionString);
            var database = client.GetDatabase(_Database);

            var collection = database.GetCollection<StoredEntity>(Collection);

            var entity = collection.Find(x => x.EntityId == entityId)?.Single();

            return entity;
        }

        public IEnumerable<StoredEntity> GetAll()
        {
            var result = new List<StoredEntity>();

            var client = new MongoClient(_ConnectionString);
            var database = client.GetDatabase(_Database);

            var collection = database.GetCollection<StoredEntity>(Collection);

            var entities = collection.Find("{}").ToList<StoredEntity>();

            return entities;
        }

        public StoredEntity FindFirst(string property, string value)
        {
            var client = new MongoClient(_ConnectionString);
            var database = client.GetDatabase(_Database);

            var collection = database.GetCollection<StoredEntity>(Collection);

            var filter = String.Format("{{Properties: {{{0}: '{1}'}}}}", property, value);
            var result = collection.Find(filter).Limit(1).ToList();
            if (result.Count == 0)
                return null;

            return result[0];
        }

        public IEnumerable<StoredEntity> Find(string property, string value)
        {
            var client = new MongoClient(_ConnectionString);
            var database = client.GetDatabase(_Database);

            var collection = database.GetCollection<StoredEntity>(Collection);

            var filter = String.Format("{{Properties: {{{0}: '{1}'}}}}", property, value);
            var result = collection.Find(filter).ToList();

            return result;
        }

        public void Add(Guid entityId, string type, IEnumerable<Event> events)
        {
            Add(entityId, type, null, events);
        }

        public void Add(Guid entityId, string type, IDictionary<string, string> properties, IEnumerable<Event> events)
        {
            var client = new MongoClient(_ConnectionString);
            var database = client.GetDatabase(_Database);

            var collection = database.GetCollection<StoredEntity>(Collection);

            var entity = new StoredEntity()
            {
                EntityId = entityId,
                Type = type
            };

            if (properties != null)
            {
                foreach (var item in properties)
                    entity.Properties.Add(item.Key, item.Value);
            }

            entity.Events.AddRange(events);

            collection.InsertOne(entity);
        }

        public void UpdateProperties(Guid entityId, IDictionary<string, string> properties)
        {
            var client = new MongoClient(_ConnectionString);
            var database = client.GetDatabase(_Database);

            var collection = database.GetCollection<StoredEntity>(Collection);

            collection.FindOneAndUpdate<StoredEntity>(x => x.EntityId == entityId,
                Builders<StoredEntity>.Update.Set<Dictionary<string, string>>(x => x.Properties, (Dictionary<string, string>)properties));
        }

        public void AppendEvent(Guid entityId, Event @event)
        {
            var client = new MongoClient(_ConnectionString);
            var database = client.GetDatabase(_Database);

            var collection = database.GetCollection<StoredEntity>(Collection);

            collection.FindOneAndUpdate<StoredEntity>(x => x.EntityId == entityId,
                Builders<StoredEntity>.Update.Push<Event>(x => x.Events, @event));
        }

        public void AppendEvents(Guid entityId, IEnumerable<Event> events)
        {
            var client = new MongoClient(_ConnectionString);
            var database = client.GetDatabase(_Database);

            var collection = database.GetCollection<StoredEntity>(Collection);

            collection.FindOneAndUpdate<StoredEntity>(x => x.EntityId == entityId,
                Builders<StoredEntity>.Update.PushEach<Event>(x => x.Events, events));
        }

    }
}
