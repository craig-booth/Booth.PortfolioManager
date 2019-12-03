using System;
using System.Collections.Generic;
using System.Text;

namespace Booth.PortfolioManager.Domain.Stocks
{
    public interface IStockResolver
    {
        Stock GetStock(Guid id);
    }
}
