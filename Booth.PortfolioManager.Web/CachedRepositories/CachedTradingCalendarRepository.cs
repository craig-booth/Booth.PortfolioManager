using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;

using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Domain.Portfolios;
using AngleSharp.Dom;
using System.Threading.Tasks;

namespace Booth.PortfolioManager.Web.CachedRepositories
{
    public class CachedTradingCalendarRepository : ITradingCalendarRepository
    {
        private readonly ITradingCalendarRepository _Repository;
        private readonly IMemoryCache _Cache;

        public CachedTradingCalendarRepository(ITradingCalendarRepository repository, IMemoryCache cache)
        {
            _Repository = repository;
            _Cache = cache;
        }

        public async Task AddAsync(TradingCalendar entity)
        {
            await _Repository.AddAsync(entity);
        }

        public IAsyncEnumerable<TradingCalendar> AllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _Repository.DeleteAsync(id);
            _Cache.Remove(id);
        }

        public Task<TradingCalendar> GetAsync(Guid id)
        {
            return _Cache.GetOrCreateAsync<TradingCalendar>(id, (cacheEntry) =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                return _Repository.GetAsync(id);
            });
        }

        public async Task UpdateAsync(TradingCalendar entity)
        {
            await _Repository.UpdateAsync(entity);
            _Cache.Remove(entity.Id);
        }

        public async Task UpdateYearAsync(TradingCalendar calendar, int year)
        {
            await _Repository.UpdateYearAsync(calendar, year);
            _Cache.Remove(calendar.Id);
        }
    }
}
