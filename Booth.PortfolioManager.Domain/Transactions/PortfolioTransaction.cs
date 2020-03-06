using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

using Booth.PortfolioManager.Domain.Utils;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public interface IPortfolioTransaction : ITransaction
    {
        Stock Stock { get; }
        string Comment { get; }
        string Description { get; }
    }
}
