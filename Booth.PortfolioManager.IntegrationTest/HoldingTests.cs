using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Web.Models.Portfolio;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;



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
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<List<Holding>>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/holdings?date=2020-01-15", TestContext.Current.CancellationToken); 

            response.Should().HaveCount(1);
        }


        [Fact]
        public async Task GetHolding()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<Holding>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/holdings/{Integration.StockId}/?date=2020-01-15", TestContext.Current.CancellationToken);

            response.Units.Should().Be(100); 
        }

        [Fact]
        public async Task ChangeDrpParticipation()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var command = new ChangeDrpParticipationCommand()
            {
                Holding = Integration.StockId,
                Participate = true
            };
            await httpClient.PostAsync<ChangeDrpParticipationCommand>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/holdings/{Integration.StockId}/changedrpparticipation", command, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<PortfolioPropertiesResponse>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/properties", TestContext.Current.CancellationToken);
            response.Holdings.Should().Contain(x => x.Stock.Id == Integration.StockId).Which.ParticipatingInDrp.Should().BeTrue(); 
        }

        [Fact]
        public async Task GetValue()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<PortfolioValueResponse>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/holdings/{Integration.StockId}/value?fromdate=2020-01-10&todate=2020-01-15&frequency=day", TestContext.Current.CancellationToken); 

            response.Values.Should().HaveCount(4); 
        }

        [Fact]
        public async Task GetCapitalGains()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<SimpleUnrealisedGainsResponse>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/holdings/{Integration.StockId}/capitalgains?date=2020-01-15", TestContext.Current.CancellationToken); 

            response.UnrealisedGains.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetDetailedCapitalGains()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<DetailedUnrealisedGainsResponse>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/holdings/{Integration.StockId}/detailedcapitalgains?date=2020-01-15", TestContext.Current.CancellationToken); 

            response.UnrealisedGains.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetCorporateActions()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<CorporateActionsResponse>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/holdings/{Integration.StockId}/corporateactions", TestContext.Current.CancellationToken); 

            response.CorporateActions.Should().HaveCount(2); 
        }
    }
}
