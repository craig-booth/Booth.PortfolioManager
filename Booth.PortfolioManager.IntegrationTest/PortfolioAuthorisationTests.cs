using System;
using System.Net;
using System.Threading.Tasks;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.RestApi.Client;

namespace Booth.PortfolioManager.IntegrationTest
{
    [Collection(Integration.Collection)]
    public class PortfolioAuthorisationTests
    {
        private readonly IntegrationTestFixture _Fixture;
        public PortfolioAuthorisationTests(IntegrationTestFixture fixture)
        {
            _Fixture = fixture;
        }

        [Fact]
        public async void AnonymousUserShouldNotHaveAccess()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            client.SetPortfolio(Integration.PortfolioId);

            Func<Task> a = () => client.Portfolio.GetProperties();

            (await a.Should().ThrowAsync<RestException>()).Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async void UserThatIsNotPortfolioOwnerShouldNotHaveAccess()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User2, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            Func<Task> a = () => client.Portfolio.GetProperties();

            (await a.Should().ThrowAsync<RestException>()).Which.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async void PortfolioOwnerShouldHaveAccess()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);
            client.SetPortfolio(Integration.PortfolioId);

            var response = await client.Portfolio.GetProperties();

            response.Should().BeEquivalentTo(new
            {
                Id = Integration.PortfolioId,
                StartDate = new Date(2020, 01, 01),
                EndDate = Date.MaxValue
            });
        }
    }
}
