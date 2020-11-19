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
                PortfolioValue = 5958.75m,
                PortfolioCost = 5743.57m,
                Return1Year = 0.01991m,
                Return3Year = 0.01882m,
                Return5Year = 0.00837m,
                ReturnAll = 0.00539m,
                CashBalance = 5151.25m,
                Holdings = new[]
                {
                    new RestApi.Portfolios.Holding()
                    {
                        Stock = PortfolioTestCreator.Stock_ARG,
                        Units = 250,
                        Value = 500m,
                        Cost = 299.87m,
                        CostBase = 299.87m
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
