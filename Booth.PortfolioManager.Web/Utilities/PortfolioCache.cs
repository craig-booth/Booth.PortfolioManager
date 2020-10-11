using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

using Booth.EventStore;
using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Web.Utilities
{
    public interface IPortfolioCache
    {
        Portfolio Get(Guid id);
        bool TryGet(Guid id, out Portfolio portfolio);
    }

    public class PortfolioCache : IPortfolioCache
    {
        private IRepository<Portfolio> _Repository;
        private IMemoryCache _Cache;

        public PortfolioCache(IRepository<Portfolio> repository, IMemoryCache memoryCache)
        {
            _Repository = repository;
            _Cache = memoryCache;
        }

        public bool TryGet(Guid id, out Portfolio portfolio)
        {
            if (_Cache.TryGetValue(id, out portfolio))
                return true;

            portfolio = _Repository.Get(id);
            if (portfolio == null)
                return false;

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

            _Cache.Set(id, portfolio, cacheEntryOptions);

            return true;
        }

        public Portfolio Get(Guid id)
        {
            if (TryGet(id, out var portfolio))
                return portfolio;
            else
                throw new KeyNotFoundException();
        }
    }
}
