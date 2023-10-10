using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using FluentAssertions;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Web.Utilities;
using MongoDB.Driver;

namespace Booth.PortfolioManager.IntegrationTest
{
    
    public class TransactionTests
    {
        private IPortfolioManagerDatabase _Database;
        private IPortfolioFactory _PortfolioFactory;

        private Stock _Stock1;

        public TransactionTests() 
        {
            var stockCache = new EntityCache<Stock>();
            var stockResolver = new StockResolver(stockCache);
            _PortfolioFactory = new PortfolioFactory(stockResolver);

            _Stock1 = new Stock(Guid.NewGuid());
            _Stock1.List("ABC", "ABCPty Ltd", new Date(2000, 01, 01), false, AssetCategory.AustralianStocks);
            stockCache.Add(_Stock1);

            var mongoClient = new MongoClient("mongodb://192.168.1.93:27017");
            _Database = new PortfolioManagerDatabase(mongoClient, "TransactionTest", _PortfolioFactory, stockResolver);
        }

        [Fact]
        public void AddSingleTransaction()
        {
            var repository = new PortfolioRepository(_Database);

            var portfolio = _PortfolioFactory.CreatePortfolio(Guid.NewGuid());
            portfolio.Create("Test", Guid.NewGuid());
            repository.Add(portfolio);

            var transaction = new OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Stock = _Stock1,
                Date = new Date(2020, 01, 02),
                Units = 20,
                CostBase = 14.00m,
                AquisitionDate = new Date(2020, 01, 02)
            };
            portfolio.AddTransaction(transaction);

            repository.AddTransaction(portfolio, transaction.Id);



            var resultPortfolio = repository.Get(portfolio.Id);

            resultPortfolio.Transactions.Should().BeEquivalentTo(portfolio.Transactions);
        }

        [Fact]
        public void AddMultipleTransactions()
        {
            var repository = new PortfolioRepository(_Database);

            var portfolio = _PortfolioFactory.CreatePortfolio(Guid.NewGuid());
            repository.Add(portfolio);

            var transaction1 = new OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Stock = _Stock1,
                Date = new Date(2020, 01, 02),
                Units = 20,
                CostBase = 140.00m,
                AquisitionDate = new Date(2020, 01, 02)
            };
            portfolio.AddTransaction(transaction1);

            repository.AddTransaction(portfolio, transaction1.Id);

            var transaction2 = new Disposal()
            {
                Id = Guid.NewGuid(),
                Stock = _Stock1,
                Date = new Date(2020, 01, 12),
                Units = 10,
                AveragePrice = 1.00m,
                TransactionCosts = 19.95m,
                CgtMethod = Domain.Utils.CgtCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = false
            };
            portfolio.AddTransaction(transaction2);
            repository.AddTransaction(portfolio, transaction2.Id);

            var transaction3 = new Disposal()
            {
                Id = Guid.NewGuid(),
                Stock = _Stock1,
                Date = new Date(2020, 01, 14),
                Units = 10,
                AveragePrice = 2.00m,
                TransactionCosts = 19.95m,
                CgtMethod = Domain.Utils.CgtCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = false
            };
            portfolio.AddTransaction(transaction3);
            repository.AddTransaction(portfolio, transaction3.Id);

            var resultPortfolio = repository.Get(portfolio.Id);

            resultPortfolio.Transactions.Should().BeEquivalentTo(portfolio.Transactions);
        }

        [Fact]
        public void UpdateTransaction()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void DeleteTransaction()
        {
            throw new NotImplementedException();
        }

    }
}
