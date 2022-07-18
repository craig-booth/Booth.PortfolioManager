using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Repository
{

    public interface IStockPriceRepository : IRepository<StockPriceHistory>
    {
        void UpdatePrice(StockPriceHistory stockPriceHistory, Date date);

        void UpdatePrices(StockPriceHistory stockPriceHistory, DateRange dateRange);
    }

    public class StockPriceRepository : Repository<StockPriceHistory>, IStockPriceRepository
    {
        public StockPriceRepository(IPortfolioManagerDatabase database)
            : base(database, "StockPriceHistory")
        {
        }

        public override void Update(StockPriceHistory entity)
        {
            throw new NotSupportedException();
        }

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
