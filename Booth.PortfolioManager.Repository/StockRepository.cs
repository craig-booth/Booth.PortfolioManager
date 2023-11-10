using System;
using System.Collections.Generic;
using System.Threading.Tasks;


using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using Booth.Common;
using Booth.PortfolioManager.Domain;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Repository.Serialization;
using System.Formats.Asn1;

namespace Booth.PortfolioManager.Repository
{
    public interface IStockRepository : IRepository<Stock>
    {
        Task UpdatePropertiesAsync(Stock stock, Date date);
        Task UpdateDividendRulesAsync(Stock stock, Date date);
        Task AddCorporateActionAsync(Stock stock, Guid id);
        Task DeleteCorporateActionAsync(Stock stock, Guid id);
        Task UpdateCorporateActionAsync(Stock stock, Guid id);
        Task UpdateRelativeNTAsAsync(Stock stock, Date date);
    }

    public class StockRepository : Repository<Stock>, IStockRepository
    {
        public StockRepository(IPortfolioManagerDatabase database)
            : base(database, "Stocks")
        {
        }

        public override async Task UpdateAsync(Stock entity)
        {
            var bson = Builders<BsonDocument>.Update
            .Set("listingDate", entity.EffectivePeriod.FromDate)
            .Set("trust", entity.Trust);

            if (entity.EffectivePeriod.ToDate != Date.MaxValue)
                bson.Set("delistingDate", entity.EffectivePeriod.ToDate);

            await _Collection.UpdateOneAsync(Builders<BsonDocument>.Filter.Eq("_id", entity.Id), bson);
        }

        public async Task UpdatePropertiesAsync(Stock stock, Date date)
        {
            await base.UpdateEffectivePropertiesAsync<StockProperties>(stock, date, stock.Properties[date], "properties");
        }

        public async Task UpdateDividendRulesAsync(Stock stock, Date date)
        {
            await base.UpdateEffectivePropertiesAsync<DividendRules>(stock, date, stock.DividendRules[date], "dividendRules");
        }

        public async Task AddCorporateActionAsync(Stock stock, Guid id)
        {
            var action = stock.CorporateActions[id];

            var addValue = Builders<BsonDocument>.Update
                .Push("corporateActions", action);

            await _Collection.UpdateOneAsync(Builders<BsonDocument>.Filter.Eq("_id", stock.Id), addValue);
        }

        public async Task DeleteCorporateActionAsync(Stock stock, Guid id)
        {
            var deleteValue = Builders<BsonDocument>.Update
                .Pull("corporateActions", Builders<BsonDocument>.Filter.Eq("corporateActions._id", id));

            await _Collection.UpdateOneAsync(Builders<BsonDocument>.Filter.Eq("_id", stock.Id), deleteValue);
        }

        public async Task UpdateCorporateActionAsync(Stock stock, Guid id)
        {
            var action = stock.CorporateActions[id];

            var filter = Builders<BsonDocument>.Filter
                .And(new[]
                    {
                    Builders<BsonDocument>.Filter.Eq("_id", stock.Id),
                    Builders<BsonDocument>.Filter.Eq("corporateActions._id", id)
                    }
                );
            
            var updateValue = Builders<BsonDocument>.Update
                .Set("corporateActions.$", action);

            await _Collection.UpdateOneAsync(filter, updateValue);
        }

        public async Task UpdateRelativeNTAsAsync(Stock stock, Date date)
        {
            var stapledSecurity = stock as StapledSecurity;
            if (stapledSecurity == null)
                throw new Exception("Can only update Relative NTAs on stapled securities");

            await base.UpdateEffectivePropertiesAsync<RelativeNTA>(stock, date, stapledSecurity.RelativeNTAs[date], "relativeNTAs");
        }

    }
}
