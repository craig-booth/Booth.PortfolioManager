using System;
using System.Collections.Generic;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Web.Utilities
{
    public class StockPriceRetriever : IStockPriceRetriever
    {
        private readonly IEntityCache<StockPriceHistory> _Cache;

        public StockPriceRetriever(IEntityCache<StockPriceHistory> cache) 
        {
            _Cache = cache;
        }

        public StockPrice GetLatestPrice(Guid stock)
        {
            var priceHistory = _Cache.Get(stock);
            if (priceHistory == null)
                return new StockPrice(Date.MinValue, 0.00m);

            return new StockPrice(priceHistory.LatestDate, priceHistory.GetPrice(priceHistory.LatestDate));
        }

        public decimal GetPrice(Guid stock, Date date)
        {
            var priceHistory = _Cache.Get(stock);
            if (priceHistory == null)
                return 0.00m;

            return priceHistory.GetPrice(date);
        }

        public IEnumerable<StockPrice> GetPrices(Guid stock, DateRange dateRange)
        {
            var priceHistory = _Cache.Get(stock);
            if (priceHistory == null)
                return new StockPrice[0];

            return priceHistory.GetPrices(dateRange);
        }
    }
}
