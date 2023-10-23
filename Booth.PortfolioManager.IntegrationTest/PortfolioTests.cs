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
    public class PortfolioTests
    {

        private readonly IntegrationTestFixture _Fixture;

        public PortfolioTests(IntegrationTestFixture fixture)
        {
            _Fixture = fixture;
        }

        [Fact]
        public async Task CreatePortfolio()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);

            var portfolioId = Guid.NewGuid();
            var command = new CreatePortfolioCommand()
            {
                Id = portfolioId,
                Name = "Test Portfolio"
            };
            await client.Portfolio.CreatePortfolio(command);

            client.SetPortfolio(portfolioId);
            var properties = await client.Portfolio.GetProperties();

            properties.Should().BeEquivalentTo(new { Id = portfolioId, Name = "Test Portfolio" });
        }


        [Fact]
        public async Task GetProperties()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            var response = await client.Portfolio.GetProperties();

            response.Name.Should().Be("Test");
        }


        [Fact]
        public async Task GetSummary()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            var response = await client.Portfolio.GetSummary(new Date(2022, 01, 01));

            response.PortfolioValue.Should().Be(11941.10m);
        }

        [Fact]
        public async Task GetPerformance()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            var response = await client.Portfolio.GetPerformance(new DateRange(Date.MinValue, new Date(2022, 01, 01)));

            response.ChangeInMarketValue.Should().Be(2791.00m);
        }

        [Fact]
        public async Task GetValue()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            var response = await client.Portfolio.GetValue(new DateRange(new Date(2022, 01, 01), new Date(2022, 06, 30)), ValueFrequency.Month);

            response.Values.Should().HaveCount(7);
        }

        [Fact]
        public async Task GetTransactions()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            var response = await client.Portfolio.GetTransactions(new DateRange(Date.MinValue, Date.MaxValue));

            response.Transactions.Should().HaveCount(5);
        }

        [Fact]
        public async Task GetCapitalGains()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            var response = await client.Portfolio.GetCapitalGains(new Date(2022, 01, 01));

            response.UnrealisedGains[0].CapitalGain.Should().Be(1224.03m);
        }

        [Fact]
        public async Task GetDetailedCapitalGains()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            var response = await client.Portfolio.GetDetailedCapitalGains(new Date(2022, 01, 01));

            response.UnrealisedGains[0].AquisitionDate.Should().Be(new Date(2020, 01, 10));
        }

        [Fact]
        public async Task GetCGTLiability()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            var response = await client.Portfolio.GetCGTLiability(new DateRange(new Date(2020, 07, 01), new Date(2021, 06, 30)));

            response.CurrentYearCapitalGainsTotal.Should().Be(1517.07m);
        }

        [Fact]
        public async Task GetCashAccount()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            var response = await client.Portfolio.GetCashAccount(new DateRange(Date.MinValue, Date.MaxValue));

            response.ClosingBalance.Should().Be(9694.10m);
        }

        [Fact]
        public async Task GetIncome()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            var response = await client.Portfolio.GetIncome(new DateRange(Date.MinValue, Date.MaxValue));

            response.Income.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetCorporateActions()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            var response = await client.Portfolio.GetCorporateActions();

            response.CorporateActions.Should().HaveCount(2);
        }
    }
}
