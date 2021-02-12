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
    public class PortfolioCgtLiabilityServiceTests
    {
        [Fact]
        public void PortfolioNotFound()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));

            var service = new PortfolioCgtLiabilityService(null);

            var result = service.GetCGTLiability(dateRange);

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetCGTLiability()
        {
            var dateRange = new DateRange(new Date(2003, 07, 01), new Date(2004, 06, 30));

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var service = new PortfolioCgtLiabilityService(portfolio);

            var result = service.GetCGTLiability(dateRange);

            result.Result.Should().BeEquivalentTo(new
            {
                CurrentYearCapitalGainsOther = 0.00m,
                CurrentYearCapitalGainsDiscounted = 0.00m,
                CurrentYearCapitalGainsTotal =-0.00m,
                CurrentYearCapitalLossesOther = 0.00m,
                CurrentYearCapitalLossesDiscounted = 28.93m,
                CurrentYearCapitalLossesTotal = 28.93m,
                GrossCapitalGainOther = 0.00m,
                GrossCapitalGainDiscounted = -28.93m,
                GrossCapitalGainTotal = -28.93m,
                Discount = 0.00m,
                NetCapitalGainOther = 0.00m, 
                NetCapitalGainDiscounted = -28.93m,
                NetCapitalGainTotal = -28.93m,
                Events = new [] 
                { 
                    new CgtLiabilityResponse.CgtLiabilityEvent()
                    {
                        Stock = PortfolioTestCreator.Stock_ARG,
                        EventDate = new Date(2004, 01, 01),
                        CostBase = 59.98m,
                        AmountReceived = 31.05m,
                        CapitalGain = -28.93m,
                        Method = CgtMethod.Discount
                    }
                }
            });
        }
    } 
}
