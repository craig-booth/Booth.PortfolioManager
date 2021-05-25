using System;
using System.Collections.Generic;
using System.Linq;

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

using Booth.Common;
using Booth.EventStore;
using Booth.PortfolioManager.Domain;
using Booth.PortfolioManager.Domain.Utils;


namespace Booth.PortfolioManager.Repository.Serialization
{
    class SerializationProvider : IBsonSerializationProvider
    {

        public static void Configure()
        {
            var conventions = new ConventionPack();
            conventions.Add(new CamelCaseElementNameConvention());
         //   conventions.Add(new ImmutableTypeClassMapConvention());
            ConventionRegistry.Register("PortfolioManager", conventions, t => true);



            BsonSerializer.RegisterSerializationProvider(new SerializationProvider());
            BsonSerializer.RegisterSerializer(typeof(Date), new DateSerializer());


        /*    BsonClassMap.RegisterClassMap<TrackedEntity>(cm =>
            {
                cm.MapIdField(c => c.Id);
            });  */

            StockRepository.ConfigureSerializaton();
            TradingCalendarRepository.ConfigureSerializaton();
        }

        public IBsonSerializer GetSerializer(Type type)
        {
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IEffectiveProperties<>)))
            {
                var classType = typeof(EffectivePropertiesSerializer<>);
                var argumentType = type.GetGenericArguments()[0];
                var genericType = classType.MakeGenericType(argumentType);

                return (IBsonSerializer)Activator.CreateInstance(genericType);
            }

            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IEffectivePropertyValues<>)))
            {
                var classType = typeof(EffectivePropertyValuesSerializer<>);
                var argumentType = type.GetGenericArguments()[0];
                var genericType = classType.MakeGenericType(argumentType);

                return (IBsonSerializer)Activator.CreateInstance(genericType);
            }

            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(ITransactionList<>)))
            {
                var classType = typeof(TransactionListSerializer<>);
                var argumentType = type.GetGenericArguments()[0];
                var genericType = classType.MakeGenericType(argumentType);

                return (IBsonSerializer)Activator.CreateInstance(genericType);
            }

            return null;
        }
    }
}
