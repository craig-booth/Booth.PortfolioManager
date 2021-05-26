using System;
using System.Collections.Generic;
using System.Text;

using MongoDB.Driver;
using MongoDB.Bson;

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
        private readonly IMongoDatabase _Database;
        public PortfolioManagerDatabase(string connectionString, string databaseName, IPortfolioFactory portfolioFactory, IStockResolver stockResolver)
        {
            var client = new MongoClient(connectionString);
            _Database = client.GetDatabase(databaseName);

            SerializationProvider.Configure(portfolioFactory, stockResolver);
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
