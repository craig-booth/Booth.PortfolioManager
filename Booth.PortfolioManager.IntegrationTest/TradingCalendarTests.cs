using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Web.Models.TradingCalendar;


namespace Booth.PortfolioManager.IntegrationTest
{
    [Collection(Integration.Collection)]
    public class TradingCalendarTests
    {
        private readonly IntegrationTestFixture _Fixture;
        public TradingCalendarTests(IntegrationTestFixture fixture)
        {
            _Fixture = fixture;
        }


        [Fact]
        public async Task Update()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.AdminUser, Integration.Password, TestContext.Current.CancellationToken);

            var calendar = await httpClient.GetAsync<TradingCalendar>("https://integrationtest.com/api/tradingcalendars/2019", TestContext.Current.CancellationToken);

            calendar.AddNonTradingDay(new Date(2019, 04, 01), "April Fools Day");
            await httpClient.PostAsync<TradingCalendar>("https://integrationtest.com/api/tradingcalendars/2019", calendar, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<TradingCalendar>("https://integrationtest.com/api/tradingcalendars/2019", TestContext.Current.CancellationToken);

            response.NonTradingDays.Should().Contain(x => x.Date == new Date(2019, 04, 01) && x.Description == "April Fools Day"); 
        }

    }
}
