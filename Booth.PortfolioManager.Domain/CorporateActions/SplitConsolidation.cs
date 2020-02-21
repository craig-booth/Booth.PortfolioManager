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
        public int OriginalUnits { get; private set; }
        public int NewUnits { get; private set; }
        internal SplitConsolidation(Guid id, Stock stock, Date actionDate, string description, int originalUnits, int newUnits)
            : base(id, stock, CorporateActionType.SplitConsolidation, actionDate, description)
        {
            OriginalUnits = originalUnits;
            NewUnits = newUnits;
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
