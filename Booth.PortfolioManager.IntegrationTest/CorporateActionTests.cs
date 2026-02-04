using Booth.Common;
using Booth.PortfolioManager.Web.Models.CorporateAction;
using Booth.PortfolioManager.Web.Models.Stock;
using FluentAssertions;
using MongoDB.Driver.Core.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

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
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.AdminUser, Integration.Password, TestContext.Current.CancellationToken);

            var stockId = Guid.NewGuid();
            var command = new CreateStockCommand()
            {
                Id = stockId,
                ListingDate = new Date(2010, 01, 01),
                AsxCode = _Fixture.GenerateUniqueAsxCode(),
                Name = "Test",
                Category = AssetCategory.AustralianStocks
            };
            await httpClient.PostAsync<CreateStockCommand>("https://integrationtest.com/api/stocks/", command, TestContext.Current.CancellationToken);

            var corporateAction = new CapitalReturn()
            {
                Id = Guid.NewGuid(),
                Stock = stockId,
                Description = "Action 1",
                ActionDate = new Date(2012, 01, 15),
                PaymentDate = new Date(2012, 01, 30),
                Amount = 10.00m              
            };
            await httpClient.PostAsync<CorporateAction>($"https://integrationtest.com/api/stocks/{stockId}/corporateactions", corporateAction, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<List<CorporateAction>>($"https://integrationtest.com/api/stocks/{stockId}/corporateactions?startDate={Date.MinValue}&endDate={Date.MaxValue}", TestContext.Current.CancellationToken);

            response.Should().HaveCount(1); 
        }

        [Fact]
        public async Task AddMultipleCorporateActions()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.AdminUser, Integration.Password, TestContext.Current.CancellationToken);

            var stockId = Guid.NewGuid();
            var command = new CreateStockCommand()
            {
                Id = stockId,
                ListingDate = new Date(2010, 01, 01),
                AsxCode = _Fixture.GenerateUniqueAsxCode(),
                Name = "Test",
                Category = AssetCategory.AustralianStocks
            };
            await httpClient.PostAsync<CreateStockCommand>("https://integrationtest.com/api/stocks/", command, TestContext.Current.CancellationToken);

            var corporateAction1 = new CapitalReturn()
            {
                Id = Guid.NewGuid(),
                Stock = stockId,
                Description = "Action 1",
                ActionDate = new Date(2012, 01, 15),
                PaymentDate = new Date(2012, 01, 30),
                Amount = 10.00m
            };
            await httpClient.PostAsync<CorporateAction>($"https://integrationtest.com/api/stocks/{stockId}/corporateactions", corporateAction1, TestContext.Current.CancellationToken);

            var corporateAction2 = new SplitConsolidation()
            {
                Id = Guid.NewGuid(),
                Stock = stockId,
                Description = "Action 2",
                ActionDate = new Date(2012, 02, 15),
                NewUnits = 2,
                OriginalUnits = 1
            };
            await httpClient.PostAsync<CorporateAction>($"https://integrationtest.com/api/stocks/{stockId}/corporateactions", corporateAction2, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<List<CorporateAction>>($"https://integrationtest.com/api/stocks/{stockId}/corporateactions?startDate={Date.MinValue}&endDate={Date.MaxValue}", TestContext.Current.CancellationToken);

            response.Should().HaveCount(2); 
        }

        [Fact]
        public async Task UpdateCorporateAction()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.AdminUser, Integration.Password, TestContext.Current.CancellationToken);

            var stockId = Guid.NewGuid();
            var command = new CreateStockCommand()
            {
                Id = stockId,
                ListingDate = new Date(2010, 01, 01),
                AsxCode = _Fixture.GenerateUniqueAsxCode(),
                Name = "Test",
                Category = AssetCategory.AustralianStocks
            };
            await httpClient.PostAsync<CreateStockCommand>("https://integrationtest.com/api/stocks/", command, TestContext.Current.CancellationToken);

            var corporateAction1 = new CapitalReturn()
            {
                Id = Guid.NewGuid(),
                Stock = stockId,
                Description = "Action 1",
                ActionDate = new Date(2012, 01, 15),
                PaymentDate = new Date(2012, 01, 30),
                Amount = 10.00m
            };
            await httpClient.PostAsync<CorporateAction>($"https://integrationtest.com/api/stocks/{stockId}/corporateactions", corporateAction1, TestContext.Current.CancellationToken);

            var corporateAction2 = new SplitConsolidation()
            {
                Id = Guid.NewGuid(),
                Stock = stockId,
                Description = "Action 2",
                ActionDate = new Date(2012, 03, 15),
                NewUnits = 2,
                OriginalUnits = 1
            };
            await httpClient.PostAsync<CorporateAction>($"https://integrationtest.com/api/stocks/{stockId}/corporateactions", corporateAction2, TestContext.Current.CancellationToken);

            var corporateAction3 = new CapitalReturn()
            {
                Id = Guid.NewGuid(),
                Stock = stockId,
                Description = "Action 3",
                ActionDate = new Date(2013, 02, 15),
                PaymentDate = new Date(2013, 02, 28),
                Amount = 20.00m
            };
            await httpClient.PostAsync<CorporateAction>($"https://integrationtest.com/api/stocks/{stockId}/corporateactions", corporateAction3, TestContext.Current.CancellationToken);


            corporateAction2.NewUnits = 3;
            await httpClient.PostAsync<CorporateAction>($"https://integrationtest.com/api/stocks/{stockId}/corporateactions/{corporateAction2.Id}", corporateAction2, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<List<CorporateAction>>($"https://integrationtest.com/api/stocks/{stockId}/corporateactions?startDate={Date.MinValue}&endDate={Date.MaxValue}", TestContext.Current.CancellationToken);
            response.Should().HaveCount(3);


            var action = await httpClient.GetAsync<CorporateAction>($"https://integrationtest.com/api/stocks/{stockId}/corporateactions/{corporateAction2.Id}", TestContext.Current.CancellationToken);

            ((SplitConsolidation)action).NewUnits.Should().Be(3); 
        }

        [Fact]
        public async Task DeleteCorporateAction()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.AdminUser, Integration.Password, TestContext.Current.CancellationToken);

            var stockId = Guid.NewGuid();
            var command = new CreateStockCommand()
            {
                Id = stockId,
                ListingDate = new Date(2010, 01, 01),
                AsxCode = _Fixture.GenerateUniqueAsxCode(),
                Name = "Test",
                Category = AssetCategory.AustralianStocks
            };
            await httpClient.PostAsync<CreateStockCommand>("https://integrationtest.com/api/stocks/", command, TestContext.Current.CancellationToken);

            var corporateAction1 = new CapitalReturn()
            {
                Id = Guid.NewGuid(),
                Stock = stockId,
                Description = "Action 1",
                ActionDate = new Date(2012, 01, 15),
                PaymentDate = new Date(2012, 01, 30),
                Amount = 10.00m
            };
            await httpClient.PostAsync<CorporateAction>($"https://integrationtest.com/api/stocks/{stockId}/corporateactions", corporateAction1, TestContext.Current.CancellationToken);

            var corporateAction2 = new SplitConsolidation()
            {
                Id = Guid.NewGuid(),
                Stock = stockId,
                Description = "Action 2",
                ActionDate = new Date(2012, 03, 15),
                NewUnits = 2,
                OriginalUnits = 1
            };
            await httpClient.PostAsync<CorporateAction>($"https://integrationtest.com/api/stocks/{stockId}/corporateactions", corporateAction2, TestContext.Current.CancellationToken);

            var corporateAction3 = new CapitalReturn()
            {
                Id = Guid.NewGuid(),
                Stock = stockId,
                Description = "Action 3",
                ActionDate = new Date(2013, 02, 15),
                PaymentDate = new Date(2013, 02, 28),
                Amount = 20.00m
            };
            await httpClient.PostAsync<CorporateAction>($"https://integrationtest.com/api/stocks/{stockId}/corporateactions", corporateAction3, TestContext.Current.CancellationToken);

            await httpClient.DeleteAsync($"https://integrationtest.com/api/stocks/{stockId}/corporateactions/{corporateAction2.Id}", TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<List<CorporateAction>>($"https://integrationtest.com/api/stocks/{stockId}/corporateactions?startDate={Date.MinValue}&endDate={Date.MaxValue}", TestContext.Current.CancellationToken);
            response.Select(x => x.Id).Should().BeEquivalentTo(new[] { corporateAction1.Id, corporateAction3.Id }); 
        }
    }
}
