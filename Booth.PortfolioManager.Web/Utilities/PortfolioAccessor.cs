using System;
using Microsoft.AspNetCore.Http;

using Booth.PortfolioManager.Domain.Portfolios;

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
        public PortfolioAccessor(IHttpContextAccessor httpContextAccessor, IPortfolioCache portfolioCache)
        {
            _Portfolio = null;

            if (httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue("portfolioId", out var portfolioParameter))
            {
                if (Guid.TryParse((string)portfolioParameter, out var portfolioId))
                {
                    if (portfolioCache.TryGet(portfolioId, out var portfolio))
                    {
                        _Portfolio = portfolio;
                    }
                }
            }
        }

        public IReadOnlyPortfolio ReadOnlyPortfolio => _Portfolio;
        public IPortfolio Portfolio => _Portfolio;
    }
}
