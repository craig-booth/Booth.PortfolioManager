using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Repository;


namespace Booth.PortfolioManager.Web.Utilities
{
    public interface IPortfolioAccessor
    {
        IReadOnlyPortfolio ReadOnlyPortfolio { get; }
        IPortfolio Portfolio { get; }
    }

    class PortfolioAccessor : IPortfolioAccessor
    {
        private readonly Portfolio _Portfolio;

        public IReadOnlyPortfolio ReadOnlyPortfolio => _Portfolio;
        public IPortfolio Portfolio => _Portfolio;

        public PortfolioAccessor(IHttpContextAccessor httpContextAccessor, IPortfolioRepository repository, IMemoryCache memoryCache)
        {
            _Portfolio = null;

            if (httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue("portfolioId", out var portfolioParameter))
            {
                if (Guid.TryParse((string)portfolioParameter, out var portfolioId))
                {
                    if (memoryCache.TryGetValue(portfolioId, out _Portfolio))
                        return;

                    _Portfolio = repository.Get(portfolioId);
                    if (_Portfolio != null)
                    {
                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                                .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                        memoryCache.Set(portfolioId, _Portfolio, cacheEntryOptions);
                    }

                }
            }
        }

    }
}
