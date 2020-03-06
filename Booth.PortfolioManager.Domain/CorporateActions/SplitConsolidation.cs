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
        internal SplitConsolidation(Guid id, Stock stock, Date actionDate, string description, int originalUnits, int newUnits)
            : base(id, stock, CorporateActionType.SplitConsolidation, actionDate, description)
        {
            OriginalUnits = originalUnits;
            NewUnits = newUnits;
        }

        public IEnumerable<IPortfolioTransaction> GetTransactionList(IReadOnlyHolding holding, IStockResolver stockResolver)
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

        public bool HasBeenApplied(IPortfolioTransactionList transactions)
        {
            return transactions.ForHolding(Stock.Id, Date).OfType<UnitCountAdjustment>().Any();
        }
    }
}
