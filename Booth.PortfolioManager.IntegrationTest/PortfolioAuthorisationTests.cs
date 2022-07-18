using System;
using System.Net;
using System.Threading.Tasks;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.RestApi.Client;
using Booth.PortfolioManager.RestApi.Stocks;
using Booth.PortfolioManager.IntegrationTest.TestFixture;

namespace Booth.PortfolioManager.IntegrationTest
{
    public class PortfolioAuthorisationTests : IClassFixture<AppTestFixture>
    {
        private AppTestFixture _Fixture;
        public PortfolioAuthorisationTests(AppTestFixture fixture)
        {
            _Fixture = fixture;
        }

        [Fact]
        public void AnonymousUserShouldNotHaveAccess()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            client.SetPortfolio(Ids.PortfolioId);

            Func<Task> a = async () => await client.Portfolio.GetProperties();

            a.Should().Throw<RestException>().Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async void UserThatIsNotPortfolioOwnerShouldNotHaveAccess()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate("StandardUser2", "secret");
            client.SetPortfolio(Ids.PortfolioId);

            Func<Task> a = async () => await client.Portfolio.GetProperties();

            a.Should().Throw<RestException>().Which.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async void PortfolioOwnerShouldHaveAccess()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate("StandardUser", "secret");
            client.SetPortfolio(Ids.PortfolioId);

            var response = await client.Portfolio.GetProperties();

            response.Should().BeEquivalentTo(new
            {
                Id = Ids.PortfolioId,
                StartDate = new Date(2000, 01, 01),
                EndDate = Date.MaxValue
            });
        }
    }
}
