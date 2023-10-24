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

        public void Add(Stock entity)
        {
            _Repository.Add(entity);
            _Cache.Add(entity);
        }

        public Task AddAsync(Stock entity)
        {
            throw new NotImplementedException();
        }

        public void AddCorporateAction(Stock stock, Guid id)
        {
            _Repository.AddCorporateAction(stock, id);
        }

        public Task AddCorporateActionAsync(Stock stock, Guid id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Stock> All()
        {
            // If cache is empty then load it first
            if (_Cache.Count == 0)
            {
                try
                {
                    _Semphore.Wait();
                    if (_Cache.Count == 0)
                    {
                        var entities = _Repository.All();
                        foreach (var entity in entities)
                            _Cache.Add(entity);
                    }
                }
                finally
                {
                    _Semphore.Release();
                }

            }

            return _Cache.All();
        }

        public IAsyncEnumerable<Stock> AllAsync()
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

        public void DeleteCorporateAction(Stock stock, Guid id)
        {
            _Repository.DeleteCorporateAction(stock, id);
        }

        public Task DeleteCorporateActionAsync(Stock stock, Guid id)
        {
            throw new NotImplementedException();
        }

        public Stock Get(Guid id)
        {
            return _Cache.Get(id);
        }

        public Task<Stock> GetAsync(Guid id)
        {
            return Task.FromResult<Stock>(_Cache.Get(id));
        }

        public void Update(Stock entity)
        {
            _Repository.Update(entity);
        }

        public Task UpdateAsync(Stock entity)
        {
            throw new NotImplementedException();
        }

        public void UpdateCorporateAction(Stock stock, Guid id)
        {
            _Repository.UpdateCorporateAction(stock, id);
        }

        public Task UpdateCorporateActionAsync(Stock stock, Guid id)
        {
            throw new NotImplementedException();
        }

        public void UpdateDividendRules(Stock stock, Date date)
        {
            _Repository.UpdateDividendRules(stock, date);
        }

        public Task UpdateDividendRulesAsync(Stock stock, Date date)
        {
            throw new NotImplementedException();
        }

        public void UpdateProperties(Stock stock, Date date)
        {
            _Repository.UpdateProperties(stock, date);
        }

        public Task UpdatePropertiesAsync(Stock stock, Date date)
        {
            throw new NotImplementedException();
        }

        public void UpdateRelativeNTAs(Stock stock, Date date)
        {
            _Repository.UpdateRelativeNTAs(stock, date);
        }

        public Task UpdateRelativeNTAsAsync(Stock stock, Date date)
        {
            throw new NotImplementedException();
        }
    }
}
