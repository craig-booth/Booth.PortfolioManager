using Booth.Common;
using Booth.PortfolioManager.Web.Models.Stock;
using Booth.PortfolioManager.Web.Models.User;
using Booth.PortfolioManager.Web.Serialization;
using FluentAssertions;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Booth.PortfolioManager.IntegrationTest
{
    [Collection(Integration.Collection)]
    public class AuthorizationTests
    { 
        private readonly IntegrationTestFixture _Fixture;
        public AuthorizationTests(IntegrationTestFixture fixture)
        {
            _Fixture = fixture;         
        }

        [Fact]
        public async Task AnonymousUserShouldNotHaveAccess()
        {
            var httpClient = _Fixture.CreateClient();

            Func<Task> a = () => httpClient.GetAsync<StockResponse>($"https://integrationtest.com/api/stocks/{Integration.StockId}", TestContext.Current.CancellationToken);

            (await a.Should().ThrowAsync<RestApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized); 
        }

        [Fact]
        public async Task StandardUserHasReadAccess()
        {
            var httpClient = _Fixture.CreateClient();

            var authenticated = await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);
            authenticated.Should().Be(true);

            var response = await httpClient.GetAsync<StockResponse>($"https://integrationtest.com/api/stocks/{Integration.StockId}", TestContext.Current.CancellationToken);
            response.Should().NotBeNull();
        }

        [Fact]
        public async Task StandardUserShouldNotHaveUpdateAccess()
        {
            var httpClient = _Fixture.CreateClient();

            var authenticated = await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);
            authenticated.Should().Be(true);

            var command = new CreateStockCommand()
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                ListingDate = new Date(2021, 01, 02),
                AsxCode = _Fixture.GenerateUniqueAsxCode(),
                Category = AssetCategory.AustralianStocks,
            };

            Func<Task> a = () => httpClient.PostAsync<CreateStockCommand>("https://integrationtest.com/api/stocks/", command, TestContext.Current.CancellationToken);

            (await a.Should().ThrowAsync<RestApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.Forbidden); 
        }

        [Fact]
        public async Task AdminUserHasUpdateAccess()
        {
            var httpClient = _Fixture.CreateClient();

            var authenticated = await httpClient.AuthenticateAsync(Integration.AdminUser, Integration.Password, TestContext.Current.CancellationToken);
            authenticated.Should().Be(true);

            var command = new CreateStockCommand()
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                ListingDate = new Date(2021, 01, 02),
                AsxCode = _Fixture.GenerateUniqueAsxCode(),
                Category = AssetCategory.AustralianStocks,
            };

            await httpClient.PostAsync<CreateStockCommand>("https://integrationtest.com/api/stocks/", command, TestContext.Current.CancellationToken); 
        }

    }

    

}
