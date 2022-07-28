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

        private List<ICorporateAction> _ChildActions;
        public IEnumerable<ICorporateAction> ChildActions
        {
            get { return _ChildActions; }
            private set { _ChildActions = (List<ICorporateAction>)value; }
        }

        public CompositeAction(Guid id, IReadOnlyStock stock, Date actionDate, string description, IEnumerable<ICorporateAction> childActions)
            : base(id, stock, actionDate, (description != "") ? description : "Complex corporate action")
        {
            _ChildActions = new List<ICorporateAction>(childActions);
        }

        public override IEnumerable<IPortfolioTransaction> GetTransactionList(IReadOnlyHolding holding, IStockResolver stockResolver)
        {
            var transactions = new List<IPortfolioTransaction>();

            foreach (var action in ChildActions)
            {
                var childTransactions = action.GetTransactionList(holding, stockResolver);
                transactions.AddRange(childTransactions);
            }         

            return transactions; 
        }

        public override bool HasBeenApplied(IPortfolioTransactionList transactions)
        {
            if (ChildActions.Any())
                return ChildActions.First().HasBeenApplied(transactions);
            else
                return false; 
        }
    }
}
