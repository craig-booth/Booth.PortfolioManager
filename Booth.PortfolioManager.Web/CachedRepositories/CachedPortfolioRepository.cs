using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;

using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Repository;
using System.Threading.Tasks;

namespace Booth.PortfolioManager.Web.CachedRepositories
{
    public class CachedPortfolioRepository : IPortfolioRepository
    {
        private readonly IPortfolioRepository _Repository;
        private readonly IMemoryCache _Cache;

        public CachedPortfolioRepository(IPortfolioRepository repository, IMemoryCache cache) 
        { 
            _Repository = repository;
            _Cache = cache;
        }

        public async Task AddAsync(Portfolio entity)
        {
            await _Repository.AddAsync(entity);
        }

        public async Task AddTransactionAsync(Portfolio portfolio, Guid id)
        {
            await _Repository.AddTransactionAsync(portfolio, id);
            _Cache.Remove(portfolio.Id);
        }

        public IAsyncEnumerable<Portfolio> AllAsync()
        {
            return _Repository.AllAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _Repository.DeleteAsync(id);
            _Cache.Remove(id);
        }

        public async Task DeleteTransactionAsync(Portfolio portfolio, Guid id)
        {
            await _Repository.DeleteTransactionAsync(portfolio, id);
            _Cache.Remove(portfolio.Id);
        }

        public Task<Portfolio> GetAsync(Guid id)
        {
            return _Cache.GetOrCreateAsync<Portfolio>(id, (cacheEntry) =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return _Repository.GetAsync(id);
            });
        }

        public async Task UpdateAsync(Portfolio entity)
        {
            await _Repository.UpdateAsync(entity);
            _Cache.Remove(entity.Id);
        }

        public async Task UpdateTransactionAsync(Portfolio portfolio, Guid id)
        {
            await _Repository.UpdateTransactionAsync(portfolio, id);
            _Cache.Remove(portfolio.Id);
        }
    }
}
