using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Utilities;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.RestApi.Portfolios;

namespace Booth.PortfolioManager.Web.Test.Services
{
    public class CashAccountServiceTests
    {
        [Fact]
        public void PortfolioNotFound()
        {
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));

            var service = new CashAccountService(null);

            var result = service.GetTransactions(dateRange);

            result.Should().HaveNotFoundStatus();
        }

        [Fact]
        public void NoTransactionsInRange()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));

            var stockResolver = mockRepository.Create<IStockResolver>();
            var factory = new PortfolioFactory(stockResolver.Object);
            var portfolio = factory.CreatePortfolio(id);

            var service = new CashAccountService(portfolio);

            var result = service.GetTransactions(dateRange);
            var response = result.Result;

            using (new AssertionScope())
            {
                response.Should().BeEquivalentTo(new
                {
                    OpeningBalance = 0.00m,
                    ClosingBalance = 0.00m,
                });
                response.Transactions.Should().BeEmpty();
            }

            mockRepository.VerifyAll();
        }

        [Fact]
        public void SingleTransactionInRange()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31));

            var stockResolver = mockRepository.Create<IStockResolver>();
            var factory = new PortfolioFactory(stockResolver.Object);
            var portfolio = factory.CreatePortfolio(id);
            portfolio.MakeCashTransaction(new Date(1999, 01, 01), BankAccountTransactionType.Deposit, 100.00m, "", Guid.NewGuid());
            portfolio.MakeCashTransaction(new Date(2000, 04, 01), BankAccountTransactionType.Withdrawl, 50.00m, "", Guid.NewGuid());
            portfolio.MakeCashTransaction(new Date(2001, 01, 01), BankAccountTransactionType.Deposit, 20.00m, "", Guid.NewGuid());

            var service = new CashAccountService(portfolio);

            var result = service.GetTransactions(dateRange);
            var response = result.Result;

            using (new AssertionScope())
            {
                response.Should().BeEquivalentTo(new
                {
                    OpeningBalance = 100.00m,
                    ClosingBalance = 50.00m,
                });
                response.Transactions.Should().BeEquivalentTo(new CashAccountTransactionsResponse.Transaction[]
                {
                    new CashAccountTransactionsResponse.Transaction()
                    {
                        Date = new Date(2000, 04, 01),
                        Type = RestApi.Transactions.CashTransactionType.Withdrawl,
                        Description = "Withdrawl",
                        Amount = -50.00m,
                        Balance = 50.00m
                    }
                });
            }

            mockRepository.VerifyAll();
        }

        [Fact]
        public void MultipleTransactionsInRange()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2002, 12, 31));

            var stockResolver = mockRepository.Create<IStockResolver>();
            var factory = new PortfolioFactory(stockResolver.Object);
            var portfolio = factory.CreatePortfolio(id);
            portfolio.MakeCashTransaction(new Date(1999, 01, 01), BankAccountTransactionType.Deposit, 100.00m, "", Guid.NewGuid());
            portfolio.MakeCashTransaction(new Date(2000, 04, 01), BankAccountTransactionType.Withdrawl, 50.00m, "", Guid.NewGuid());
            portfolio.MakeCashTransaction(new Date(2001, 01, 01), BankAccountTransactionType.Deposit, 20.00m, "", Guid.NewGuid());

            var service = new CashAccountService(portfolio);

            var result = service.GetTransactions(dateRange);
            var response = result.Result;

            using (new AssertionScope())
            {
                response.Should().BeEquivalentTo(new
                {
                    OpeningBalance = 100.00m,
                    ClosingBalance = 70.00m,
                });
                response.Transactions.Should().HaveCount(2);
            }

            mockRepository.VerifyAll();
        }

        
    }
}
