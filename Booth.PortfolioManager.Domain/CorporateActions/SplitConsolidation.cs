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
    public class SplitConsolidation : CorporateAction
    {
        internal SplitConsolidation(Guid id, Stock stock, Date actionDate, string description)
            : base(id, stock, CorporateActionType.SplitConsolidation, actionDate, description)
        {
        }

        public override IEnumerable<Transaction> GetTransactionList(IReadOnlyHolding holding)
        {
            var transactions = new List<Transaction>();

            return transactions;
        }

        public override bool HasBeenApplied(ITransactionCollection transactions)
        {
            return false;
        }
    }
}
