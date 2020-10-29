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
        IReadOnlyStock Stock { get; }
        string Comment { get; }
        string Description { get; }
    }

    public abstract class PortfolioTransaction : IPortfolioTransaction
    {
        public Guid Id { get; set; }
        public Date Date { get; set; }
        public IReadOnlyStock Stock { get; set; }
        public string Comment { get; set; }
        public abstract string Description { get; }

    }
}
