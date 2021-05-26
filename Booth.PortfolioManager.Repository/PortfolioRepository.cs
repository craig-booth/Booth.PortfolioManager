﻿using System;
using System.Collections.Generic;
using System.Linq;


using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using Booth.Common;
using Booth.PortfolioManager.Domain;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Repository.Serialization;

namespace Booth.PortfolioManager.Repository
{
    public interface IPortfolioRepository : IRepository<Portfolio>
    {
        void AddTransaction(Portfolio portfolio, Guid id);
        void DeleteTransaction(Portfolio portfolio, Guid id);
        void UpdateTransaction(Portfolio portfolio, Guid id);

    }

    public class PortfolioRepository : Repository<Portfolio>, IPortfolioRepository
    {
        public PortfolioRepository(IPortfolioManagerDatabase database)
            : base(database, "Portfolios")
        {
        }

        public override void Update(Portfolio entity)
        {
            var bson = Builders<BsonDocument>.Update
            .Set("name", entity.Name)
            .Set("owner", entity.Owner);

            _Collection.UpdateOne(Builders<BsonDocument>.Filter.Eq("_id", entity.Id), bson);
        }

        public void AddTransaction(Portfolio portfolio, Guid id)
        {
            throw new NotSupportedException();
        }

        public void DeleteTransaction(Portfolio portfolio, Guid id)
        {
            throw new NotSupportedException();
        }

        public void UpdateTransaction(Portfolio portfolio, Guid id)
        {
            throw new NotSupportedException();
        }

        public static void ConfigureSerializaton(IPortfolioFactory factory, IStockResolver stockResolver)
        {
            BsonSerializer.RegisterSerializer<Portfolio>(new PortfolioSerializer(factory));

            BsonClassMap.RegisterClassMap<PortfolioTransaction>(cm =>
            {
                cm.AutoMap();
                // This won't be needed if PortfolioTransaction can contain the AsxCode
                cm.MapProperty(c => c.Stock).SetSerializer(new PortfolioTransactionStockSerializer(stockResolver));
            });

            var transactionTypes = typeof(PortfolioTransaction).GetSubclassesOf(true);
            foreach (var transactionType in transactionTypes)
                BsonClassMap.LookupClassMap(transactionType);
        }
    }
}
