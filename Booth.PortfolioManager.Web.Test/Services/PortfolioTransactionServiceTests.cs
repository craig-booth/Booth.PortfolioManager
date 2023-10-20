using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.RestApi.Transactions;
using Booth.PortfolioManager.Web.Mappers;

namespace Booth.PortfolioManager.Web.Test.Services
{
    [Collection(Services.Collection)]
    public class PortfolioTransactionServiceTests
    {
        private readonly ServicesTestFixture _Fixture;

        public PortfolioTransactionServiceTests(ServicesTestFixture fixture)
        {
            _Fixture = fixture;
        }

        [Fact]
        public void PortfolioNotFound()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));

            var service = new PortfolioTransactionService(null ,null, new TransactionMapper(_Fixture.StockResolver));

            var result = service.GetTransactions(dateRange);

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetTransactionNotFound()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioTransactionService(portfolio, null, new TransactionMapper(_Fixture.StockResolver));

            var result = service.GetTransaction(Guid.NewGuid());

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetTransaction()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioTransactionService(portfolio, null, new TransactionMapper(_Fixture.StockResolver));

            var id = portfolio.Transactions[1].Id;
            var result = service.GetTransaction(id);

            result.Result.Should().BeEquivalentTo(new
            {
                Stock = _Fixture.Stock_ARG.Id,
                Id = id,
                Type = TransactionType.Aquisition,
                TransactionDate = new Date(2000, 01, 01),
                Description = "Aquired 100 shares @ $1.00"
            });
        }

        [Fact]
        public void GetTransactions()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioTransactionService(portfolio, null, new TransactionMapper(_Fixture.StockResolver));

            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));
            var result = service.GetTransactions(dateRange);

            result.Result.Transactions.Should().BeEquivalentTo(new []
            {
                new RestApi.Portfolios.TransactionsResponse.TransactionItem()
                {
                    Id = portfolio.Transactions[0].Id,
                    Stock = null,
                    TransactionDate = new Date(2000, 01, 01),
                    Description = "Deposit $10,000.00",
                    Comment= ""
                },
                new RestApi.Portfolios.TransactionsResponse.TransactionItem()
                {
                    Id = portfolio.Transactions[1].Id,
                    Stock = _Fixture.Stock_ARG,
                    TransactionDate = new Date(2000, 01, 01),
                    Description = "Aquired 100 shares @ $1.00",
                    Comment= ""
                },
                new RestApi.Portfolios.TransactionsResponse.TransactionItem()
                {
                    Id = portfolio.Transactions[2].Id,
                    Stock = _Fixture.Stock_WAM,
                    TransactionDate = new Date(2000, 01, 01),
                    Description = "Aquired 200 shares @ $1.20",
                    Comment= ""
                }
            });
        }

        [Fact]
        public void GetTransactionsForStockNotOwned()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioTransactionService(portfolio, null, new TransactionMapper(_Fixture.StockResolver));

            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));
            var result = service.GetTransactions(Guid.NewGuid(), dateRange);

            result.Result.Transactions.Should().BeEmpty();
        }

        [Fact]
        public void GetTransactionsForStock()
        {
            var portfolio = _Fixture.CreateDefaultPortfolio();

            var service = new PortfolioTransactionService(portfolio, null, new TransactionMapper(_Fixture.StockResolver));

            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));
            var result = service.GetTransactions(_Fixture.Stock_WAM.Id, dateRange);

            result.Result.Transactions.Should().BeEquivalentTo(new[]
            {
                new RestApi.Portfolios.TransactionsResponse.TransactionItem()
                {
                    Id = portfolio.Transactions[2].Id,
                    Stock = _Fixture.Stock_WAM,
                    TransactionDate = new Date(2000, 01, 01),
                    Description = "Aquired 200 shares @ $1.20",
                    Comment= ""
                }
            });
        }


        [Fact]
        public void AddAquisition()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var transaction = new Aquisition()
            {
                Id = Guid.NewGuid(),
                Stock = _Fixture.Stock_ARG.Id,
                TransactionDate = new Date(2007, 01, 01),
                Units = 50,
                AveragePrice = 0.98m,
                TransactionCosts = 1.00m,
                Comment = "",
                CreateCashTransaction = true
            };

            var repository = mockRepository.Create<IPortfolioRepository> ();
            repository.Setup(x => x.AddTransaction(portfolio, transaction.Id));

            var service = new PortfolioTransactionService(portfolio, repository.Object, new TransactionMapper(_Fixture.StockResolver));

            var priorUnits = portfolio.Holdings[_Fixture.Stock_ARG.Id].Properties[transaction.TransactionDate].Units;

            var result = service.AddTransaction(transaction);

            result.Should().HaveOkStatus();

            portfolio.Holdings[_Fixture.Stock_ARG.Id].Properties[transaction.TransactionDate].Units.Should().Be(priorUnits + 50);

            mockRepository.Verify(); 
        }

        [Fact]
        public void AddCashTransaction()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var transaction = new CashTransaction()
            {
                Id = Guid.NewGuid(),
                Stock = Guid.Empty,
                TransactionDate = new Date(2007, 01, 01),
                Comment = "",
                CashTransactionType = CashTransactionType.Interest,
                Amount = 1.98m
            };

            var repository = mockRepository.Create<IPortfolioRepository>();
            repository.Setup(x => x.AddTransaction(portfolio, transaction.Id));

            var service = new PortfolioTransactionService(portfolio, repository.Object, new TransactionMapper(_Fixture.StockResolver));

            var priorBalance = portfolio.CashAccount.Balance(transaction.TransactionDate);

            var result = service.AddTransaction(transaction);

            result.Should().HaveOkStatus();

            portfolio.CashAccount.Balance(transaction.TransactionDate).Should().Be(priorBalance + 1.98m);

            mockRepository.Verify(); 
        }

        [Fact]
        public void AddCostBaseAdjustment()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var transaction = new CostBaseAdjustment()
            {
                Id = Guid.NewGuid(),
                Stock = _Fixture.Stock_ARG.Id,
                TransactionDate = new Date(2007, 01, 01),
                Comment = "",
                Percentage = 0.50m
            };

            var repository = mockRepository.Create<IPortfolioRepository>();
            repository.Setup(x => x.AddTransaction(portfolio, transaction.Id));

            var service = new PortfolioTransactionService(portfolio, repository.Object, new TransactionMapper(_Fixture.StockResolver));

            var priorCostBase = portfolio.Holdings[_Fixture.Stock_ARG.Id].Properties[transaction.TransactionDate].CostBase;

            var result = service.AddTransaction(transaction);

            result.Should().HaveOkStatus();

            portfolio.Holdings[_Fixture.Stock_ARG.Id].Properties[transaction.TransactionDate].CostBase.Should().BeApproximately(priorCostBase * 0.50m, 2);

            mockRepository.Verify(); 
        }

        [Fact]
        public void AddDisposal()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var transaction = new Disposal()
            {
                Id = Guid.NewGuid(),
                Stock = _Fixture.Stock_ARG.Id,
                TransactionDate = new Date(2007, 01, 01),
                Comment = "",
                Units = 50,
                AveragePrice = 0.98m,
                TransactionCosts = 1.00m,
                CgtMethod = CgtCalculationMethod.LastInFirstOut,
                CreateCashTransaction = true
            };

            var repository = mockRepository.Create<IPortfolioRepository>();
            repository.Setup(x => x.AddTransaction(portfolio, transaction.Id));

            var service = new PortfolioTransactionService(portfolio, repository.Object, new TransactionMapper(_Fixture.StockResolver));

            var priorUnits = portfolio.Holdings[_Fixture.Stock_ARG.Id].Properties[transaction.TransactionDate].Units;

            var result = service.AddTransaction(transaction);

            result.Should().HaveOkStatus();          

            portfolio.Holdings[_Fixture.Stock_ARG.Id].Properties[transaction.TransactionDate].Units.Should().Be(priorUnits - 50);

            mockRepository.Verify(); 
        }


        [Fact]
        public void AddIncomeReceived()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var transaction = new IncomeReceived()
            {
                Id = Guid.NewGuid(),
                Stock = _Fixture.Stock_ARG.Id,
                TransactionDate = new Date(2007, 01, 01),
                Comment = "",
                RecordDate = new Date(2006, 12, 27),
                FrankedAmount = 1.00m,
                UnfrankedAmount = 2.00m,
                FrankingCredits = 3.00m,
                Interest = 4.00m,
                TaxDeferred = 5.00m,
                CreateCashTransaction = true,
                DrpCashBalance = 2.50m
            };

            var repository = mockRepository.Create<IPortfolioRepository>();
            repository.Setup(x => x.AddTransaction(portfolio, transaction.Id));

            var service = new PortfolioTransactionService(portfolio, repository.Object, new TransactionMapper(_Fixture.StockResolver));

            var priorBalance = portfolio.CashAccount.Balance(transaction.TransactionDate);

            var result = service.AddTransaction(transaction);

            result.Should().HaveOkStatus();

            portfolio.CashAccount.Balance(transaction.TransactionDate).Should().Be(priorBalance + 1.00m + 2.00m + 4.00m + 5.00m);

            mockRepository.Verify(); 
        }

        [Fact]
        public void AddOpeningBalance()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var transaction = new OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Stock = _Fixture.Stock_ARG.Id,
                TransactionDate = new Date(2007, 01, 01),
                Comment = "",
                Units = 50,
                CostBase = 0.98m,
                AquisitionDate = new Date(2005, 01, 01)
            };

            var repository = mockRepository.Create<IPortfolioRepository>();
            repository.Setup(x => x.AddTransaction(portfolio, transaction.Id));

            var service = new PortfolioTransactionService(portfolio, repository.Object, new TransactionMapper(_Fixture.StockResolver));

            var priorUnits = portfolio.Holdings[_Fixture.Stock_ARG.Id].Properties[transaction.TransactionDate].Units;

            var result = service.AddTransaction(transaction);

            result.Should().HaveOkStatus();

            portfolio.Holdings[_Fixture.Stock_ARG.Id].Properties[transaction.TransactionDate].Units.Should().Be(priorUnits + 50);

            mockRepository.Verify(); 
        }

        [Fact]
        public void AddReturnOfCapital()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var transaction = new ReturnOfCapital()
            {
                Id = Guid.NewGuid(),
                Stock = _Fixture.Stock_ARG.Id,
                TransactionDate = new Date(2007, 01, 01),
                Comment = "",
                Amount = 0.10m,
                RecordDate = new Date(2006, 12, 01),
                CreateCashTransaction = true
            };

            var repository = mockRepository.Create<IPortfolioRepository>();
            repository.Setup(x => x.AddTransaction(portfolio, transaction.Id));

            var service = new PortfolioTransactionService(portfolio, repository.Object, new TransactionMapper(_Fixture.StockResolver));

            var priorCostBase = portfolio.Holdings[_Fixture.Stock_ARG.Id].Properties[transaction.TransactionDate].CostBase;

            var result = service.AddTransaction(transaction);

            result.Should().HaveOkStatus();

            portfolio.Holdings[_Fixture.Stock_ARG.Id].Properties[transaction.TransactionDate].CostBase.Should().Be(priorCostBase - 25.00m);

            mockRepository.Verify(); 
        }

        [Fact]
        public void AddUnitCountAdjustment()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var transaction = new UnitCountAdjustment()
            {
                Id = Guid.NewGuid(),
                Stock = _Fixture.Stock_ARG.Id,
                TransactionDate = new Date(2007, 01, 01),
                Comment = "",
                OriginalUnits = 1,
                NewUnits = 2
            };

            var repository = mockRepository.Create<IPortfolioRepository>();
            repository.Setup(x => x.AddTransaction(portfolio, transaction.Id));

            var service = new PortfolioTransactionService(portfolio, repository.Object, new TransactionMapper(_Fixture.StockResolver));

            var priorUnits = portfolio.Holdings[_Fixture.Stock_ARG.Id].Properties[transaction.TransactionDate].Units;

            var result = service.AddTransaction(transaction);

            result.Should().HaveOkStatus();
            
            portfolio.Holdings[_Fixture.Stock_ARG.Id].Properties[transaction.TransactionDate].Units.Should().Be(priorUnits * 2);

            mockRepository.Verify(); 
        }

        [Fact]
        public void UpdateTransactionNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var updatedTransaction = new OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Stock = _Fixture.Stock_WAM.Id,
                TransactionDate = new Date(2005, 01, 03),
                Comment = "",
                Description = "",
                Units = 7,
                CostBase = 32.50m,
                AquisitionDate = new Date(2005, 01, 03)
            };

            var repository = mockRepository.Create<IPortfolioRepository>();        

            var service = new PortfolioTransactionService(portfolio, repository.Object, new TransactionMapper(_Fixture.StockResolver));

            var result = service.UpdateTransaction(updatedTransaction.Id, updatedTransaction);

            result.Should().HaveNotFoundStatus();
            portfolio.Holdings[_Fixture.Stock_WAM.Id].Properties[new Date(2005, 01, 03)].Units.Should().Be(205);

            mockRepository.Verify();
        }

        [Fact]
        public void UpdateTransactionIdChanged()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var updatedTransaction = new OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Stock = _Fixture.Stock_WAM.Id,
                TransactionDate = new Date(2005, 01, 03),
                Comment = "",
                Description = "",
                Units = 7,
                CostBase = 32.50m,
                AquisitionDate = new Date(2005, 01, 03)
            };

            var repository = mockRepository.Create<IPortfolioRepository>();

            var service = new PortfolioTransactionService(portfolio, repository.Object, new TransactionMapper(_Fixture.StockResolver));

            var result = service.UpdateTransaction(updatedTransaction.Id, updatedTransaction);

            result.Should().HaveNotFoundStatus();
            portfolio.Holdings[_Fixture.Stock_WAM.Id].Properties[new Date(2005, 01, 03)].Units.Should().Be(205);

            mockRepository.Verify();
        }


        [Fact]
        public void UpdateTransaction()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var updatedTransaction = new OpeningBalance()
            {
                Id = _Fixture.WAM_OpeningBalance,
                Stock = _Fixture.Stock_WAM.Id,
                TransactionDate = new Date(2005, 01, 03),
                Comment = "",
                Description = "",
                Units = 7,
                CostBase = 32.50m,
                AquisitionDate = new Date(2005, 01, 03)
            };

            var repository = mockRepository.Create<IPortfolioRepository>();
            repository.Setup(x => x.UpdateTransaction(portfolio, _Fixture.WAM_OpeningBalance));

            var service = new PortfolioTransactionService(portfolio, repository.Object, new TransactionMapper(_Fixture.StockResolver));

            var result = service.UpdateTransaction(updatedTransaction.Id, updatedTransaction);

            result.Should().HaveOkStatus();
            portfolio.Holdings[_Fixture.Stock_WAM.Id].Properties[new Date(2005, 01, 03)].Units.Should().Be(207);

            mockRepository.Verify();
        }

        [Fact]
        public void DeleteTransactionNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var repository = mockRepository.Create<IPortfolioRepository>();
            repository.Setup(x => x.DeleteTransaction(portfolio, _Fixture.WAM_OpeningBalance));

            var service = new PortfolioTransactionService(portfolio, repository.Object, new TransactionMapper(_Fixture.StockResolver));

            var result = service.DeleteTransaction(Guid.NewGuid());

            result.Should().HaveNotFoundStatus();

            portfolio.Holdings[_Fixture.Stock_WAM.Id].Properties[new Date(2005, 01, 03)].Units.Should().Be(205);

            mockRepository.Verify();
        }

        [Fact]
        public void DeleteTransaction()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = _Fixture.CreateDefaultPortfolio();

            var repository = mockRepository.Create<IPortfolioRepository>();
            repository.Setup(x => x.DeleteTransaction(portfolio, _Fixture.WAM_OpeningBalance));

            var service = new PortfolioTransactionService(portfolio, repository.Object, new TransactionMapper(_Fixture.StockResolver));

            var result = service.DeleteTransaction(_Fixture.WAM_OpeningBalance);

            result.Should().HaveOkStatus();

            portfolio.Holdings[_Fixture.Stock_WAM.Id].Properties[new Date(2005, 01, 03)].Units.Should().Be(200);

            mockRepository.Verify();
        }
    } 
}
