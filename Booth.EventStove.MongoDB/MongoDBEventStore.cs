using System;
using System.Collections.Generic;
using System.Text;

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

using Booth.Common;

namespace Booth.EventStore.MongoDB
{
    public class MongodbEventStore : IEventStore
    {
        private readonly string _ConnectionString;
        private readonly string _Database;

        public MongodbEventStore(string connectionString, string database)
        {
            _ConnectionString = connectionString;
            _Database = database;

            EventStoreSerializers.Register();
        }

        public IEventStream GetEventStream(string collection)
        {
            return GetEventStream<object>(collection);
        }

        public IEventStream<T> GetEventStream<T>(string streamName)
        {
            var eventStream = new MongodbEventStream<T>(streamName, _ConnectionString, _Database);
            return (IEventStream<T>)eventStream;
        }
    }

    static class EventStoreSerializers
    {
        private static bool _Registered = false;

        public static void Register()
        {
            if (_Registered)
                return;

            _Registered = true;

            BsonSerializer.RegisterSerializer(typeof(Date), new DateSerializer());
            var conventionPack = new ConventionPack()
            {
                new IgnoreExtraElementsConvention(true),
            };
            ConventionRegistry.Register("Booth.EventStore.StoredEntity", conventionPack, t => (t == typeof(StoredEntity)) || t.IsSubclassOf(typeof(Event)));

            /*   BsonSerializer.RegisterSerializer(typeof(DateTime), new DateOnlySerializer());
               var conventionPack = new ConventionPack()
               {
                   new IgnoreExtraElementsConvention(true),
               };
               ConventionRegistry.Register("PortfolioManager.Events", conventionPack, t => (t == typeof(StoredEntity)) || t.IsSubclassOf(typeof(Event)));

               var eventTypes = typeof(Event).GetSubclassesOf(true);
               foreach (var eventType in eventTypes)
                   BsonClassMap.LookupClassMap(eventType); */
        }
    } 
}
