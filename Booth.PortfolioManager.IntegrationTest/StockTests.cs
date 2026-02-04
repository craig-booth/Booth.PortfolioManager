using Booth.Common;
using Booth.PortfolioManager.Web.Models.Stock;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

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
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.AdminUser, Integration.Password, TestContext.Current.CancellationToken);

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
            await httpClient.PostAsync<CreateStockCommand>("https://integrationtest.com/api/stocks/", command, TestContext.Current.CancellationToken);

            var stock = await httpClient.GetAsync<StockResponse>($"https://integrationtest.com/api/stocks/{stockId}", TestContext.Current.CancellationToken);

            stock.Should().BeEquivalentTo(new { Id = stockId, AsxCode = asxCode, Name = "Test", Category = AssetCategory.AustralianStocks, Trust = false, ListingDate = new Date(2003, 06, 01) });
        }

        [Fact]
        public async Task DelistStock()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.AdminUser, Integration.Password, TestContext.Current.CancellationToken);

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
            await httpClient.PostAsync<CreateStockCommand>("https://integrationtest.com/api/stocks/", createCommand, TestContext.Current.CancellationToken);

            var delistCommand = new DelistStockCommand()
            {
                Id = stockId,
                DelistingDate = new Date(2010, 01, 01)
            };
            await httpClient.PostAsync<DelistStockCommand>($"https://integrationtest.com/api/stocks/{stockId}/delist", delistCommand, TestContext.Current.CancellationToken);

            var stock = await httpClient.GetAsync<StockResponse>("https://integrationtest.com/api/stocks/" + stockId, TestContext.Current.CancellationToken);

            stock.Should().BeEquivalentTo(new { Id = stockId, AsxCode = asxCode, Name = "Test", Category = AssetCategory.AustralianStocks, Trust = false, ListingDate = new Date(2003, 06, 01), DelistedDate = new Date(2010, 01, 01) });
            }

        [Fact]
        public async Task ChangeStock()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.AdminUser, Integration.Password, TestContext.Current.CancellationToken);

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
            await httpClient.PostAsync<CreateStockCommand>("https://integrationtest.com/api/stocks/", createCommand, TestContext.Current.CancellationToken);

            var asxCode = _Fixture.GenerateUniqueAsxCode();
            var changeCommand = new ChangeStockCommand()
            {
                Id = stockId,
                ChangeDate = new Date(2005, 01, 01),
                AsxCode = asxCode,
                Name = "New Name"
            };
            await httpClient.PostAsync<ChangeStockCommand>($"https://integrationtest.com/api/stocks/{stockId}/change", changeCommand, TestContext.Current.CancellationToken);

            var stock = await httpClient.GetAsync<StockResponse>($"https://integrationtest.com/api/stocks/{stockId}", TestContext.Current.CancellationToken);

            stock.Should().BeEquivalentTo(new { Id = stockId, AsxCode = asxCode, Name = "New Name", });
        }

        [Fact]
        public async Task ChangeDividendRules()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.AdminUser, Integration.Password, TestContext.Current.CancellationToken);

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
            await httpClient.PostAsync<CreateStockCommand>("https://integrationtest.com/api/stocks/", createCommand, TestContext.Current.CancellationToken);

            var changeCommand = new ChangeDividendRulesCommand()
            {
                Id = stockId,
                ChangeDate = new Date(2005, 01, 01),
                CompanyTaxRate = 0.40m
            };
            await httpClient.PostAsync<ChangeDividendRulesCommand>($"https://integrationtest.com/api/stocks/{stockId}/changedividendrules", changeCommand, TestContext.Current.CancellationToken);

            var stock = await httpClient.GetAsync<StockResponse>($"https://integrationtest.com/api/stocks/{stockId}", TestContext.Current.CancellationToken);

            stock.Should().BeEquivalentTo(new { Id = stockId, AsxCode = asxCode, Name = "Test", CompanyTaxRate = 0.40m });
        }

        [Fact]
        public async Task UpdateClosingPrices()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.AdminUser, Integration.Password, TestContext.Current.CancellationToken);

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
            await httpClient.PostAsync<CreateStockCommand>("https://integrationtest.com/api/stocks/", createCommand, TestContext.Current.CancellationToken);

            var updateCommand = new UpdateClosingPricesCommand()
            {
                Id = stockId
            };
            updateCommand.AddClosingPrice(new Date(2003, 06, 01), 1.30m);
            updateCommand.AddClosingPrice(new Date(2003, 06, 02), 1.35m);
            updateCommand.AddClosingPrice(new Date(2003, 06, 03), 1.32m);
            await httpClient.PostAsync<UpdateClosingPricesCommand>($"https://integrationtest.com/api/stocks/{stockId}/closingprices", updateCommand, TestContext.Current.CancellationToken);

            var prices = await httpClient.GetAsync<StockPriceResponse>($"https://integrationtest.com/api/stocks/{stockId}/closingprices?fromdate=2003-06-01&todate=2003-06-30", TestContext.Current.CancellationToken);

            prices.ClosingPrices.Should().HaveCount(3);
        }

        [Fact]
        public async Task Get()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.AdminUser, Integration.Password, TestContext.Current.CancellationToken);

            var stock = await httpClient.GetAsync<StockResponse>($"https://integrationtest.com/api/stocks/{Integration.StockId}", TestContext.Current.CancellationToken);
 
            stock.Should().BeEquivalentTo(new { Id = Integration.StockId, AsxCode = "ABC", Name = "Test Company" });
        }

        [Fact]
        public async Task GetHistory()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.AdminUser, Integration.Password, TestContext.Current.CancellationToken);

            var stock = await httpClient.GetAsync<StockHistoryResponse>($"https://integrationtest.com/api/stocks/{Integration.StockId}/history", TestContext.Current.CancellationToken);

            stock.Should().BeEquivalentTo(new { Id = Integration.StockId, AsxCode = "ABC", Name = "Test Company" });
        }

        [Fact]
        public async Task GetClosingPrices()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.AdminUser, Integration.Password, TestContext.Current.CancellationToken);

            var prices = await httpClient.GetAsync<StockPriceResponse>($"https://integrationtest.com/api/stocks/{Integration.StockId}/closingprices?fromdate=2020-01-01&todate=2020-01-10", TestContext.Current.CancellationToken);

            prices.ClosingPrices.Should().HaveCount(7);
        }
    }



}
