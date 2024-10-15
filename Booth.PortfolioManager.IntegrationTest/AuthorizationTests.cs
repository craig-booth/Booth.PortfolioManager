using System;
using System.Net;
using System.Threading.Tasks;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.RestApi.Client;
using Booth.PortfolioManager.RestApi.Stocks;

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
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");

            Func<Task> a = () => client.Stocks.Get(Integration.StockId);

            (await a.Should().ThrowAsync<RestException>()).Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task StandardUserHasReadAccess()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);

            var response = await client.Stocks.Get(Integration.StockId);

            response.Should().NotBeNull();
        }

        [Fact]
        public async Task StandardUserShouldNotHaveUpdateAccess()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);

            var command = new CreateStockCommand()
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                ListingDate = new Date(2021, 01, 02),
                AsxCode = _Fixture.GenerateUniqueAsxCode(),
                Category = AssetCategory.AustralianStocks,
            };

            Func<Task> a = () => client.Stocks.CreateStock(command);

            (await a.Should().ThrowAsync<RestException>()).Which.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AdminUserHasUpdateAccess()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.AdminUser, Integration.Password);


            var command = new CreateStockCommand()
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                ListingDate = new Date(2021, 01, 02),
                AsxCode = _Fixture.GenerateUniqueAsxCode(),
                Category = AssetCategory.AustralianStocks,
            };

            await client.Stocks.CreateStock(command);
        }

    }

    

}
