using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.RestApi.Portfolios;

namespace Booth.PortfolioManager.Web.Services
{

    public interface IPortfolioPerformanceService
    {
        ServiceResult<PortfolioPerformanceResponse> GetPerformance(DateRange dateRange);
    }
    
    public class PortfolioPerformanceService : IPortfolioPerformanceService
    {
        private readonly IReadOnlyPortfolio _Portfolio;

        public PortfolioPerformanceService(IReadOnlyPortfolio portfolio)
        {
            _Portfolio = portfolio;
        }

        public ServiceResult<PortfolioPerformanceResponse> GetPerformance(DateRange dateRange)
        {
            /*     var portfolio = _PortfolioCache.Get(portfolioId);

                 var response = new PortfolioPerformanceResponse();

                 var dateRangeExcludingFirstDay = new DateRange(dateRange.FromDate.AddDays(1), dateRange.ToDate);

                 var openingHoldings = portfolio.Holdings.All(dateRange.FromDate);
                 var closingHoldings = portfolio.Holdings.All(dateRange.ToDate);


                 var workingList = new List<HoldingPerformanceWorkItem>();

                 HoldingPerformanceWorkItem workItem;

                 // Add opening holdings
                 foreach (var holding in openingHoldings)
                 {
                     workItem = new HoldingPerformanceWorkItem(holding.Stock.Convert(dateRange.FromDate));

                     var value = holding.Value(dateRange.FromDate);
                     workItem.HoldingPerformance.OpeningBalance = value;
                     workItem.CashFlows.Add(dateRange.FromDate, -value);
                     workingList.Add(workItem);
                 }

                 // Process transactions during the period
                 var transactions = portfolio.Transactions.InDateRange(dateRangeExcludingFirstDay);
                 foreach (var transaction in transactions)
                 {
                     if ((transaction is Aquisition) ||
                         (transaction is OpeningBalance) ||
                         (transaction is Disposal) ||
                         (transaction is IncomeReceived))
                     {
                         workItem = workingList.FirstOrDefault(x => x.HoldingPerformance.Stock.Id == transaction.Stock.Id);
                         if (workItem == null)
                         {
                             workItem = new HoldingPerformanceWorkItem(transaction.Stock.Convert(transaction.Date));
                             workingList.Add(workItem);
                         }

                         if (transaction is Aquisition)
                         {
                             var aquisition = transaction as Aquisition;

                             workItem.HoldingPerformance.Purchases += aquisition.Units * aquisition.AveragePrice;
                             workItem.CashFlows.Add(aquisition.Date, -(aquisition.Units * aquisition.AveragePrice));
                         }
                         else if (transaction is OpeningBalance)
                         {
                             var openingBalance = transaction as OpeningBalance;

                             workItem.HoldingPerformance.Purchases += openingBalance.CostBase;
                             workItem.CashFlows.Add(openingBalance.Date, -openingBalance.CostBase);
                         }
                         else if (transaction is Disposal)
                         {
                             var disposal = transaction as Disposal;

                             workItem.HoldingPerformance.Sales += disposal.Units * disposal.AveragePrice;
                             workItem.CashFlows.Add(disposal.Date, disposal.Units * disposal.AveragePrice);
                         }
                         else if (transaction is IncomeReceived)
                         {
                             var income = transaction as IncomeReceived;

                             workItem.HoldingPerformance.Dividends += income.CashIncome;
                             workItem.CashFlows.Add(income.Date, income.CashIncome);
                         }
                     }
                 }

                 // Populate HoldingPerformance from work list
                 foreach (var item in workingList)
                 {
                     var holding = closingHoldings.FirstOrDefault(x => x.Stock.Id == item.HoldingPerformance.Stock.Id);
                     if (holding != null)
                     {
                         var value = holding.Value(dateRange.ToDate);
                         item.HoldingPerformance.ClosingBalance = value;
                         item.CashFlows.Add(dateRange.ToDate, value);

                         item.HoldingPerformance.DRPCashBalance = holding.DrpAccount.Balance(dateRange.ToDate);
                     }
                     else
                         item.HoldingPerformance.ClosingBalance = 0.00m;

                     item.HoldingPerformance.CapitalGain = item.HoldingPerformance.ClosingBalance - (item.HoldingPerformance.OpeningBalance + item.HoldingPerformance.Purchases - item.HoldingPerformance.Sales);
                     item.HoldingPerformance.TotalReturn = item.HoldingPerformance.CapitalGain + item.HoldingPerformance.Dividends;

                     var irr = IrrCalculator.CalculateIrr(item.CashFlows);
                     item.HoldingPerformance.IRR = (decimal)Math.Round(irr, 5);

                     response.HoldingPerformance.Add(item.HoldingPerformance);
                 }

                 var cashTransactions = portfolio.CashAccount.Transactions.InDateRange(dateRangeExcludingFirstDay);
                 response.OpeningCashBalance = portfolio.CashAccount.Balance(dateRange.FromDate);
                 response.Deposits = cashTransactions.Where(x => x.Type == BankAccountTransactionType.Deposit).Sum(x => x.Amount);
                 response.Withdrawls = cashTransactions.Where(x => x.Type == BankAccountTransactionType.Withdrawl).Sum(x => x.Amount);
                 response.Interest = cashTransactions.Where(x => x.Type == BankAccountTransactionType.Interest).Sum(x => x.Amount);
                 response.Fees = cashTransactions.Where(x => x.Type == BankAccountTransactionType.Fee).Sum(x => x.Amount);
                 response.ClosingCashBalance = portfolio.CashAccount.Balance(dateRange.ToDate);

                 response.OpeningBalance = openingHoldings.Sum(x => x.Value(dateRange.FromDate));
                 response.Dividends = response.HoldingPerformance.Sum(x => x.Dividends);
                 response.ChangeInMarketValue = response.HoldingPerformance.Sum(x => x.CapitalGain);
                 response.OutstandingDRPAmount = -response.HoldingPerformance.Sum(x => x.DRPCashBalance);
                 response.ClosingBalance = closingHoldings.Sum(x => x.Value(dateRange.ToDate));

                 return response; */
            throw new NotSupportedException();
        }

      /*  private class HoldingPerformanceWorkItem
        {
            public PortfolioPerformanceResponse.HoldingPerformanceItem HoldingPerformance;
            public CashFlows CashFlows;

            public HoldingPerformanceWorkItem(Stock stock)
            {
                HoldingPerformance = new PortfolioPerformanceResponse.HoldingPerformanceItem()
                {
                    Stock = stock
                };
                CashFlows = new CashFlows();
            } 
        } */
    } 
}

