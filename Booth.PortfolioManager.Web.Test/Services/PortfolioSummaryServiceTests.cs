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
                PortfolioValue = 6027.70m,
                PortfolioCost = 5772.50m,
                Return1Year = 0.02228m,
                Return3Year = 0.02132m,
                Return5Year = 0.00916m,
                ReturnAll = 0.00611m,
                CashBalance = 5120.20m,
                Holdings = new[]
                {
                    new RestApi.Portfolios.Holding()
                    {
                        Stock = PortfolioTestCreator.Stock_ARG,
                        Units = 300,
                        Value = 600m,
                        Cost = 359.85m,
                        CostBase = 359.85m
                    },
                    new RestApi.Portfolios.Holding()
                    {
                        Stock = PortfolioTestCreator.Stock_WAM,
                        Units = 205,
                        Value = 307.50m,
                        Cost = 292.45m,
                        CostBase = 292.45m
                    }
                }

            });

        }
    } 
}
