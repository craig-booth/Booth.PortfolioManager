using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Repository;


namespace Booth.PortfolioManager.Web.Utilities
{
    public interface IHttpContextPortfolioAccessor
    {
        IReadOnlyPortfolio ReadOnlyPortfolio { get; }
        IPortfolio Portfolio { get; }
    }

    class HttpContextPortfolioAccessor : IHttpContextPortfolioAccessor
    {
        private readonly Portfolio _Portfolio;

        public IReadOnlyPortfolio ReadOnlyPortfolio => _Portfolio;
        public IPortfolio Portfolio => _Portfolio;

        public HttpContextPortfolioAccessor(IHttpContextAccessor httpContextAccessor, IPortfolioRepository repository)
        {
            _Portfolio = null;

            if (httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue("portfolioId", out var portfolioParameter))
            {
                if (Guid.TryParse((string)portfolioParameter, out var portfolioId))
                {       
                    _Portfolio = repository.Get(portfolioId);
                }
            }
        }

    }
}
