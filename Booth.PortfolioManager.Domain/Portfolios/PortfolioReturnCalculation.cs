using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Utils;
using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain.Portfolios
{
    public static class PortfolioReturnCalculation
    {
        public static decimal CalculateIRR(this Portfolio portfolio, DateRange dateRange)
        {
            var cashFlows = new CashFlows();

            // Get the initial portfolio value         
            var initialHoldings = portfolio.Holdings.All(dateRange.FromDate);
            var initialHoldingsValue = initialHoldings.Sum(x => x.Value(dateRange.FromDate));

            // Get initial Cash Account Balance
            var initialCashBalance = portfolio.CashAccount.Balance(dateRange.FromDate);

            // Add the initial portfolio value
            var initialValue = initialHoldingsValue + initialCashBalance;
            cashFlows.Add(dateRange.FromDate, -initialValue);

            // generate list of cashFlows
            var transactionRange = new DateRange(dateRange.FromDate.AddDays(1), dateRange.ToDate);
            var transactions = portfolio.Transactions.InDateRange(transactionRange).OfType<CashTransaction>();
            foreach (var transaction in transactions)
            {
                var cashTransaction = transaction as CashTransaction;
                if ((cashTransaction.CashTransactionType == BankAccountTransactionType.Deposit) ||
                    (cashTransaction.CashTransactionType == BankAccountTransactionType.Withdrawl))
                {
                    cashFlows.Add(cashTransaction.Date, -cashTransaction.Amount);
                }
            }

            // Get the final portfolio value
            var finalHoldings = portfolio.Holdings.All(dateRange.ToDate);
            var finalHoldingsValue = finalHoldings.Sum(x => x.Value(dateRange.ToDate));

            // Get final Cash Account Balance
            var finalCashBalance = portfolio.CashAccount.Balance(dateRange.ToDate);

            // Add the final portfolio value
            var finalValue = finalHoldingsValue + finalCashBalance;
            cashFlows.Add(dateRange.ToDate, finalValue);

            //   var irr = IrrCalculator.CalculateIrr(cashFlows);

            //  return (decimal)Math.Round(irr, 5);

            return 0.00m;
        }
    }
}
