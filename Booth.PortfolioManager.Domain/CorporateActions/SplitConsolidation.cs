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
    public class SplitConsolidation : ICorporateAction
    {
        public Guid Id { get; private set; }
        public Stock Stock { get; private set; }
        public Date Date { get; private set; }
        public CorporateActionType Type { get; private set; }
        public string Description { get; private set; }
        public int OriginalUnits { get; private set; }
        public int NewUnits { get; private set; }
        internal SplitConsolidation(Guid id, Stock stock, Date actionDate, string description, int originalUnits, int newUnits)
        {
            Id = id;
            Stock = stock;
            Date = actionDate;
            Type = CorporateActionType.SplitConsolidation;
            Description = description;
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
