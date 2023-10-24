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
        private readonly SemaphoreSlim _Semphore = new SemaphoreSlim(1, 1);

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
            if (_Cache.TryGetValue(id, out Portfolio entity))
                return entity;

            try
            {
                _Semphore.Wait();
                if (_Cache.TryGetValue(id, out entity))
                    return entity;

                entity = _Repository.Get(id);
                if (entity != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                    _Cache.Set(id, entity, cacheEntryOptions);
                }
            }
            finally
            {
                _Semphore.Release();
            }

            return entity;
        }

        public async Task<Portfolio> GetAsync(Guid id)
        {
            if (_Cache.TryGetValue(id, out Portfolio entity))
                return entity;

            try
            {
                _Semphore.Wait();
                if (_Cache.TryGetValue(id, out entity))
                    return entity;

                entity = await _Repository.GetAsync(id);
                if (entity != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                    _Cache.Set(id, entity, cacheEntryOptions);
                }
            }
            finally
            {
                _Semphore.Release();
            }

            return entity;
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
