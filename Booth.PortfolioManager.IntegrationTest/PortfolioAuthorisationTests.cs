using System;
using System.Net;
using System.Threading.Tasks;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Web.Models.Portfolio;

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
        public async Task AnonymousUserShouldNotHaveAccess()
        {
            var httpClient = _Fixture.CreateClient();

            Func<Task> a = () => httpClient.GetAsync<PortfolioPropertiesResponse>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/properties", TestContext.Current.CancellationToken);

            (await a.Should().ThrowAsync<RestApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized); 
        }

        [Fact]
        public async Task UserThatIsNotPortfolioOwnerShouldNotHaveAccess()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User2, Integration.Password, TestContext.Current.CancellationToken);

            Func<Task> a = () => httpClient.GetAsync<PortfolioPropertiesResponse>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/properties", TestContext.Current.CancellationToken);

            (await a.Should().ThrowAsync<RestApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task PortfolioOwnerShouldHaveAccess()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<PortfolioPropertiesResponse>($"https://integrationtest.com/api/portfolio/{Integration.PortfolioId}/properties", TestContext.Current.CancellationToken);

            response.Should().BeEquivalentTo(new
            {
                Id = Integration.PortfolioId,
                StartDate = new Date(2020, 01, 01),
                EndDate = Date.MaxValue
            }); 
        }
    }
}
