using System;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.EventStore;
using Booth.EventStore.Memory;
using Booth.PortfolioManager.Web;
using Booth.PortfolioManager.RestApi.Client;
using Booth.PortfolioManager.RestApi.Stocks;
using Booth.PortfolioManager.Domain.Users;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.IntegrationTest
{
    public class AuthorizationTests  : IClassFixture<AppTestFixture>
    {
        private AppTestFixture _Fixture;
        private SecureString _Password;
        public AuthorizationTests(AppTestFixture fixture)
        {
            _Fixture = fixture;
            
            _Password = new SecureString();
            _Password.AppendChar('s');
            _Password.AppendChar('e');
            _Password.AppendChar('c');
            _Password.AppendChar('r');
            _Password.AppendChar('e');
            _Password.AppendChar('t');
        }

        [Fact]
        public void AnonymousUserShouldNotHaveAccess()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");

            Func<Task> a = async () => await client.Stocks.Get(StockId.BHP);

            a.Should().Throw<RestException>().Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async void StandardUserHasReadAccess()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate("StandardUser", _Password);

            var response = await client.Stocks.Get(StockId.BHP);

            response.Should().NotBeNull();
        }

        [Fact]
        public async void StandardUserShouldNotHaveUpdateAccess()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate("StandardUser", _Password);

            var command = new ChangeStockCommand()
            {
                Id = StockId.BHP,
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
            await client.Authenticate("AdminUser", _Password);

            var command = new ChangeStockCommand()
            {
                Id = StockId.BHP,
                ChangeDate = new Date(2013, 01, 02),
                AsxCode = "ABC",
                Category = RestApi.Stocks.AssetCategory.AustralianStocks,
            };

            await client.Stocks.ChangeStock(command);
        }

    }

    static class StockId
    {
        public static Guid BHP = Guid.NewGuid();
    }

    public class AppTestFixture: WebApplicationFactory<Startup>
    {

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var eventStore = new MemoryEventStore();

            AddUsers(eventStore);
            AddStocks(eventStore);
            AddPortfolios(eventStore);

            builder.ConfigureTestServices(x =>
                {
                    x.AddSingleton<IEventStore>(eventStore);
                    x.RemoveAll<IHostedService>();
                });
        }

        private void AddUsers(IEventStore eventStore)
        {
            var eventStream = eventStore.GetEventStream<User>("Users");

            var standardUser = new User(Guid.NewGuid());
            standardUser.Create("StandardUser", "secret");
            eventStream.Add(standardUser.Id, "User", standardUser.GetProperties(), standardUser.FetchEvents());

            var administrator = new User(Guid.NewGuid());
            administrator.Create("AdminUser", "secret");
            administrator.AddAdministratorPrivilage();
            eventStream.Add(administrator.Id, "User", administrator.GetProperties(), administrator.FetchEvents());
        }

        private void AddStocks(IEventStore eventStore)
        {
            var eventStream = eventStore.GetEventStream<Stock>("Stocks");

            var stock = new Stock(StockId.BHP);
            stock.List("BHP", "BHP Pty Ltd", new Date(2000, 01, 01), false, Domain.Stocks.AssetCategory.AustralianStocks);
            eventStream.Add(stock.Id, "Stock", stock.FetchEvents());
        }

        private void AddPortfolios(IEventStore eventStore)
        {

        }
    }

}
