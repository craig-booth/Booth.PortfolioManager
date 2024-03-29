﻿using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Utils;
using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain.Portfolios
{

    public interface IPortfolioReturnCalculator
    {
        decimal Calculate(IReadOnlyPortfolio portfolio, DateRange dateRange);
    }

    public class IrrReturnCalculator : IPortfolioReturnCalculator
    {
        private readonly IStockPriceRetriever _PriceRetreiver;
        public IrrReturnCalculator(IStockPriceRetriever priceRetriever)
        {
            _PriceRetreiver = priceRetriever;
        }

        public decimal Calculate(IReadOnlyPortfolio portfolio, DateRange dateRange)
        {
            // Get the initial portfolio value         
            var initialHoldings = portfolio.Holdings.All(dateRange.FromDate);
            var initialHoldingsValue = initialHoldings.Sum(x => x.Properties[dateRange.FromDate].Units * _PriceRetreiver.GetPrice(x.Stock.Id, dateRange.FromDate));
            var initialCashBalance = portfolio.CashAccount.Balance(dateRange.FromDate);
            var initialValue = initialHoldingsValue + initialCashBalance;

            // Get the final portfolio value
            var finalHoldings = portfolio.Holdings.All(dateRange.ToDate);
            var finalHoldingsValue = finalHoldings.Sum(x => x.Properties[dateRange.ToDate].Units * _PriceRetreiver.GetPrice(x.Stock.Id, dateRange.ToDate));
            var finalCashBalance = portfolio.CashAccount.Balance(dateRange.ToDate);
            var finalValue = finalHoldingsValue + finalCashBalance;

            // Generate list of cashFlows
            var cashFlows = new CashFlows();
            var transactionRange = new DateRange(dateRange.FromDate.AddDays(1), dateRange.ToDate);
            var transactions = portfolio.CashAccount.Transactions.InDateRange(transactionRange)
                .Where(x => (x.Type == BankAccountTransactionType.Deposit) || ((x.Type == BankAccountTransactionType.Withdrawl)));
            foreach (var transaction in transactions)
            {
                cashFlows.Add(transaction.Date, -transaction.Amount);
            }
                

            var irr = IrrCalculator.CalculateIrr(dateRange.FromDate, initialValue, dateRange.ToDate, finalValue, cashFlows);

            return (decimal)Math.Round(irr, 5);
        }
    }
}
