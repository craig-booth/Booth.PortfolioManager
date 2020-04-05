using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain.CorporateActions
{
    public class CompositeAction : CorporateAction
    {
        IEnumerable<ICorporateAction> _ChildActions;

        internal CompositeAction(Guid id, Stock stock, Date actionDate, string description, IEnumerable<ICorporateAction> childActions)
            : base(id, stock, CorporateActionType.Composite, actionDate, description)
        {
            _ChildActions = childActions;
        }

        public IEnumerable<IPortfolioTransaction> GetTransactionList(IReadOnlyHolding holding, IStockResolver stockResolver)
        {
            var transactions = new List<IPortfolioTransaction>();

            foreach (var action in _ChildActions)
            {
                var childTransactions = action.GetTransactionList(holding, stockResolver);
                transactions.AddRange(childTransactions);
            }         

            return transactions; 
        }

        public bool HasBeenApplied(IPortfolioTransactionList transactions)
        {
            if (_ChildActions.Any())
                return _ChildActions.First().HasBeenApplied(transactions);
            else
                return false; 
        }
    }
}
