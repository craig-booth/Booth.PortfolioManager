using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.RestApi.Transactions;

namespace Booth.PortfolioManager.Web.Test.Services
{
    public class PortfolioTransactionServiceTests
    {
        [Fact]
        public void PortfolioNotFound()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));

            var service = new PortfolioTransactionService(null);

            var result = service.GetTransactions(dateRange);

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetTransactionNotFound()
        {
            var portfolio = PortfolioTestCreator.CreatePortfolio();

            var service = new PortfolioTransactionService(portfolio);

            var result = service.GetTransaction(Guid.NewGuid());

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void GetTransaction()
        {
            var portfolio = PortfolioTestCreator.CreatePortfolio();

            var service = new PortfolioTransactionService(portfolio);

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
            var portfolio = PortfolioTestCreator.CreatePortfolio();

            var service = new PortfolioTransactionService(portfolio);

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
            var portfolio = PortfolioTestCreator.CreatePortfolio();

            var service = new PortfolioTransactionService(portfolio);

            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));
            var result = service.GetTransactions(Guid.NewGuid(), dateRange);

            result.Result.Transactions.Should().BeEmpty();
        }

        [Fact]
        public void GetTransactionsForStock()
        {
            var portfolio = PortfolioTestCreator.CreatePortfolio();

            var service = new PortfolioTransactionService(portfolio);

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

     /*   [Fact]
        public void ApplyAquisitionValidationError()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void ApplyAquisition()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void ApplyCashTransactionValidationError()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void ApplyCashTransaction()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void ApplyCostBaseAdjustmentValidationError()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void ApplyCostBaseAdjustment()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void ApplyDisposalValidationError()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void ApplyDisposal()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void ApplyIncomeReceivedValidationError()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void ApplyIncomeReceived()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void ApplyOpeningBalanceValidationError()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void ApplyOpeningBalance()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void ApplyReturnOfCapitalValidationError()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void ApplyReturnOfCapital()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void ApplyUnitCountAdjustmentValidationError()
        {
            false.Should().BeTrue();
        }

        [Fact]
        public void ApplyUnitCountAdjustment()
        {
            false.Should().BeTrue();
        } */
    } 
}
