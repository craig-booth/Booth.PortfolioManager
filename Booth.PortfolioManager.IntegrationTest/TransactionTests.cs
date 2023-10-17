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
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);

            var portfolioId = Guid.NewGuid();
            var command = new CreatePortfolioCommand()
            {
                Id = portfolioId,
                Name = "Test Portfolio"
            };
            await client.Portfolio.CreatePortfolio(command);
            client.SetPortfolio(portfolioId);

            var transaction = new RestApi.Transactions.OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Stock = Integration.StockId,
                TransactionDate = new Date(2020, 01, 02),
                Units = 20,
                CostBase = 14.00m,
                AquisitionDate = new Date(2020, 01, 02)
            };
            await client.Transactions.Add(transaction);

            var response = await client.Portfolio.GetTransactions(new DateRange(Date.MinValue, Date.MaxValue));

            response.Transactions.Should().HaveCount(1);
        }

        [Fact]
        public async Task AddMultipleTransactions()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);

            var portfolioId = Guid.NewGuid();
            var command = new CreatePortfolioCommand()
            {
                Id = portfolioId,
                Name = "Test Portfolio"
            };
            await client.Portfolio.CreatePortfolio(command);
            client.SetPortfolio(portfolioId);

            var transaction1 = new RestApi.Transactions.OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Stock = Integration.StockId,
                TransactionDate = new Date(2020, 01, 02),
                Units = 20,
                CostBase = 14.00m,
                AquisitionDate = new Date(2020, 01, 02)
            };
            await client.Transactions.Add(transaction1);

            var transaction2 = new RestApi.Transactions.Disposal()
            {
                Id = Guid.NewGuid(),
                Stock = Integration.StockId,
                TransactionDate = new Date(2020, 01, 12),
                Units = 10,
                AveragePrice = 1.00m,
                TransactionCosts = 19.95m,
                CgtMethod = RestApi.Transactions.CgtCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = false
            };
            await client.Transactions.Add(transaction2);

            var response = await client.Portfolio.GetTransactions(new DateRange(Date.MinValue, Date.MaxValue));

            response.Transactions.Should().HaveCount(2);
        }

        [Fact]
        public async Task UpdateTransaction()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);

            var portfolioId = Guid.NewGuid();
            var command = new CreatePortfolioCommand()
            {
                Id = portfolioId,
                Name = "Test Portfolio"
            };
            await client.Portfolio.CreatePortfolio(command);
            client.SetPortfolio(portfolioId);

            var transaction1 = new RestApi.Transactions.OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Stock = Integration.StockId,
                TransactionDate = new Date(2020, 01, 02),
                Units = 20,
                CostBase = 14.00m,
                AquisitionDate = new Date(2020, 01, 02)
            };
            await client.Transactions.Add(transaction1);

            var transaction2 = new RestApi.Transactions.Disposal()
            {
                Id = Guid.NewGuid(),
                Stock = Integration.StockId,
                TransactionDate = new Date(2020, 01, 12),
                Units = 10,
                AveragePrice = 1.00m,
                TransactionCosts = 19.95m,
                CgtMethod = RestApi.Transactions.CgtCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = false
            };
            await client.Transactions.Add(transaction2);

            var transaction3 = new RestApi.Transactions.CashTransaction()
            {
                Id = Guid.NewGuid(),
                TransactionDate = new Date(2020, 01, 12),
                CashTransactionType = RestApi.Transactions.CashTransactionType.Deposit,
                Amount = 1200.00m
            };
            await client.Transactions.Add(transaction3);


            transaction2.AveragePrice = 1.20m;
            await client.Transactions.Update(transaction2);

            var response = await client.Portfolio.GetTransactions(new DateRange(Date.MinValue, Date.MaxValue));
            response.Transactions.Should().HaveCount(3);

            var transaction = await client.Transactions.Get(transaction2.Id);
            ((RestApi.Transactions.Disposal)transaction).AveragePrice.Should().Be(1.20m);

        }

        [Fact]
        public async Task DeleteTransaction()
        {
            var client = new RestClient(_Fixture.CreateClient(), "https://integrationtest.com/api/");
            await client.Authenticate(Integration.User, Integration.Password);

            var portfolioId = Guid.NewGuid();
            var command = new CreatePortfolioCommand()
            {
                Id = portfolioId,
                Name = "Test Portfolio"
            };
            await client.Portfolio.CreatePortfolio(command);
            client.SetPortfolio(portfolioId);

            var transaction1 = new RestApi.Transactions.OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Stock = Integration.StockId,
                TransactionDate = new Date(2020, 01, 02),
                Units = 20,
                CostBase = 14.00m,
                AquisitionDate = new Date(2020, 01, 02)
            };
            await client.Transactions.Add(transaction1);

            var transaction2 = new RestApi.Transactions.Disposal()
            {
                Id = Guid.NewGuid(),
                Stock = Integration.StockId,
                TransactionDate = new Date(2020, 01, 12),
                Units = 10,
                AveragePrice = 1.00m,
                TransactionCosts = 19.95m,
                CgtMethod = RestApi.Transactions.CgtCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = false
            };
            await client.Transactions.Add(transaction2);

            var transaction3 = new RestApi.Transactions.CashTransaction()
            {
                Id = Guid.NewGuid(),
                TransactionDate = new Date(2020, 01, 12),
                CashTransactionType = RestApi.Transactions.CashTransactionType.Deposit,
                Amount = 1200.00m
            };
            await client.Transactions.Add(transaction3);


            await client.Transactions.Delete(transaction2.Id);

            var response = await client.Portfolio.GetTransactions(new DateRange(Date.MinValue, Date.MaxValue));
            response.Transactions.Select(x => x.Id).Should().BeEquivalentTo(new[] { transaction1.Id, transaction3.Id });
        }

    }
}
