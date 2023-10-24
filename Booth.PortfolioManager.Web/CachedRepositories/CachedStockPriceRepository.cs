using System;
using System.Collections.Generic;

using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Web.Utilities;
using Booth.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Booth.PortfolioManager.Web.CachedRepositories
{
    public class CachedStockPriceRepository : IStockPriceRepository
    {
        private readonly IStockPriceRepository _Repository;
        private readonly IEntityCache<StockPriceHistory> _Cache;
        private readonly SemaphoreSlim _Semphore = new SemaphoreSlim(1, 1);

        public CachedStockPriceRepository(IStockPriceRepository repository, IEntityCache<StockPriceHistory> cache)
        {
            _Repository = repository;
            _Cache = cache;
        }

        public void Add(StockPriceHistory entity)
        {
            _Repository.Add(entity);
            _Cache.Add(entity);
        }

        public Task AddAsync(StockPriceHistory entity)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StockPriceHistory> All()
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

        public IAsyncEnumerable<StockPriceHistory> AllAsync()
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

        public StockPriceHistory Get(Guid id)
        {
            return _Cache.Get(id);
        }

        public Task<StockPriceHistory> GetAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Update(StockPriceHistory entity)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(StockPriceHistory entity)
        {
            throw new NotImplementedException();
        }

        public void UpdatePrice(StockPriceHistory stockPriceHistory, Date date)
        {
            _Repository.UpdatePrice(stockPriceHistory, date);
        }

        public Task UpdatePriceAsync(StockPriceHistory stockPriceHistory, Date date)
        {
            throw new NotImplementedException();
        }

        public void UpdatePrices(StockPriceHistory stockPriceHistory, DateRange dateRange)
        {
            _Repository.UpdatePrices(stockPriceHistory, dateRange);
        }

        public Task UpdatePricesAsync(StockPriceHistory stockPriceHistory, DateRange dateRange)
        {
            throw new NotImplementedException();
        }
    }
}
