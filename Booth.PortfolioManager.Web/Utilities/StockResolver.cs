using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Web.Utilities
{
    class StockResolver : IStockResolver
    {

        private readonly IEntityCache<Stock> _StockCache;

        public StockResolver(IEntityCache<Stock> stockCache)
        {
            _StockCache = stockCache;
        }

        public IReadOnlyStock GetStock(Guid id)
        {
            return _StockCache.Get(id);
        }
    }
}
