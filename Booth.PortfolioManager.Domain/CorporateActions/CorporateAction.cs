using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.CorporateActions
{

    public enum CorporateActionType { Dividend, CapitalReturn, Transformation, SplitConsolidation, Composite }

    public interface ICorporateAction : ITransaction
    {
        Stock Stock { get; }
        CorporateActionType Type { get; }
        string Description { get; }
        IEnumerable<IPortfolioTransaction> GetTransactionList(IReadOnlyHolding holding, IStockResolver stockResolver);
        bool HasBeenApplied(IPortfolioTransactionList transactions);
    }
}
