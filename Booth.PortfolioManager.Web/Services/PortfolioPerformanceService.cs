using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Utils;
using Booth.PortfolioManager.RestApi.Portfolios;
using Booth.PortfolioManager.Web.Mappers;

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
            if (_Portfolio == null)
                return ServiceResult<PortfolioPerformanceResponse>.NotFound();

            var response = new PortfolioPerformanceResponse();

            var dateRangeExcludingFirstDay = new DateRange(dateRange.FromDate.AddDays(1), dateRange.ToDate);

            var openingHoldings = _Portfolio.Holdings.All(dateRange.FromDate);
            var closingHoldings = _Portfolio.Holdings.All(dateRange.ToDate);


            var workingList = new List<HoldingPerformanceWorkItem>();

            HoldingPerformanceWorkItem workItem;

            // Add opening holdings
            foreach (var holding in openingHoldings)
            {
                workItem = new HoldingPerformanceWorkItem(holding.Stock.ToSummaryResponse(dateRange.FromDate));

                var value = holding.Value(dateRange.FromDate);

                workItem.HoldingPerformance.OpeningBalance = value;
                workItem.StartDate = dateRange.FromDate;
                workItem.InitialValue = value;

                workingList.Add(workItem);
            }

            // Process transactions during the period
            var transactions = _Portfolio.Transactions.InDateRange(dateRangeExcludingFirstDay);
            foreach (var transaction in transactions)
            {
                if ((transaction is Aquisition) ||
                    (transaction is OpeningBalance) ||
                    (transaction is Disposal) ||
                    (transaction is IncomeReceived))
                {
                    var newItem = false;

                    workItem = workingList.FirstOrDefault(x => x.HoldingPerformance.Stock.Id == transaction.Stock.Id);
                    if (workItem == null)
                    {
                        newItem = true;
                        workItem = new HoldingPerformanceWorkItem(transaction.Stock.ToSummaryResponse(dateRange.FromDate));
                        workItem.HoldingPerformance.OpeningBalance = 0.00m;
                        workingList.Add(workItem);
                    }

                    if (transaction is Aquisition aquisition)
                    {
                        var value = aquisition.Units * aquisition.AveragePrice;

                        workItem.HoldingPerformance.Purchases += value;                   
                        if (newItem)
                        {
                            workItem.StartDate = aquisition.Date;
                            workItem.InitialValue = value;
                        }
                        else
                            workItem.CashFlows.Add(aquisition.Date, -value);
                    }
                    else if (transaction is OpeningBalance openingBalance)
                    {
                        workItem.HoldingPerformance.Purchases += openingBalance.CostBase;

                        if (newItem)
                        {
                            workItem.StartDate = openingBalance.Date;
                            workItem.InitialValue = openingBalance.CostBase;
                        }
                        else
                            
                        workItem.CashFlows.Add(openingBalance.Date, -openingBalance.CostBase);
                    }
                    else if (transaction is Disposal disposal)
                    {
                        var value = disposal.Units * disposal.AveragePrice;

                        workItem.HoldingPerformance.Sales += value;
                        workItem.CashFlows.Add(disposal.Date, value);
                    }
                    else if (transaction is IncomeReceived income)
                    {
                        workItem.HoldingPerformance.Dividends += income.CashIncome;
                        workItem.CashFlows.Add(income.Date, income.CashIncome);
                    } 
                } 
            }

            // Populate HoldingPerformance from work list
            foreach (var item in workingList)
            {
                //    var holding = closingHoldings.FirstOrDefault(x => x.Stock.Id == item.HoldingPerformance.Stock.Id);
                var holding = _Portfolio.Holdings[item.HoldingPerformance.Stock.Id];

                if (holding.EffectivePeriod.ToDate < dateRange.ToDate)
                {
                    // Holding sold before period ended
                    item.HoldingPerformance.ClosingBalance = 0.00m;
                    item.EndDate = holding.EffectivePeriod.ToDate;
                    item.FinalValue = 0.00m;
                    item.HoldingPerformance.DrpCashBalance = 0.00m;
                }
                else
                {
                    // Holding still held at period end
                    var value = holding.Value(dateRange.ToDate);
                    item.HoldingPerformance.ClosingBalance = value;

                    item.EndDate = dateRange.ToDate;
                    item.FinalValue = value;

                    item.HoldingPerformance.DrpCashBalance = holding.DrpAccount.Balance(dateRange.ToDate);
                }

                item.HoldingPerformance.CapitalGain = item.HoldingPerformance.ClosingBalance - (item.HoldingPerformance.OpeningBalance + item.HoldingPerformance.Purchases - item.HoldingPerformance.Sales);
                item.HoldingPerformance.TotalReturn = item.HoldingPerformance.CapitalGain + item.HoldingPerformance.Dividends;

                var irr = IrrCalculator.CalculateIrr(item.StartDate, item.InitialValue, item.EndDate, item.FinalValue, item.CashFlows);
                if (double.IsNaN(irr) || double.IsInfinity(irr))
                    item.HoldingPerformance.Irr = 0.00M;
                else
                    item.HoldingPerformance.Irr = (decimal)Math.Round(irr, 5);

                response.HoldingPerformance.Add(item.HoldingPerformance); 
            }

            var cashTransactions = _Portfolio.CashAccount.Transactions.InDateRange(dateRangeExcludingFirstDay);
            response.OpeningCashBalance = _Portfolio.CashAccount.Balance(dateRange.FromDate);
            response.Deposits = cashTransactions.Where(x => x.Type == BankAccountTransactionType.Deposit).Sum(x => x.Amount);
            response.Withdrawls = cashTransactions.Where(x => x.Type == BankAccountTransactionType.Withdrawl).Sum(x => x.Amount);
            response.Interest = cashTransactions.Where(x => x.Type == BankAccountTransactionType.Interest).Sum(x => x.Amount);
            response.Fees = cashTransactions.Where(x => x.Type == BankAccountTransactionType.Fee).Sum(x => x.Amount);
            response.ClosingCashBalance = _Portfolio.CashAccount.Balance(dateRange.ToDate);

            response.OpeningBalance = openingHoldings.Sum(x => x.Value(dateRange.FromDate));
            response.Dividends = response.HoldingPerformance.Sum(x => x.Dividends);
            response.ChangeInMarketValue = response.HoldingPerformance.Sum(x => x.CapitalGain);
            response.OutstandingDRPAmount = -response.HoldingPerformance.Sum(x => x.DrpCashBalance);
            response.ClosingBalance = closingHoldings.Sum(x => x.Value(dateRange.ToDate));

          
            return ServiceResult<PortfolioPerformanceResponse>.Ok(response);
        }

        private class HoldingPerformanceWorkItem
        {
            public PortfolioPerformanceResponse.HoldingPerformanceItem HoldingPerformance;
            public CashFlows CashFlows;
            public Date StartDate;
            public decimal InitialValue;
            public Date EndDate;
            public decimal FinalValue;

            public HoldingPerformanceWorkItem(Stock stock)
            {
                HoldingPerformance = new PortfolioPerformanceResponse.HoldingPerformanceItem()
                {
                    Stock = stock
                };
                CashFlows = new CashFlows();
            } 
        } 
    } 
}

