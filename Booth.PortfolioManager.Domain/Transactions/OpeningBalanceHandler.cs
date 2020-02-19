using System;
using System.Collections.Generic;
using System.Text;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public class OpeningBalanceHandler : ITransactionHandler
    {
        private IHoldingCollection _Holdings;
        private ICashAccount _CashAccount;

        public OpeningBalanceHandler(IHoldingCollection holdings, ICashAccount cashAccount)
        {
            _Holdings = holdings;
            _CashAccount = cashAccount;
        }

        public void ApplyTransaction(Transaction transaction)
        {
            var openingBalance = transaction as OpeningBalance;
            if (openingBalance == null)
                throw new ArgumentException("Expected transaction to be an OpeningBalance");

            var holding = _Holdings[openingBalance.Stock.Id];
            if (holding == null)
            {
                holding = _Holdings.Add(openingBalance.Stock, openingBalance.Date);
            }

            holding.AddParcel(openingBalance.Date, openingBalance.AquisitionDate, openingBalance.Units, openingBalance.CostBase, openingBalance.CostBase, transaction);
        }
    }
}
