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
        IEnumerable<IPortfolioTransaction> GetTransactionList(IReadOnlyHolding holding, IStockResolver stockResolver);
        bool HasBeenApplied(IPortfolioTransactionList transactions);
    }

    public abstract class CorporateAction : ICorporateAction
    {
        public Guid Id { get; private set; }
        public IReadOnlyStock Stock { get; private set; }
        public Date Date { get; private set; }
        public string Description { get; private set; }

        internal CorporateAction(Guid id, IReadOnlyStock stock, Date actionDate, string description)
        {
            Id = id;
            Stock = stock;
            Date = actionDate;
            Description = description;
        }

        public abstract IEnumerable<IPortfolioTransaction> GetTransactionList(IReadOnlyHolding holding, IStockResolver stockResolver);
        public abstract bool HasBeenApplied(IPortfolioTransactionList transactions);
    }
}
