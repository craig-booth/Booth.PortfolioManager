using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Web.Services;


namespace Booth.PortfolioManager.Web.Test.Services
{
    public class PortfolioPreformanceServiceTests
    {
        [Fact]
        public void PortfolioNotFound()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));

            var service = new PortfolioPerformanceService(null);

            var result = service.GetPerformance(dateRange);

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetPerformance()
        {
            var portfolio = PortfolioTestCreator.CreatePortfolio();

            var service = new PortfolioPerformanceService(portfolio);

            var result = service.GetPerformance(new DateRange(new Date(2001, 01, 01), new Date(2010, 01, 01)));

            result.Result.Should().BeEquivalentTo(new
            {
                OpeningBalance = 335.00m,
                Dividends = 173.00m,
                ChangeInMarketValue = 340.00m,
                OutstandingDRPAmount = -0.50m,
                ClosingBalance = 907.50m,
                OpeningCashBalance = 9620.10m,
                Deposits = 500.00m,
                Withdrawls = -5000.00m,
                Interest = 100.00m,
                Fees = -39.90m,
                ClosingCashBalance = 5120.20m,
                HoldingPerformance = new[]
                {
                    new RestApi.Portfolios.PortfolioPerformanceResponse.HoldingPerformanceItem()
                    {
                        Stock = PortfolioTestCreator.Stock_ARG,
                        OpeningBalance = 105.00m,
                        Purchases = 200.00m,
                        ClosingBalance = 600.00m,
                        Dividends = 140.00m,
                        CapitalGain = 295.00m,
                        DrpCashBalance = 0.00m,
                        TotalReturn = 435.00m,
                        Irr = 0.15062m
                    },
                    new RestApi.Portfolios.PortfolioPerformanceResponse.HoldingPerformanceItem()
                    {
                        Stock = PortfolioTestCreator.Stock_WAM,
                        OpeningBalance = 230.00m,
                        Purchases = 32.50m,
                        ClosingBalance = 307.50m,
                        Dividends = 33.00m,
                        CapitalGain = 45.00m,
                        DrpCashBalance = 0.50m,
                        TotalReturn = 78.00m,
                        Irr = 0.03299m
                    }
                }

            });
        } 
    }
}
