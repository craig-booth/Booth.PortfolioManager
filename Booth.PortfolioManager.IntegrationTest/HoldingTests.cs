using System;
using System.Threading.Tasks;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.RestApi.Client;
using Booth.PortfolioManager.RestApi.Portfolios;


namespace Booth.PortfolioManager.IntegrationTest
{
    [Collection(Integration.Collection)]
    public class HoldingTests
    {
        private readonly IntegrationTestFixture _Fixture;

        
        public HoldingTests(IntegrationTestFixture fixture)
        {
            _Fixture = fixture;
        }


        [Fact]
        public async Task ListHoldings()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            var response = await client.Holdings.Get(new Date(2020, 01, 15));

            response.Should().HaveCount(1);
        }


        [Fact]
        public async Task GetHolding()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            var response = await client.Holdings.Get(Integration.StockId, new Date(2020, 01, 15));

            response.Units.Should().Be(100);
        }

        [Fact]
        public async Task ChangeDrpParticipation()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            await client.Holdings.ChangeDrpParticipation(Integration.StockId, true);

            var response = await client.Portfolio.GetProperties();
            response.Holdings.Should().Contain(x => x.Stock.Id == Integration.StockId).Which.ParticipatingInDrp.Should().BeTrue();
        }

        [Fact]
        public async Task GetValue()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            var response = await client.Holdings.GetValue(Integration.StockId, new DateRange(new Date(2020, 01, 10), new Date(2020, 01, 15)), ValueFrequency.Day);

            response.Values.Should().HaveCount(4);
        }

        [Fact]
        public async Task GetCapitalGains()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            var response = await client.Holdings.GetCapitalGains(Integration.StockId, new Date(2020, 01, 15));

            response.UnrealisedGains.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetDetailedCapitalGains()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            var response = await client.Holdings.GetDetailedCapitalGains(Integration.StockId, new Date(2020, 01, 15));

            response.UnrealisedGains.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetCorporateActions()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            var response = await client.Holdings.GetCorporateActions(Integration.StockId);

            response.CorporateActions.Should().HaveCount(2);
        }
    }
}
