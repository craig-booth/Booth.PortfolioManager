using System;
using System.Collections.Generic;
using System.Linq;

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
        public SplitConsolidation(Guid id, IReadOnlyStock stock, Date actionDate, string description, int originalUnits, int newUnits)
            : base(id, stock, actionDate, (description != "") ? description :  (originalUnits <= newUnits) ? String.Format("{0} for {1} Stock Split", originalUnits, newUnits) :  String.Format("{0} for {1} Stock Consolidation", originalUnits, newUnits))
        {
            OriginalUnits = originalUnits;
            NewUnits = newUnits;
        }

        public override IEnumerable<IPortfolioTransaction> GetTransactionList(IReadOnlyHolding holding, IStockResolver stockResolver)
        {
            var transactions = new List<IPortfolioTransaction>();

            var holdingProperties = holding.Properties[Date];
            if (holdingProperties.Units == 0)
                return transactions;

            var dividendRules = Stock.DividendRules[Date];

            var returnOfCapital = new UnitCountAdjustment()
            {
                Id = Guid.NewGuid(),
                Date = Date,
                Stock = Stock,
                NewUnits = NewUnits,
                OriginalUnits = OriginalUnits,
                Comment = Description
            };
            transactions.Add(returnOfCapital);

            return transactions;
        }

        public override bool HasBeenApplied(IPortfolioTransactionList transactions)
        {
            return transactions.ForHolding(Stock.Id, Date).OfType<UnitCountAdjustment>().Any();
        }
    }
}
