using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Web.Models.Portfolio;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;


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
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var portfolioId = Guid.NewGuid();
            var command = new CreatePortfolioCommand()
            {
                Id = portfolioId,
                Name = "Test Portfolio"
            };
            await httpClient.PostAsync<CreatePortfolioCommand>("https://integrationtest.com/api/portfolio", command, TestContext.Current.CancellationToken);

            var properties = await httpClient.GetAsync<PortfolioPropertiesResponse>($"https://integrationtest.com/api/portfolio/{portfolioId}/properties", TestContext.Current.CancellationToken);

            properties.Should().BeEquivalentTo(new { Id = portfolioId, Name = "Test Portfolio" });
        }


        [Fact]
        public async Task GetProperties()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<PortfolioPropertiesResponse>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/properties", TestContext.Current.CancellationToken);

            response.Name.Should().Be("Test"); 
        }


        [Fact]
        public async Task GetSummary()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<PortfolioSummaryResponse>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/summary?date=2022-01-01", TestContext.Current.CancellationToken); 

            response.PortfolioValue.Should().Be(11941.10m);
        }

        [Fact]
        public async Task GetPerformance()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<PortfolioPerformanceResponse>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/performance?fromdate=0001-01-01&todate=2022-01-01", TestContext.Current.CancellationToken); 

            response.ChangeInMarketValue.Should().Be(2791.00m); 
        }

        [Fact]
        public async Task GetValue()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<PortfolioValueResponse>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/value?fromdate=2022-01-01&todate=2022-06-30&frequency=month", TestContext.Current.CancellationToken); 

            response.Values.Should().HaveCount(7); 
        }

        [Fact]
        public async Task GetTransactions()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<TransactionsResponse>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/transactions?fromdate=0001-01-01&todate=2999-01-01", TestContext.Current.CancellationToken); 

            response.Transactions.Should().HaveCount(5); 
        }

        [Fact]
        public async Task GetCapitalGains()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<SimpleUnrealisedGainsResponse>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/capitalgains?date=2022-01-01", TestContext.Current.CancellationToken); 

            response.UnrealisedGains[0].CapitalGain.Should().Be(1224.03m);
        }

        [Fact]
        public async Task GetDetailedCapitalGains()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<DetailedUnrealisedGainsResponse>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/detailedcapitalgains?date=2022-01-01", TestContext.Current.CancellationToken);

            response.UnrealisedGains[0].AquisitionDate.Should().Be(new Date(2020, 01, 10));
        }

        [Fact]
        public async Task GetCGTLiability()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<CgtLiabilityResponse>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/cgtliability?fromdate=2020-07-01&todate=2021-06-30", TestContext.Current.CancellationToken);

            response.CurrentYearCapitalGainsTotal.Should().Be(1517.07m);
        }

        [Fact]
        public async Task GetCashAccount()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<CashAccountTransactionsResponse>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/cashaccount?fromdate=0001-01-01&todate=2099-06-30", TestContext.Current.CancellationToken);

            response.ClosingBalance.Should().Be(9694.10m);
        }

        [Fact]
        public async Task GetIncome()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<IncomeResponse>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/income?fromdate=0001-01-01&todate=2099-06-30", TestContext.Current.CancellationToken);

            response.Income.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetCorporateActions()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<CorporateActionsResponse>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/corporateactions", TestContext.Current.CancellationToken); 

            response.CorporateActions.Should().HaveCount(2);
        }
    }
}
