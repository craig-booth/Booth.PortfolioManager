using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Web.Models.Portfolio;
using Booth.PortfolioManager.Web.Models.Transaction;

namespace Booth.PortfolioManager.IntegrationTest
{
    [Collection(Integration.Collection)]
    public class TransactionTests
    {
   
        private IntegrationTestFixture _Fixture;
        public TransactionTests(IntegrationTestFixture fixture)
        {
            _Fixture = fixture;
        }

        [Fact]
        public async Task AddSingleTransaction()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var portfolioId = Guid.NewGuid();
            var command = new CreatePortfolioCommand()
            {
                Id = portfolioId,
                Name = "Test Portfolio"
            };
            await httpClient.PostAsync<CreatePortfolioCommand>("https://integrationtest.com/api/portfolio", command, TestContext.Current.CancellationToken);

            var transaction = new OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Stock = Integration.StockId,
                TransactionDate = new Date(2020, 01, 02),
                Units = 20,
                CostBase = 14.00m,
                AquisitionDate = new Date(2020, 01, 02)
            };
            await httpClient.PostAsync<Transaction>($"https://integrationtest.com/api/portfolio/{portfolioId}/transactions", transaction, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<TransactionsResponse>($"https://integrationtest.com/api/portfolio/{portfolioId}/transactions?fromdate={Date.MinValue.ToIsoDateString()}&todate={Date.MaxValue.ToIsoDateString()}", TestContext.Current.CancellationToken);

            response.Transactions.Should().HaveCount(1); 
        }

        [Fact]
        public async Task AddMultipleTransactions()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var portfolioId = Guid.NewGuid();
            var command = new CreatePortfolioCommand()
            {
                Id = portfolioId,
                Name = "Test Portfolio"
            };
            await httpClient.PostAsync<CreatePortfolioCommand>("https://integrationtest.com/api/portfolio", command, TestContext.Current.CancellationToken);

