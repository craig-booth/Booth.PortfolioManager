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

        public void Add(Portfolio entity)
        {
            _Repository.Add(entity);
        }

        public Task AddAsync(Portfolio entity)
        {
            throw new NotImplementedException();
        }

        public void AddTransaction(Portfolio portfolio, Guid id)
        {
            _Repository.AddTransaction(portfolio, id);
            _Cache.Remove(portfolio.Id);  
        }

        public Task AddTransactionAsync(Portfolio portfolio, Guid id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Portfolio> All()
        {
            return _Repository.All();
        }

        public IAsyncEnumerable<Portfolio> AllAsync()
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            _Repository.Delete(id);
            _Cache.Remove(id);
        }

        public Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public void DeleteTransaction(Portfolio portfolio, Guid id)
        {
            _Repository.DeleteTransaction(portfolio, id);
            _Cache.Remove(portfolio.Id);
        }

        public Task DeleteTransactionAsync(Portfolio portfolio, Guid id)
        {
            throw new NotImplementedException();
        }

        public Portfolio Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Portfolio> GetAsync(Guid id)
        {
            return _Cache.GetOrCreateAsync<Portfolio>(id, (cacheEntry) =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return _Repository.GetAsync(id);
            });
        }

        public void Update(Portfolio entity)
        {
            _Repository.Update(entity);
            _Cache.Remove(entity.Id);
        }

        public Task UpdateAsync(Portfolio entity)
        {
            throw new NotImplementedException();
        }

        public void UpdateTransaction(Portfolio portfolio, Guid id)
        {
            _Repository.UpdateTransaction(portfolio, id);
            _Cache.Remove(portfolio.Id);
        }

        public Task UpdateTransactionAsync(Portfolio portfolio, Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
