using System;
using System.Threading.Tasks;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.RestApi.Portfolios;
using Booth.PortfolioManager.RestApi.Stocks;
using Microsoft.VisualBasic;

namespace Booth.PortfolioManager.Web.Test.Services
{
    [Collection(Services.Collection)]
    public class PortfolioValueServiceTests
    {

        private readonly ServicesTestFixture _Fixture;
        public PortfolioValueServiceTests(ServicesTestFixture fixture)
        {
            _Fixture = fixture;
        }


        [Fact]
        public async Task PortfolioNotFound()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));

            var service = new PortfolioValueService(null, _Fixture.TradingCalendarRepository);

            var result = await service.GetValue(dateRange, ValueFrequency.Day);

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public async Task GetValueDaily()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 01, 10));

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioValueService(portfolio, _Fixture.TradingCalendarRepository);

            var result = await service.GetValue(dateRange, ValueFrequency.Day);

            result.Result.Should().BeEquivalentTo(new
            {
                Values = new[]
                {
                    new ClosingPrice() {Date = new Date(2000, 01, 03), Price = 9963.10m},
                    new ClosingPrice() {Date = new Date(2000, 01, 04), Price = 9960.10m},
                    new ClosingPrice() {Date = new Date(2000, 01, 05), Price = 9969.10m},
                    new ClosingPrice() {Date = new Date(2000, 01, 06), Price = 9966.10m},
                    new ClosingPrice() {Date = new Date(2000, 01, 07), Price = 9963.10m},
                    new ClosingPrice() {Date = new Date(2000, 01, 10), Price = 9975.10m}
                }
            }); 

        }

        [Fact]
        public async Task GetValueWeekly()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 01, 17));

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioValueService(portfolio, _Fixture.TradingCalendarRepository);

            var result = await service.GetValue(dateRange, ValueFrequency.Week);

            result.Result.Should().BeEquivalentTo(new
            {
                Values = new[]
                {
                    new ClosingPrice() {Date = new Date(2000, 01, 02), Price = 9960.10m},
                    new ClosingPrice() {Date = new Date(2000, 01, 09), Price = 9963.10m},
                    new ClosingPrice() {Date = new Date(2000, 01, 16), Price = 9975.10m},
                    new ClosingPrice() {Date = new Date(2000, 01, 17), Price = 9982.10m}
                }
            });
        }

        [Fact]
        public async Task GetValueMonthly()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 05, 25));

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioValueService(portfolio, _Fixture.TradingCalendarRepository);

            var result = await service.GetValue(dateRange, ValueFrequency.Month);

            result.Result.Should().BeEquivalentTo(new
            {
                Values = new[]
                {
                    new ClosingPrice() {Date = new Date(2000, 01, 03), Price = 9963.10m},
                    new ClosingPrice() {Date = new Date(2000, 01, 31), Price = 9985.10m},
                    new ClosingPrice() {Date = new Date(2000, 02, 29), Price = 9988.10m},
                    new ClosingPrice() {Date = new Date(2000, 03, 31), Price = 9981.10m},
                    new ClosingPrice() {Date = new Date(2000, 04, 30), Price = 9981.10m},
                    new ClosingPrice() {Date = new Date(2000, 05, 25), Price = 9969.10m}
                }
            });
        }

        [Fact]
        public async Task GetValueForStockNotFound()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioValueService(portfolio, _Fixture.TradingCalendarRepository);

            var result = await service.GetValue(Guid.Empty, dateRange, ValueFrequency.Day);

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public async Task GetValueForStockNotOwned()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioValueService(portfolio, _Fixture.TradingCalendarRepository);

            var result = await service.GetValue(Guid.NewGuid(), dateRange, ValueFrequency.Day);

            result.Should().HaveNotFoundStatus();
        }


        [Fact]
        public async Task GetValueForStockDaily()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 01, 10));

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioValueService(portfolio, _Fixture.TradingCalendarRepository);

            var result = await service.GetValue(_Fixture.Stock_ARG.Id, dateRange, ValueFrequency.Day);

            result.Result.Should().BeEquivalentTo(new
            {
                Values = new[]
                {
                    new ClosingPrice() {Date = new Date(2000, 01, 03), Price = 101.00m},
                    new ClosingPrice() {Date = new Date(2000, 01, 04), Price = 100.00m},
                    new ClosingPrice() {Date = new Date(2000, 01, 05), Price = 103.00m},
                    new ClosingPrice() {Date = new Date(2000, 01, 06), Price = 102.00m},
                    new ClosingPrice() {Date = new Date(2000, 01, 07), Price = 101.00m},
                    new ClosingPrice() {Date = new Date(2000, 01, 10), Price = 105.00m}
                }
            });
        }

        [Fact]
        public async Task GetValueForStockWeekly()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 01, 17));

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioValueService(portfolio, _Fixture.TradingCalendarRepository);

            var result = await service.GetValue(_Fixture.Stock_ARG.Id, dateRange, ValueFrequency.Week);

            result.Result.Should().BeEquivalentTo(new
            {
                Values = new[]
                {
                    new ClosingPrice() {Date = new Date(2000, 01, 02), Price = 100.00m},
                    new ClosingPrice() {Date = new Date(2000, 01, 09), Price = 101.00m},
                    new ClosingPrice() {Date = new Date(2000, 01, 16), Price = 107.00m},
                    new ClosingPrice() {Date = new Date(2000, 01, 17), Price = 108.00m}
                }
            });
        }

        [Fact]
        public async Task GetValueForStockMonthly()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 05, 25));

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioValueService(portfolio, _Fixture.TradingCalendarRepository);

            var result = await service.GetValue(_Fixture.Stock_ARG.Id, dateRange, ValueFrequency.Month);

            result.Result.Should().BeEquivalentTo(new
            {
                Values = new[]
                {
                    new ClosingPrice() {Date = new Date(2000, 01, 03), Price = 101.00m},
                    new ClosingPrice() {Date = new Date(2000, 01, 31), Price = 109.00m},
                    new ClosingPrice() {Date = new Date(2000, 02, 29), Price = 110.00m},
                    new ClosingPrice() {Date = new Date(2000, 03, 31), Price = 107.00m},
                    new ClosingPrice() {Date = new Date(2000, 04, 30), Price = 107.00m},
                    new ClosingPrice() {Date = new Date(2000, 05, 25), Price = 103.00m}
                }
            });
        }

    } 
}
