using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


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
        Task AddTransactionAsync(Portfolio portfolio, Guid id);
        Task DeleteTransactionAsync(Portfolio portfolio, Guid id);
        Task UpdateTransactionAsync(Portfolio portfolio, Guid id);

    }

    public class PortfolioRepository : Repository<Portfolio>, IPortfolioRepository
    {
        public PortfolioRepository(IPortfolioManagerDatabase database)
            : base(database, "Portfolios")
        {
        }

        public override async Task UpdateAsync(Portfolio entity)
        {
            var drpHoldings = entity.Holdings.All().Where(x => x.Settings.ParticipateInDrp).Select(x => x.Id).ToArray();

            var bson = Builders<BsonDocument>.Update
            .Set("name", entity.Name)
            .Set("owner", entity.Owner)
            .Set("participateInDrp", drpHoldings);

            await _Collection.UpdateOneAsync(Builders<BsonDocument>.Filter.Eq("_id", entity.Id), bson);
        }

        public async Task AddTransactionAsync(Portfolio portfolio, Guid id)
        {
            var transaction = portfolio.Transactions[id];

            var addValue = Builders<BsonDocument>.Update
                .Push("transactions", transaction);

            await _Collection.UpdateOneAsync(Builders<BsonDocument>.Filter.Eq("_id", portfolio.Id), addValue);
        }


        public async Task DeleteTransactionAsync(Portfolio portfolio, Guid id)
        {
            var updateValue = Builders<BsonDocument>.Update
                .PullFilter("transactions", Builders<BsonDocument>.Filter.Eq("_id", id));

            await _Collection.UpdateOneAsync(Builders<BsonDocument>.Filter.Eq("_id", portfolio.Id), updateValue);
        }

        public async Task UpdateTransactionAsync(Portfolio portfolio, Guid id)
        {
            var transaction = portfolio.Transactions[id];

            var filter = Builders<BsonDocument>.Filter
                .And(Builders<BsonDocument>.Filter.Eq("_id", portfolio.Id), Builders<BsonDocument>.Filter.Eq("transactions._id", id));

            var updateValue = Builders<BsonDocument>.Update
                .Set("transactions.$", transaction);

            await _Collection.UpdateOneAsync(filter, updateValue);
        }
    }
}
