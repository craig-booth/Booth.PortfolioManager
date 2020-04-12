using System;
using System.Collections.Generic;
using System.Text;

using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Portfolios
{
    public interface IStockResolver
    {
        IReadOnlyStock GetStock(Guid id);
    }
}
