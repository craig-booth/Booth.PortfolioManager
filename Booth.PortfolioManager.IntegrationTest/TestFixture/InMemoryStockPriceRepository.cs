using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Repository;

namespace Booth.PortfolioManager.IntegrationTest.TestFixture
{
    internal class InMemoryStockPriceRepository : InMemoryRepository<StockPriceHistory>, IStockPriceRepository
    {
        public void UpdatePrice(StockPriceHistory stockPriceHistory, Date date)
        {
            throw new NotImplementedException();
        }

        public void UpdatePrices(StockPriceHistory stockPriceHistory, DateRange dateRange)
        {
            throw new NotImplementedException();
        }
    }
}
