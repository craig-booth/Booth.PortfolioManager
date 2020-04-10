using System;
using System.Collections.Generic;
using System.Text;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public interface ITransactionHandler
    {
        void Apply(IPortfolioTransaction transaction, IHolding holding, ICashAccount cashAccount);
    }

}
