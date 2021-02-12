using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.RestApi.Portfolios;

namespace Booth.PortfolioManager.Web.Test.Services
{
    public class PortfolioIncomeServiceTests
    {
        [Fact]
        public void PortfolioNotFound()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));

            var service = new PortfolioIncomeService(null);

            var result = service.GetIncome(dateRange);

            result.Should().HaveNotFoundStatus();
        }


        [Fact]
        public void GetIncome()
        {
            var dateRange = new DateRange(new Date(2001, 01, 01), new Date(2010, 01, 01));

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var service = new PortfolioIncomeService(portfolio);

            var result = service.GetIncome(dateRange);

            result.Result.Should().BeEquivalentTo(new
            {
                Income = new[]
                {
                    new IncomeResponse.IncomeItem()
                    {
                        Stock = PortfolioTestCreator.Stock_ARG,
                        UnfrankedAmount = 20.00m,
                        FrankedAmount = 120.00m,
                        FrankingCredits = 4.00m,
                        NetIncome = 140.00m,
                        GrossIncome = 144.00m
                    },
                    new IncomeResponse.IncomeItem()
                    {
                        Stock = PortfolioTestCreator.Stock_WAM,
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
