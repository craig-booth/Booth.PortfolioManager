using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.IntegrationTest.TestFixture;
using Booth.PortfolioManager.DataServices;
using System.Diagnostics;


namespace Booth.PortfolioManager.IntegrationTest
{
    public class DataServiceTests : IClassFixture<AppTestFixture>
    {
        private AppTestFixture _Fixture;
        public DataServiceTests(AppTestFixture fixture)
        {
            _Fixture = fixture;
        }

        [Fact]
        public async void GetTradingDaysForThisYear()
        {
            var dataService = _Fixture.Services.GetService<ITradingDayService>();

            var nonTradingDays = await dataService.GetNonTradingDays(Date.Today.Year, CancellationToken.None);

            nonTradingDays.Should().NotBeEmpty();
        }

        [Fact]
        public async void GetLiveStockPrice()
        {
            var dataService = _Fixture.Services.GetService<ILiveStockPriceService>();

            var price = await dataService.GetSinglePrice("ARG", CancellationToken.None);

            price.Should().NotBeNull();
            price.Date.Should().BeGreaterThanOrEqualTo(Date.Today.AddDays(-7));
        }

        [Fact]
        public async void GetLastWeeksClosingPrices()
        {
            var dataService = _Fixture.Services.GetService<IHistoricalStockPriceService>();

            var prices = await dataService.GetHistoricalPriceData("ARG", new DateRange(Date.Today.AddDays(-7), Date.Today), CancellationToken.None);

            prices.Should().NotBeEmpty();
        }

    }
}
