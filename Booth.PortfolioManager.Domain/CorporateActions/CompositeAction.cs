using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain.CorporateActions
{
    public class CompositeAction : ICorporateAction
    {
        public Guid Id { get; private set; }
        public Stock Stock { get; private set; }
        public Date Date { get; private set; }
        public CorporateActionType Type { get; private set; }
        public string Description { get; private set; }

        public IEnumerable<ICorporateAction> ChildActions;

        internal CompositeAction(Guid id, Stock stock, Date actionDate, string description, IEnumerable<ICorporateAction> childActions)
        {
            Id = id;
            Stock = stock;
            Date = actionDate;
            Type = CorporateActionType.Composite;
            Description = description;
            ChildActions = childActions;
        }

        public IEnumerable<IPortfolioTransaction> GetTransactionList(IReadOnlyHolding holding, IStockResolver stockResolver)
        {
            var transactions = new List<IPortfolioTransaction>();

            foreach (var action in ChildActions)
            {
                var childTransactions = action.GetTransactionList(holding, stockResolver);
                transactions.AddRange(childTransactions);
            }         

            return transactions; 
        }

        public bool HasBeenApplied(IPortfolioTransactionList transactions)
        {
            if (ChildActions.Any())
                return ChildActions.First().HasBeenApplied(transactions);
            else
                return false; 
        }
    }
}
