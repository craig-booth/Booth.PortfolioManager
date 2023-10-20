using System;
using System.Threading.Tasks;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.RestApi.Client;
using Booth.PortfolioManager.RestApi.Stocks;
using MongoDB.Driver;

namespace Booth.PortfolioManager.IntegrationTest
{
    [Collection(Integration.Collection)]
    public class StockTests
    {
        private readonly IntegrationTestFixture _Fixture;

        public StockTests(IntegrationTestFixture fixture)
        {
            _Fixture = fixture;
        }

        [Fact]
        public async Task CreateStock()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.AdminUser, Integration.Password);

            var stockId = Guid.NewGuid();
            var asxCode = _Fixture.GenerateUniqueAsxCode();
            var command = new CreateStockCommand()
            {
                Id = stockId,
                ListingDate = new Date(2003, 06, 01),
                AsxCode = asxCode,
                Name = "Test",
                Trust = false,
                Category = AssetCategory.AustralianStocks
            };
            await client.Stocks.CreateStock(command);

            var stock = await client.Stocks.Get(stockId);

            stock.Should().BeEquivalentTo(new { Id = stockId, AsxCode = asxCode, Name = "Test", Category = AssetCategory.AustralianStocks, Trust = false, ListingDate = new Date(2003, 06, 01) });
        }

        [Fact]
        public async Task DelistStock()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.AdminUser, Integration.Password);

            var stockId = Guid.NewGuid();
            var asxCode = _Fixture.GenerateUniqueAsxCode();
            var createCommand = new CreateStockCommand()
            {
                Id = stockId,
                ListingDate = new Date(2003, 06, 01),
                AsxCode = asxCode,
                Name = "Test",
                Trust = false,
                Category = AssetCategory.AustralianStocks
            };
            await client.Stocks.CreateStock(createCommand);

            var delistCommand = new DelistStockCommand()
            {
                Id = stockId,
                DelistingDate = new Date(2010, 01, 01)
            };
            await client.Stocks.DelistStock(delistCommand);

            var stock = await client.Stocks.Get(stockId);

            stock.Should().BeEquivalentTo(new { Id = stockId, AsxCode = asxCode, Name = "Test", Category = AssetCategory.AustralianStocks, Trust = false, ListingDate = new Date(2003, 06, 01), DelistedDate = new Date(2010, 01, 01) });
        }

        [Fact]
        public async Task ChangeStock()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.AdminUser, Integration.Password);

            var stockId = Guid.NewGuid();
            
            var createCommand = new CreateStockCommand()
            {
                Id = stockId,
                ListingDate = new Date(2003, 06, 01),
                AsxCode = _Fixture.GenerateUniqueAsxCode(),
                Name = "Test",
                Trust = false,
                Category = AssetCategory.AustralianStocks
            };
            await client.Stocks.CreateStock(createCommand);

            var asxCode = _Fixture.GenerateUniqueAsxCode();
            var changeCommand = new ChangeStockCommand()
            {
                Id = stockId,
                ChangeDate = new Date(2005, 01, 01),
                AsxCode = asxCode,
                Name = "New Name"
            };
            await client.Stocks.ChangeStock(changeCommand);

            var stock = await client.Stocks.Get(stockId);

            stock.Should().BeEquivalentTo(new { Id = stockId, AsxCode = asxCode, Name = "New Name", });

        }

        [Fact]
        public async Task ChangeDividendRules()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.AdminUser, Integration.Password);

            var stockId = Guid.NewGuid();
            var asxCode = _Fixture.GenerateUniqueAsxCode();
            var createCommand = new CreateStockCommand()
            {
                Id = stockId,
                ListingDate = new Date(2003, 06, 01),
                AsxCode = asxCode,
                Name = "Test",
                Trust = false,
                Category = AssetCategory.AustralianStocks
            };
            await client.Stocks.CreateStock(createCommand);

            var changeCommand = new ChangeDividendRulesCommand()
            {
                Id = stockId,
                ChangeDate = new Date(2005, 01, 01),
                CompanyTaxRate = 0.40m
            };
            await client.Stocks.ChangeDividendRules(changeCommand);

            var stock = await client.Stocks.Get(stockId);

            stock.Should().BeEquivalentTo(new { Id = stockId, AsxCode = asxCode, Name = "Test", CompanyTaxRate = 0.40m });

        }

        [Fact]
        public async Task UpdateClosingPrices()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.AdminUser, Integration.Password);

            var stockId = Guid.NewGuid();
            var createCommand = new CreateStockCommand()
            {
                Id = stockId,
                ListingDate = new Date(2003, 06, 01),
                AsxCode = _Fixture.GenerateUniqueAsxCode(),
                Name = "Test",
                Trust = false,
                Category = AssetCategory.AustralianStocks
            };
            await client.Stocks.CreateStock(createCommand);

            var updateCommand = new UpdateClosingPricesCommand()
            {
                Id = stockId
            };
            updateCommand.AddClosingPrice(new Date(2003, 06, 01), 1.30m);
            updateCommand.AddClosingPrice(new Date(2003, 06, 02), 1.35m);
            updateCommand.AddClosingPrice(new Date(2003, 06, 03), 1.32m);
            await client.Stocks.UpdateClosingPrices(updateCommand);

            var prices = await client.Stocks.GetPrices(stockId, new DateRange(new Date(2003, 06, 01), new Date(2003, 06, 30)));

            prices.ClosingPrices.Should().HaveCount(3);

        }

        [Fact]
        public async Task Get()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.AdminUser, Integration.Password);

            var stock = await client.Stocks.Get(Integration.StockId);

            stock.Should().BeEquivalentTo(new { Id = Integration.StockId, AsxCode = "ABC", Name = "Test Company" });

        }

        [Fact]
        public async Task GetHistory()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.AdminUser, Integration.Password);

            var stock = await client.Stocks.GetHistory(Integration.StockId);

            stock.Should().BeEquivalentTo(new { Id = Integration.StockId, AsxCode = "ABC", Name = "Test Company" });
        }

        [Fact]
        public async Task GetClosingPrices()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.AdminUser, Integration.Password);

            var prices = await client.Stocks.GetPrices(Integration.StockId, new DateRange(new Date(2020, 01, 01), new Date(2020, 01, 10)));

            prices.ClosingPrices.Should().HaveCount(7);
        }
    }



}
