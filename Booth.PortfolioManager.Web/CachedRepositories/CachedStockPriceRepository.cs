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

        public async Task AddAsync(StockPriceHistory entity)
        {
            await _Repository.AddAsync(entity);
            _Cache.Add(entity);
        }

        public async IAsyncEnumerable<StockPriceHistory> AllAsync()
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

        public async Task<StockPriceHistory> GetAsync(Guid id)
        {
            var stockPriceHistory = _Cache.Get(id);
            if (stockPriceHistory != null)
                return stockPriceHistory;

            try
            {
                _Semphore.Wait();

                stockPriceHistory = _Cache.Get(id);
                if (stockPriceHistory == null)
                {

                    stockPriceHistory = await _Repository.GetAsync(id);
                    if (stockPriceHistory != null)
                        _Cache.Add(stockPriceHistory);
                }
            }
            finally
            {
                _Semphore.Release();
            }

            return stockPriceHistory;
        }

        public Task UpdateAsync(StockPriceHistory entity)
        {
            throw new NotImplementedException();
        }

        public async Task UpdatePriceAsync(StockPriceHistory stockPriceHistory, Date date)
        {
            await _Repository.UpdatePriceAsync(stockPriceHistory, date);
        }

        public async Task UpdatePricesAsync(StockPriceHistory stockPriceHistory, DateRange dateRange)
        {
            await _Repository.UpdatePricesAsync(stockPriceHistory, dateRange);
        }
    }
}
