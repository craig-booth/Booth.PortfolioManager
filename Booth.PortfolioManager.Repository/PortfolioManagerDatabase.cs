using System;
using System.Collections.Generic;
using System.Text;

using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Repository.Serialization;

namespace Booth.PortfolioManager.Repository
{

    public interface IPortfolioManagerDatabase
    {
        IMongoCollection<BsonDocument> GetCollection(string name);
        IMongoCollection<T> GetCollection<T>(string name);
    }

    public class PortfolioManagerDatabase : IPortfolioManagerDatabase
    {
        private readonly IMongoClient _Client;
        private IMongoDatabase _Database;

        public PortfolioManagerDatabase(IMongoClient mongoClient, string databaseName, IPortfolioFactory portfolioFactory, IStockResolver stockResolver)
        {
            _Client = mongoClient;
            _Database = mongoClient.GetDatabase(databaseName);

            SerializationProvider.Register(portfolioFactory, stockResolver);
        }

        public IMongoCollection<BsonDocument> GetCollection(string name)
        {
            return _Database.GetCollection<BsonDocument>(name);
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _Database.GetCollection<T>(name);
        }
    }
}
