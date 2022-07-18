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
    public class AuthorizationTests  : IClassFixture<AppTestFixture>
    {
        private AppTestFixture _Fixture;
        public AuthorizationTests(AppTestFixture fixture)
        {
            _Fixture = fixture;         
        }

        [Fact]
        public void AnonymousUserShouldNotHaveAccess()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");

            Func<Task> a = async () => await client.Stocks.Get(Ids.BHP);

            a.Should().Throw<RestException>().Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async void StandardUserHasReadAccess()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate("StandardUser", "secret");

            var response = await client.Stocks.Get(Ids.BHP);

            response.Should().NotBeNull();
        }

        [Fact]
        public async void StandardUserShouldNotHaveUpdateAccess()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate("StandardUser", "secret");

            var command = new ChangeStockCommand()
            {
                Id = Ids.BHP,
                ChangeDate = new Date(2013, 01, 02),
                AsxCode = "ABC",
                Category = RestApi.Stocks.AssetCategory.AustralianStocks,
            };

            Func<Task> a = async () => await client.Stocks.ChangeStock(command);

            a.Should().Throw<RestException>().Which.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async void AdminUserHasUpdateAccess()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate("AdminUser", "secret");

            var command = new ChangeStockCommand()
            {
                Id = Ids.BHP,
                ChangeDate = new Date(2013, 01, 02),
                AsxCode = "ABC",
                Category = RestApi.Stocks.AssetCategory.AustralianStocks,
            };

            await client.Stocks.ChangeStock(command);
        }

    }

    

}
