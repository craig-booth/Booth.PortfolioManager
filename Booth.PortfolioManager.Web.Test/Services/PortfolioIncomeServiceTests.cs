using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Models.Portfolio;

namespace Booth.PortfolioManager.Web.Test.Services
{
    [Collection(Services.Collection)]
    public class PortfolioIncomeServiceTests
    {
        private readonly ServicesTestFixture _Fixture;

        public PortfolioIncomeServiceTests(ServicesTestFixture fixture)
        {
            _Fixture = fixture;
        }

        [Fact]
        public void PortfolioNotFound()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));

            var service = new PortfolioIncomeService(null, _Fixture.StockMapper);

            var result = service.GetIncome(dateRange);

            result.Should().HaveNotFoundStatus();
        }


        [Fact]
        public void GetIncome()
        {
            var dateRange = new DateRange(new Date(2001, 01, 01), new Date(2010, 01, 01));

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioIncomeService(portfolio, _Fixture.StockMapper);

            var result = service.GetIncome(dateRange);

            result.Result.Should().BeEquivalentTo(new
            {
                Income = new[]
                {
                    new IncomeResponse.IncomeItem()
                    {
                        Stock = _Fixture.Stock_ARG,
                        UnfrankedAmount = 20.00m,
                        FrankedAmount = 120.00m,
                        FrankingCredits = 4.00m,
                        NetIncome = 140.00m,
                        GrossIncome = 144.00m
                    },
                    new IncomeResponse.IncomeItem()
                    {
                        Stock = _Fixture.Stock_WAM,
                        UnfrankedAmount = 3.00m,
                        FrankedAmount = 30.00m,
                        FrankingCredits = 2.00m,
                        NetIncome = 33.00m,
                        GrossIncome = 35.00m
                    }
                }
            });

        }

  
    }
}
