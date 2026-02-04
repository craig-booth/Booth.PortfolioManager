using System;
using System.Collections.Generic;
 
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.Common;
using Booth.PortfolioManager.Web.Utilities;
using System.Threading;
using Booth.PortfolioManager.Domain.Portfolios;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace Booth.PortfolioManager.Web.CachedRepositories
{
    public class CachedStockRepository : IStockRepository
    {
        private readonly IStockRepository _Repository;
        private readonly IEntityCache<Stock> _Cache;
        private readonly SemaphoreSlim _Semphore = new SemaphoreSlim(1, 1);

        public CachedStockRepository(IStockRepository repository, IEntityCache<Stock> cache)
        {
            _Repository = repository;
            _Cache = cache;
        }

        public async Task AddAsync(Stock entity)
        {
            await _Repository.AddAsync(entity);
            _Cache.Add(entity);
        }

        public async Task AddCorporateActionAsync(Stock stock, Guid id)
        {
            await _Repository.AddCorporateActionAsync(stock, id);
        }

        public async IAsyncEnumerable<Stock> AllAsync()
        {
            // If cache is empty then load it first
            if (_Cache.Count == 0)
            {
                try
                {
                    _Semphore.Wait();
                    if (_Cache.Count == 0)
                    {
                        var entities = _Repository.AllAsync();
                        await foreach (var entity in entities)
                            _Cache.Add(entity);
                    }
                }
                finally
                {
                    _Semphore.Release();
                }
            }

            foreach (var entity in _Cache.All())
                yield return entity;
        }

        public async Task DeleteAsync(Guid id)
        {
            await _Repository.DeleteAsync(id);
            _Cache.Remove(id);
        }

        public async Task DeleteCorporateActionAsync(Stock stock, Guid id)
        {
            await _Repository.DeleteCorporateActionAsync(stock, id);
        }

        public Task<Stock> GetAsync(Guid id)
        {
            return Task.FromResult<Stock>(_Cache.Get(id));
        }

        public async Task UpdateAsync(Stock entity)
        {
            await _Repository.UpdateAsync(entity);
        }

        public async Task UpdateCorporateActionAsync(Stock stock, Guid id)
        {
            await _Repository.UpdateCorporateActionAsync(stock, id);
        }

        public async Task UpdateDividendRulesAsync(Stock stock, Date date)
        {
            await _Repository.UpdateDividendRulesAsync(stock, date);
        }

        public async Task UpdatePropertiesAsync(Stock stock, Date date)
        {
            await _Repository.UpdatePropertiesAsync(stock, date);
        }

    }
}
