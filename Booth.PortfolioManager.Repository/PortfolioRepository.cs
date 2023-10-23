using System;
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
using System.Transactions;
using MongoDB.Bson.IO;

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
            var drpHoldings = entity.Holdings.All().Where(x => x.Settings.ParticipateInDrp).Select(x => x.Id).ToArray();

            var bson = Builders<BsonDocument>.Update
            .Set("name", entity.Name)
            .Set("owner", entity.Owner)
            .Set("participateInDrp", drpHoldings);

            _Collection.UpdateOne(Builders<BsonDocument>.Filter.Eq("_id", entity.Id), bson);
        }

        public void AddTransaction(Portfolio portfolio, Guid id)
        {
            var transaction = portfolio.Transactions[id];

            var addValue = Builders<BsonDocument>.Update
                .Push("transactions", transaction);

            _Collection.UpdateOne(Builders<BsonDocument>.Filter.Eq("_id", portfolio.Id), addValue); 
        }

        public void DeleteTransaction(Portfolio portfolio, Guid id)
        {
            var updateValue = Builders<BsonDocument>.Update
                .PullFilter("transactions", Builders<BsonDocument>.Filter.Eq("_id", id));

            _Collection.UpdateOne(Builders<BsonDocument>.Filter.Eq("_id", portfolio.Id), updateValue);
        }

        public void UpdateTransaction(Portfolio portfolio, Guid id)
        {
            var transaction = portfolio.Transactions[id];

            var filter = Builders<BsonDocument>.Filter
                .And(Builders<BsonDocument>.Filter.Eq("_id", portfolio.Id), Builders<BsonDocument>.Filter.Eq("transactions._id", id));

            var updateValue = Builders<BsonDocument>.Update
                .Set("transactions.$", transaction);

            _Collection.UpdateOne(filter, updateValue); 
        }

    }
}
