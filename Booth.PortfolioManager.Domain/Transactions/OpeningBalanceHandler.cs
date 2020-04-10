using System;
using System.Collections.Generic;
using System.Text;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public class OpeningBalanceHandler : ITransactionHandler
    {
        public void Apply(IPortfolioTransaction transaction, IHolding holding, ICashAccount cashAccount)
        {
            var openingBalance = transaction as OpeningBalance;
            if (openingBalance == null)
                throw new ArgumentException("Expected transaction to be an OpeningBalance");

            if (!openingBalance.Stock.IsEffectiveAt(openingBalance.Date))
                throw new StockNotActive("Stock is not active");

            holding.AddParcel(openingBalance.Date, openingBalance.AquisitionDate, openingBalance.Units, openingBalance.CostBase, openingBalance.CostBase, transaction);
        }
    }
}
