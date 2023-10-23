using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;

using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Domain.Portfolios;
using AngleSharp.Dom;

namespace Booth.PortfolioManager.Web.CachedRepositories
{
    public class CachedTradingCalendarRepository : ITradingCalendarRepository
    {
        private readonly ITradingCalendarRepository _Repository;
        private readonly IMemoryCache _Cache;
        private readonly SemaphoreSlim _Semphore = new SemaphoreSlim(1, 1);

        public CachedTradingCalendarRepository(ITradingCalendarRepository repository, IMemoryCache cache)
        {
            _Repository = repository;
            _Cache = cache;
        }

        public void Add(TradingCalendar entity)
        {
            _Repository.Add(entity);          
        }

        public IEnumerable<TradingCalendar> All()
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            _Repository.Delete(id);
            _Cache.Remove(id);
        }

        public TradingCalendar Get(Guid id)
        {
            if (_Cache.TryGetValue(id, out TradingCalendar entity))
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
                            .SetAbsoluteExpiration(TimeSpan.FromDays(1));

                    _Cache.Set(id, entity, cacheEntryOptions);
                }
            }
            finally
            {
                _Semphore.Release();
            }

            return entity;
        }

        public void Update(TradingCalendar entity)
        {
            _Repository.Update(entity);
            _Cache.Remove(entity.Id);

        }

        public void UpdateYear(TradingCalendar calendar, int year)
        {
            _Repository.UpdateYear(calendar, year);
            _Cache.Remove(calendar.Id);
        }
    }
}
