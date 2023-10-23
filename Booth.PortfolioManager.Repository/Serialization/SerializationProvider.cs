using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

using Booth.Common;
using Booth.PortfolioManager.Domain;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Utils;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.Domain.Transactions;
using MongoDB.Bson.Serialization.Serializers;

[assembly: InternalsVisibleTo("Booth.PortfolioManager.Repository.Test")]

namespace Booth.PortfolioManager.Repository.Serialization
{
    interface IConfigureSerialization
    {
        void ConfigureSerializaton();
    }

    class SerializationProvider : IBsonSerializationProvider
    {
        private readonly IPortfolioFactory _PortfolioFactory;
        private readonly IStockResolver _StockResolver;

        static bool _Registered = false;

        static SerializationProvider()
        {
            var conventions = new ConventionPack();
            conventions.Add(new CamelCaseElementNameConvention());
            ConventionRegistry.Register("PortfolioManager", conventions, t => t.Namespace.StartsWith("Booth.PortfolioManager.Domain"));

            var objectSerializer = new ObjectSerializer(type => ObjectSerializer.DefaultAllowedTypes(type) || type.Namespace.StartsWith("Booth.PortfolioManager.Domain"));
            BsonSerializer.RegisterSerializer(objectSerializer);

            // Corporate Actions
            BsonClassMap.RegisterClassMap<CorporateAction>(cm =>
            {
                cm.AutoMap();
                cm.UnmapProperty(c => c.Stock);
            });
            var actionTypes = typeof(CorporateAction).GetSubclassesOf(true);
            foreach (var actionType in actionTypes)
                BsonClassMap.LookupClassMap(actionType);

            // Transactions
            BsonClassMap.RegisterClassMap<PortfolioTransaction>();
            var transactionTypes = typeof(PortfolioTransaction).GetSubclassesOf(true);
            foreach (var transactionType in transactionTypes)
                BsonClassMap.LookupClassMap(transactionType);
        }
        protected SerializationProvider(IPortfolioFactory portfolioFactory, IStockResolver stockResolver) 
        {
            _PortfolioFactory = portfolioFactory;
            _StockResolver = stockResolver;
        }

        public static void Register(IPortfolioFactory portfolioFactory, IStockResolver stockResolver)
        {
            if (!_Registered)
            {
                BsonSerializer.RegisterSerializationProvider(new SerializationProvider(portfolioFactory, stockResolver));
                _Registered = true;
            }
        }

        public IBsonSerializer GetSerializer(Type type)
        {
            if (type == typeof(Date))
                return new DateSerializer();
            else if (type == typeof(Stock))
                return new StockSerializer();
            else if (type == typeof(StockPriceHistory))
                return new StockPriceHistorySerializer();
            else if (type == typeof(StockPrice))
                return new StockPriceSerializer();
            else if (type == typeof(TradingCalendar))
                return new TradingCalendarSerializer();
            else if (type == typeof(Portfolio))
                return new PortfolioSerializer(_PortfolioFactory);
            else if (type == typeof(IReadOnlyStock))
                return new PortfolioTransactionStockSerializer(_StockResolver);
            else if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IEffectiveProperties<>)))
            {
                var classType = typeof(EffectivePropertiesSerializer<>);
                var argumentType = type.GetGenericArguments()[0];
                var genericType = classType.MakeGenericType(argumentType);

                return (IBsonSerializer)Activator.CreateInstance(genericType);
            }
            else if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IEffectivePropertyValues<>)))
            {
                var classType = typeof(EffectivePropertyValuesSerializer<>);
                var argumentType = type.GetGenericArguments()[0];
                var genericType = classType.MakeGenericType(argumentType);

                return (IBsonSerializer)Activator.CreateInstance(genericType);
            }
            else if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(ITransactionList<>)))
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
