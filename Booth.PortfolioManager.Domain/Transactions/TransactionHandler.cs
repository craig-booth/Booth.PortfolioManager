using System;
using System.Collections.Generic;
using System.Text;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public interface ITransactionHandler
    {
        void ApplyTransaction(IPortfolioTransaction transaction);
    }
}
