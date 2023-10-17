using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.RestApi.Client;
using Booth.PortfolioManager.RestApi.Portfolios;
using Booth.PortfolioManager.RestApi.Stocks;

namespace Booth.PortfolioManager.IntegrationTest
{
    [Collection(Integration.Collection)]
    public class CorporateActionTests
    {
        private readonly IntegrationTestFixture _Fixture;
        public CorporateActionTests(IntegrationTestFixture fixture)
        {
            _Fixture = fixture;
        }

        [Fact]
        public async Task AddSingleCorporateAction()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.AdminUser, Integration.Password);

            var stockId = Guid.NewGuid();
            var command = new CreateStockCommand()
            {
                Id = stockId,
                ListingDate = new Date(2010, 01, 01),
                AsxCode = "AAA",
                Name = "Test",
                Category = AssetCategory.AustralianStocks
            };
            await client.Stocks.CreateStock(command);

            var corporateAction = new RestApi.CorporateActions.CapitalReturn()
            {
                Id = Guid.NewGuid(),
                Stock = stockId,
                Description = "Action 1",
                ActionDate = new Date(2012, 01, 15),
                PaymentDate = new Date(2012, 01, 30),
                Amount = 10.00m              
            };
            await client.CorporateActions.Add(stockId, corporateAction);

            var response = await client.CorporateActions.GetAll(stockId, new DateRange(Date.MinValue, Date.MaxValue));

            response.Should().HaveCount(1);
        }

        [Fact]
        public async Task AddMultipleCorporateActions()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.AdminUser, Integration.Password);

            var stockId = Guid.NewGuid();
            var command = new CreateStockCommand()
            {
                Id = stockId,
                ListingDate = new Date(2010, 01, 01),
                AsxCode = "BBB",
                Name = "Test",
                Category = AssetCategory.AustralianStocks
            };
            await client.Stocks.CreateStock(command);

            var corporateAction1 = new RestApi.CorporateActions.CapitalReturn()
            {
                Id = Guid.NewGuid(),
                Stock = stockId,
                Description = "Action 1",
                ActionDate = new Date(2012, 01, 15),
                PaymentDate = new Date(2012, 01, 30),
                Amount = 10.00m
            };
            await client.CorporateActions.Add(stockId, corporateAction1);

            var corporateAction2 = new RestApi.CorporateActions.SplitConsolidation()
            {
                Id = Guid.NewGuid(),
                Stock = stockId,
                Description = "Action 2",
                ActionDate = new Date(2012, 02, 15),
                NewUnits = 2,
                OriginalUnits = 1
            };
            await client.CorporateActions.Add(stockId, corporateAction2);

            var response = await client.CorporateActions.GetAll(stockId, new DateRange(Date.MinValue, Date.MaxValue));

            response.Should().HaveCount(2);
        }

        [Fact]
        public async Task UpdateCorporateAction()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.AdminUser, Integration.Password);

            var stockId = Guid.NewGuid();
            var command = new CreateStockCommand()
            {
                Id = stockId,
                ListingDate = new Date(2010, 01, 01),
                AsxCode = "CCC",
                Name = "Test",
                Category = AssetCategory.AustralianStocks
            };
            await client.Stocks.CreateStock(command);

            var corporateAction1 = new RestApi.CorporateActions.CapitalReturn()
            {
                Id = Guid.NewGuid(),
                Stock = stockId,
                Description = "Action 1",
                ActionDate = new Date(2012, 01, 15),
                PaymentDate = new Date(2012, 01, 30),
                Amount = 10.00m
            };
            await client.CorporateActions.Add(stockId, corporateAction1);

            var corporateAction2 = new RestApi.CorporateActions.SplitConsolidation()
            {
                Id = Guid.NewGuid(),
                Stock = stockId,
                Description = "Action 2",
                ActionDate = new Date(2012, 03, 15),
                NewUnits = 2,
                OriginalUnits = 1
            };
            await client.CorporateActions.Add(stockId, corporateAction2);

            var corporateAction3 = new RestApi.CorporateActions.CapitalReturn()
            {
                Id = Guid.NewGuid(),
                Stock = stockId,
                Description = "Action 3",
                ActionDate = new Date(2013, 02, 15),
                PaymentDate = new Date(2013, 02, 28),
                Amount = 20.00m
            };
            await client.CorporateActions.Add(stockId, corporateAction3);


            corporateAction2.NewUnits = 3;
            await client.CorporateActions.Update(stockId, corporateAction2);

            var response = await client.CorporateActions.GetAll(stockId, new DateRange(Date.MinValue, Date.MaxValue));
            response.Should().HaveCount(3);


            var action = await client.CorporateActions.Get(stockId, corporateAction2.Id);

            ((RestApi.CorporateActions.SplitConsolidation)action).NewUnits.Should().Be(3);
        }

        [Fact]
        public async Task DeleteCorporateAction()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.AdminUser, Integration.Password);

            var stockId = Guid.NewGuid();
            var command = new CreateStockCommand()
            {
                Id = stockId,
                ListingDate = new Date(2010, 01, 01),
                AsxCode = "DDD",
                Name = "Test",
                Category = AssetCategory.AustralianStocks
            };
            await client.Stocks.CreateStock(command);

            var corporateAction1 = new RestApi.CorporateActions.CapitalReturn()
            {
                Id = Guid.NewGuid(),
                Stock = stockId,
                Description = "Action 1",
                ActionDate = new Date(2012, 01, 15),
                PaymentDate = new Date(2012, 01, 30),
                Amount = 10.00m
            };
            await client.CorporateActions.Add(stockId, corporateAction1);

            var corporateAction2 = new RestApi.CorporateActions.SplitConsolidation()
            {
                Id = Guid.NewGuid(),
                Stock = stockId,
                Description = "Action 2",
                ActionDate = new Date(2012, 03, 15),
                NewUnits = 2,
                OriginalUnits = 1
            };
            await client.CorporateActions.Add(stockId, corporateAction2);

            var corporateAction3 = new RestApi.CorporateActions.CapitalReturn()
            {
                Id = Guid.NewGuid(),
                Stock = stockId,
                Description = "Action 3",
                ActionDate = new Date(2013, 02, 15),
                PaymentDate = new Date(2013, 02, 28),
                Amount = 20.00m
            };
            await client.CorporateActions.Add(stockId, corporateAction3);


            await client.CorporateActions.Delete(stockId, corporateAction2.Id);

            var response = await client.CorporateActions.GetAll(stockId, new DateRange(Date.MinValue, Date.MaxValue));
            response.Select(x => x.Id).Should().BeEquivalentTo(new[] { corporateAction1.Id, corporateAction3.Id });
        }
    }
}
