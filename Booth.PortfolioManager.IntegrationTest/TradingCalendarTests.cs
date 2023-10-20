using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.RestApi.Client;


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
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.AdminUser, Integration.Password);

            var calendar = await client.TradingCalander.Get(2019);

            calendar.AddNonTradingDay(new Date(2019, 04, 01), "April Fools Day");
            await client.TradingCalander.Update(calendar);

            var response = await client.TradingCalander.Get(2019);

            response.NonTradingDays.Should().Contain(x => x.Date == new Date(2019, 04, 01) && x.Description == "April Fools Day");
        }

    }
}
