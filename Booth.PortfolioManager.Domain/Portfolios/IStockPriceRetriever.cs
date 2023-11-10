using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.PortfolioManager.Domain.Portfolios
{
    public interface IStockPriceRetriever
    {
        decimal GetPrice(Guid stock, Date date);
        StockPrice GetLatestPrice(Guid stock);
        IEnumerable<StockPrice> GetPrices(Guid stock, DateRange dateRange);
    }
}
