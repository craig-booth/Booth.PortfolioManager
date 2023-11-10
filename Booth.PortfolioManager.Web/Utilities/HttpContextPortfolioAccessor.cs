using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Repository;


namespace Booth.PortfolioManager.Web.Utilities
{
    public interface IHttpContextPortfolioAccessor
    {
        Task<IReadOnlyPortfolio> GetReadOnlyPortfolio();
        Task<IPortfolio> GetPortfolio();
    }

    class HttpContextPortfolioAccessor : IHttpContextPortfolioAccessor
    {
        private readonly IPortfolioRepository _Repository;
        private Guid _PortfolioId = Guid.Empty;

        public HttpContextPortfolioAccessor(IHttpContextAccessor httpContextAccessor, IPortfolioRepository repository)
        {
            _Repository = repository;

            if (httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue("portfolioId", out var portfolioParameter))
            {
                Guid.TryParse((string)portfolioParameter, out _PortfolioId);
            }
        }

        public async Task<IReadOnlyPortfolio> GetReadOnlyPortfolio()
        {
            return await GetPortfolioInternal();
        }

        public async Task<IPortfolio> GetPortfolio()
        {
            return await GetPortfolioInternal();
        }

        private async Task<Portfolio> GetPortfolioInternal()
        {
            return await _Repository.GetAsync(_PortfolioId);
        }
    }
}
