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
    [Collection(Services.Collection)]
    public class PortfolioSummaryServiceTests
    {
        private readonly ServicesTestFixture _Fixture;

        public PortfolioSummaryServiceTests(ServicesTestFixture fixture)
        {
            _Fixture = fixture;
        }

        [Fact]
        public void PortfolioNotFound()
        {
            var service = new PortfolioSummaryService(null, new IrrReturnCalculator(_Fixture.StockPriceRetriever), _Fixture.HoldingMapper);

            var result = service.GetSummary(new Date(2000, 01, 01));

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetSummary()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioSummaryService(portfolio, new IrrReturnCalculator(_Fixture.StockPriceRetriever), _Fixture.HoldingMapper);

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
                    new Models.Portfolio.Holding()
                    {
                        Stock = _Fixture.Stock_ARG,
                        Units = 250,
                        Value = 500m,
                        Cost = 299.87m,
                        CostBase = 299.87m
                    },
                    new Models.Portfolio.Holding()
                    {
                        Stock = _Fixture.Stock_WAM,
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
