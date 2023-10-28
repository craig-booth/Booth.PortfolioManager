﻿using System;
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

        public void Add(TradingCalendar entity)
        {
            _Repository.Add(entity);          
        }

        public Task AddAsync(TradingCalendar entity)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TradingCalendar> All()
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<TradingCalendar> AllAsync()
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

        public TradingCalendar Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<TradingCalendar> GetAsync(Guid id)
        {
            return _Cache.GetOrCreateAsync<TradingCalendar>(id, (cacheEntry) =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                return _Repository.GetAsync(id);
            });
        }

        public void Update(TradingCalendar entity)
        {
            _Repository.Update(entity);
            _Cache.Remove(entity.Id);

        }

        public Task UpdateAsync(TradingCalendar entity)
        {
            throw new NotImplementedException();
        }

        public void UpdateYear(TradingCalendar calendar, int year)
        {
            _Repository.UpdateYear(calendar, year);
            _Cache.Remove(calendar.Id);
        }

        public Task UpdateYearAsync(TradingCalendar calendar, int year)
        {
            throw new NotImplementedException();
        }
    }
}
