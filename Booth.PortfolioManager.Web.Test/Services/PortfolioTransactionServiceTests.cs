using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.RestApi.Transactions;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions.Events;

namespace Booth.PortfolioManager.Web.Test.Services
{
    public class PortfolioTransactionServiceTests
    {
        [Fact]
        public void PortfolioNotFound()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));

            var service = new PortfolioTransactionService(null ,null);

            var result = service.GetTransactions(dateRange);

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetTransactionNotFound()
        {
            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var service = new PortfolioTransactionService(portfolio, null);

            var result = service.GetTransaction(Guid.NewGuid());

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetTransaction()
        {
            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var service = new PortfolioTransactionService(portfolio, null);

            var id = portfolio.Transactions[1].Id;
            var result = service.GetTransaction(id);

            result.Result.Should().BeEquivalentTo(new
            {
                Stock = PortfolioTestCreator.Stock_ARG.Id,
                Id = id,
                Type = TransactionType.Aquisition,
                TransactionDate = new Date(2000, 01, 01),
                Description = "Aquired 100 shares @ $1.00"
            });
        }

        [Fact]
        public void GetTransactions()
        {
            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var service = new PortfolioTransactionService(portfolio, null);

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
                    Stock = PortfolioTestCreator.Stock_ARG,
                    TransactionDate = new Date(2000, 01, 01),
                    Description = "Aquired 100 shares @ $1.00",
                    Comment= ""
                },
                new RestApi.Portfolios.TransactionsResponse.TransactionItem()
                {
                    Id = portfolio.Transactions[2].Id,
                    Stock = PortfolioTestCreator.Stock_WAM,
                    TransactionDate = new Date(2000, 01, 01),
                    Description = "Aquired 200 shares @ $1.20",
                    Comment= ""
                }
            });
        }

        [Fact]
        public void GetTransactionsForStockNotOwned()
        {
            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var service = new PortfolioTransactionService(portfolio, null);

            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));
            var result = service.GetTransactions(Guid.NewGuid(), dateRange);

            result.Result.Transactions.Should().BeEmpty();
        }

        [Fact]
        public void GetTransactionsForStock()
        {
            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var service = new PortfolioTransactionService(portfolio, null);

            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));
            var result = service.GetTransactions(PortfolioTestCreator.Stock_WAM.Id, dateRange);

            result.Result.Transactions.Should().BeEquivalentTo(new[]
            {
                new RestApi.Portfolios.TransactionsResponse.TransactionItem()
                {
                    Id = portfolio.Transactions[2].Id,
                    Stock = PortfolioTestCreator.Stock_WAM,
                    TransactionDate = new Date(2000, 01, 01),
                    Description = "Aquired 200 shares @ $1.20",
                    Comment= ""
                }
            });
        }


        [Fact]
        public void ApplyAquisition()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var transaction = new Aquisition()
            {
                Id = Guid.NewGuid(),
                Stock = PortfolioTestCreator.Stock_ARG.Id,
                TransactionDate = new Date(2007, 01, 01),
                Units = 50,
                AveragePrice = 0.98m,
                TransactionCosts = 1.00m,
                Comment = "",
                CreateCashTransaction = true
            };

            var repository = mockRepository.Create<IPortfolioRepository> ();
            repository.Setup(x => x.AddTransaction(portfolio, transaction.Id));

            var service = new PortfolioTransactionService(portfolio, repository.Object);

            var priorUnits = portfolio.Holdings[PortfolioTestCreator.Stock_ARG.Id].Properties[transaction.TransactionDate].Units;

            var result = service.ApplyTransaction(transaction);

            result.Should().HaveOkStatus();

            portfolio.Holdings[PortfolioTestCreator.Stock_ARG.Id].Properties[transaction.TransactionDate].Units.Should().Be(priorUnits + 50);

            mockRepository.Verify(); 
        }

        [Fact]
        public void ApplyCashTransaction()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

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

            var service = new PortfolioTransactionService(portfolio, repository.Object);

            var priorBalance = portfolio.CashAccount.Balance(transaction.TransactionDate);

            var result = service.ApplyTransaction(transaction);

            result.Should().HaveOkStatus();

            portfolio.CashAccount.Balance(transaction.TransactionDate).Should().Be(priorBalance + 1.98m);

            mockRepository.Verify(); 
        }

        [Fact]
        public void ApplyCostBaseAdjustment()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var transaction = new CostBaseAdjustment()
            {
                Id = Guid.NewGuid(),
                Stock = PortfolioTestCreator.Stock_ARG.Id,
                TransactionDate = new Date(2007, 01, 01),
                Comment = "",
                Percentage = 0.50m
            };

            var repository = mockRepository.Create<IPortfolioRepository>();
            repository.Setup(x => x.AddTransaction(portfolio, transaction.Id));

            var service = new PortfolioTransactionService(portfolio, repository.Object);

            var priorCostBase = portfolio.Holdings[PortfolioTestCreator.Stock_ARG.Id].Properties[transaction.TransactionDate].CostBase;

            var result = service.ApplyTransaction(transaction);

            result.Should().HaveOkStatus();

            portfolio.Holdings[PortfolioTestCreator.Stock_ARG.Id].Properties[transaction.TransactionDate].CostBase.Should().Be(priorCostBase * 0.50m);

            mockRepository.Verify(); 
        }

        [Fact]
        public void ApplyDisposal()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var transaction = new Disposal()
            {
                Id = Guid.NewGuid(),
                Stock = PortfolioTestCreator.Stock_ARG.Id,
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

            var service = new PortfolioTransactionService(portfolio, repository.Object);

            var priorUnits = portfolio.Holdings[PortfolioTestCreator.Stock_ARG.Id].Properties[transaction.TransactionDate].Units;

            var result = service.ApplyTransaction(transaction);

            result.Should().HaveOkStatus();          

            portfolio.Holdings[PortfolioTestCreator.Stock_ARG.Id].Properties[transaction.TransactionDate].Units.Should().Be(priorUnits - 50);

            mockRepository.Verify(); 
        }


        [Fact]
        public void ApplyIncomeReceived()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var transaction = new IncomeReceived()
            {
                Id = Guid.NewGuid(),
                Stock = PortfolioTestCreator.Stock_ARG.Id,
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

            var service = new PortfolioTransactionService(portfolio, repository.Object);

            var priorBalance = portfolio.CashAccount.Balance(transaction.TransactionDate);

            var result = service.ApplyTransaction(transaction);

            result.Should().HaveOkStatus();

            portfolio.CashAccount.Balance(transaction.TransactionDate).Should().Be(priorBalance + 1.00m + 2.00m + 4.00m + 5.00m);

            mockRepository.Verify(); 
        }

        [Fact]
        public void ApplyOpeningBalance()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var transaction = new OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Stock = PortfolioTestCreator.Stock_ARG.Id,
                TransactionDate = new Date(2007, 01, 01),
                Comment = "",
                Units = 50,
                CostBase = 0.98m,
                AquisitionDate = new Date(2005, 01, 01)
            };

            var repository = mockRepository.Create<IPortfolioRepository>();
            repository.Setup(x => x.AddTransaction(portfolio, transaction.Id));

            var service = new PortfolioTransactionService(portfolio, repository.Object);

            var priorUnits = portfolio.Holdings[PortfolioTestCreator.Stock_ARG.Id].Properties[transaction.TransactionDate].Units;

            var result = service.ApplyTransaction(transaction);

            result.Should().HaveOkStatus();

            portfolio.Holdings[PortfolioTestCreator.Stock_ARG.Id].Properties[transaction.TransactionDate].Units.Should().Be(priorUnits + 50);

            mockRepository.Verify(); 
        }

        [Fact]
        public void ApplyReturnOfCapital()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var transaction = new ReturnOfCapital()
            {
                Id = Guid.NewGuid(),
                Stock = PortfolioTestCreator.Stock_ARG.Id,
                TransactionDate = new Date(2007, 01, 01),
                Comment = "",
                Amount = 5.00m,
                RecordDate = new Date(2006, 12, 01),
                CreateCashTransaction = true
            };

            var repository = mockRepository.Create<IPortfolioRepository>();
            repository.Setup(x => x.AddTransaction(portfolio, transaction.Id));

            var service = new PortfolioTransactionService(portfolio, repository.Object);

            var priorCostBase = portfolio.Holdings[PortfolioTestCreator.Stock_ARG.Id].Properties[transaction.TransactionDate].CostBase;

            var result = service.ApplyTransaction(transaction);

            result.Should().HaveOkStatus();

            portfolio.Holdings[PortfolioTestCreator.Stock_ARG.Id].Properties[transaction.TransactionDate].CostBase.Should().Be(priorCostBase - 5.00m);

            mockRepository.Verify(); 
        }

        [Fact]
        public void ApplyUnitCountAdjustment()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolio = PortfolioTestCreator.CreateDefaultPortfolio();

            var transaction = new UnitCountAdjustment()
            {
                Id = Guid.NewGuid(),
                Stock = PortfolioTestCreator.Stock_ARG.Id,
                TransactionDate = new Date(2007, 01, 01),
                Comment = "",
                OriginalUnits = 1,
                NewUnits = 2
            };

            var repository = mockRepository.Create<IPortfolioRepository>();
            repository.Setup(x => x.AddTransaction(portfolio, transaction.Id));

            var service = new PortfolioTransactionService(portfolio, repository.Object);

            var priorUnits = portfolio.Holdings[PortfolioTestCreator.Stock_ARG.Id].Properties[transaction.TransactionDate].Units;

            var result = service.ApplyTransaction(transaction);

            result.Should().HaveOkStatus();
            
            portfolio.Holdings[PortfolioTestCreator.Stock_ARG.Id].Properties[transaction.TransactionDate].Units.Should().Be(priorUnits * 2);

            mockRepository.Verify(); 
        } 
    } 
}
