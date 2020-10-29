using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Web.Utilities;

namespace Booth.PortfolioManager.Web.Test.Services
{
    public class PortfolioSummaryServiceTests
    {
        [Fact]
        public void PortfolioNotFound()
        {
            var service = new PortfolioSummaryService(null);

            var result = service.GetSummary(new Date(2000, 01, 01));

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetSummary()
        {
            var portfolio = PortfolioTestCreator.CreatePortfolio();

            var service = new PortfolioSummaryService(portfolio);

            var result = service.GetSummary(new Date(2010, 01, 01));

            result.Result.Should().BeEquivalentTo(new
            {
                PortfolioValue = 200m + 300m + (10000m - 119.95m - 259.95m),
                PortfolioCost = 10000.00m,
                Return1Year = 0.00698m,
                Return3Year = 0.00769m,
                Return5Year = 0.00339m,
                ReturnAll = 0.00159m,
                CashBalance = 10000m - 119.95m - 259.95m,
                Holdings = new[]
                {
                    new RestApi.Portfolios.Holding()
                    {
                        Stock = PortfolioTestCreator.Stock_ARG,
                        Units = 100,
                        Value = 200m,
                        Cost = 119.95m,
                        CostBase = 119.95m
                    },
                    new RestApi.Portfolios.Holding()
                    {
                        Stock = PortfolioTestCreator.Stock_WAM,
                        Units = 200,
                        Value = 300m,
                        Cost = 259.95m,
                        CostBase = 259.95m
                    }
                }

            });

        }
    } 
}