            var transaction1 = new OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Stock = Integration.StockId,
                TransactionDate = new Date(2020, 01, 02),
                Units = 20,
                CostBase = 14.00m,
                AquisitionDate = new Date(2020, 01, 02)
            };
            await httpClient.PostAsync<Transaction>($"https://integrationtest.com/api/portfolio/{portfolioId}/transactions", transaction1, TestContext.Current.CancellationToken);

            var transaction2 = new Disposal()
            {
                Id = Guid.NewGuid(),
                Stock = Integration.StockId,
                TransactionDate = new Date(2020, 01, 12),
                Units = 10,
                AveragePrice = 1.00m,
                TransactionCosts = 19.95m,
                CgtMethod = CgtCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = false
            };
            await httpClient.PostAsync<Transaction>($"https://integrationtest.com/api/portfolio/{portfolioId}/transactions", transaction2, TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<TransactionsResponse>($"https://integrationtest.com/api/portfolio/{portfolioId}/transactions?fromdate={Date.MinValue.ToIsoDateString()}&todate={Date.MaxValue.ToIsoDateString()}", TestContext.Current.CancellationToken);

            response.Transactions.Should().HaveCount(2); 
        }

        [Fact]
        public async Task UpdateTransaction()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var portfolioId = Guid.NewGuid();
            var command = new CreatePortfolioCommand()
            {
                Id = portfolioId,
                Name = "Test Portfolio"
            };
            await httpClient.PostAsync<CreatePortfolioCommand>("https://integrationtest.com/api/portfolio", command, TestContext.Current.CancellationToken);

            var transaction1 = new OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Stock = Integration.StockId,
                TransactionDate = new Date(2020, 01, 02),
                Units = 20,
                CostBase = 14.00m,
                AquisitionDate = new Date(2020, 01, 02)
            };
            await httpClient.PostAsync<Transaction>($"https://integrationtest.com/api/portfolio/{portfolioId}/transactions", transaction1, TestContext.Current.CancellationToken);

            var transaction2 = new Disposal()
            {
                Id = Guid.NewGuid(),
                Stock = Integration.StockId,
                TransactionDate = new Date(2020, 01, 12),
                Units = 10,
                AveragePrice = 1.00m,
                TransactionCosts = 19.95m,
                CgtMethod = CgtCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = false
            };
            await httpClient.PostAsync<Transaction>($"https://integrationtest.com/api/portfolio/{portfolioId}/transactions", transaction2, TestContext.Current.CancellationToken);

            var transaction3 = new CashTransaction()
            {
                Id = Guid.NewGuid(),
                TransactionDate = new Date(2020, 01, 12),
                CashTransactionType = CashTransactionType.Deposit,
                Amount = 1200.00m
            };
            await httpClient.PostAsync<Transaction>($"https://integrationtest.com/api/portfolio/{portfolioId}/transactions", transaction3, TestContext.Current.CancellationToken);


            transaction2.AveragePrice = 1.20m;
            await httpClient.PostAsync<Transaction>($"https://integrationtest.com/api/portfolio/{portfolioId}/transactions/{transaction2.Id}", transaction2, TestContext.Current.CancellationToken);


            var response = await httpClient.GetAsync<TransactionsResponse>($"https://integrationtest.com/api/portfolio/{portfolioId}/transactions?fromdate={Date.MinValue.ToIsoDateString()}&todate={Date.MaxValue.ToIsoDateString()}", TestContext.Current.CancellationToken);
            response.Transactions.Should().HaveCount(3);

            var transaction = await httpClient.GetAsync<Transaction>($"https://integrationtest.com/api/portfolio/{portfolioId}/transactions/{transaction2.Id}", TestContext.Current.CancellationToken);
            ((Disposal)transaction).AveragePrice.Should().Be(1.20m); 

        }

        [Fact]
        public async Task DeleteTransaction()
        {
            var httpClient = _Fixture.CreateClient();
            await httpClient.AuthenticateAsync(Integration.User, Integration.Password, TestContext.Current.CancellationToken);

            var portfolioId = Guid.NewGuid();
            var command = new CreatePortfolioCommand()
            {
                Id = portfolioId,
                Name = "Test Portfolio"
            };
            await httpClient.PostAsync<CreatePortfolioCommand>("https://integrationtest.com/api/portfolio", command, TestContext.Current.CancellationToken);

            var transaction1 = new OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Stock = Integration.StockId,
                TransactionDate = new Date(2020, 01, 02),
                Units = 20,
                CostBase = 14.00m,
                AquisitionDate = new Date(2020, 01, 02)
            };
            await httpClient.PostAsync<Transaction>($"https://integrationtest.com/api/portfolio/{portfolioId}/transactions", transaction1, TestContext.Current.CancellationToken);

            var transaction2 = new Disposal()
            {
                Id = Guid.NewGuid(),
                Stock = Integration.StockId,
                TransactionDate = new Date(2020, 01, 12),
                Units = 10,
                AveragePrice = 1.00m,
                TransactionCosts = 19.95m,
                CgtMethod = CgtCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = false
            };
            await httpClient.PostAsync<Transaction>($"https://integrationtest.com/api/portfolio/{portfolioId}/transactions", transaction2, TestContext.Current.CancellationToken);

            var transaction3 = new CashTransaction()
            {
                Id = Guid.NewGuid(),
                TransactionDate = new Date(2020, 01, 12),
                CashTransactionType = CashTransactionType.Deposit,
                Amount = 1200.00m
            };
            await httpClient.PostAsync<Transaction>($"https://integrationtest.com/api/portfolio/{portfolioId}/transactions", transaction3, TestContext.Current.CancellationToken);

            await httpClient.DeleteAsync($"https://integrationtest.com/api/portfolio/{portfolioId}/transactions/{transaction2.Id}", TestContext.Current.CancellationToken);

            var response = await httpClient.GetAsync<TransactionsResponse>($"https://integrationtest.com/api/portfolio/{portfolioId}/transactions?fromdate={Date.MinValue.ToIsoDateString()}&todate={Date.MaxValue.ToIsoDateString()}", TestContext.Current.CancellationToken);
            response.Transactions.Select(x => x.Id).Should().BeEquivalentTo(new[] { transaction1.Id, transaction3.Id });
        }

    }
}
