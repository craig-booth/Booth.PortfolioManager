using System;
using System.Linq;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain.Test.Transactions
{
    public class OpeningBalanceTests
    {
        [Fact]
        public void IncorrectTransactionType()
        {
            var transaction = new CashTransaction()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 01, 01),
                Comment = "Test Deposit",
                CashTransactionType = BankAccountTransactionType.Deposit,
                Amount = 100.00m
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var holding = mockRepository.Create<IHolding>();
            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new OpeningBalanceHandler();

            Action a = () => handler.Apply(transaction, holding.Object, cashAccount.Object);
a.Should().Throw<ArgumentException>();

            mockRepository.Verify();
        }

        [Fact]
        public void StockNotActive()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(2020, 01, 01), false, AssetCategory.AustralianStocks);

            var transaction = new OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2000, 01, 01),
                Stock = stock,
                Comment = "Test Opening Balance",
                Units = 100,
                AquisitionDate = new Date(2020, 02, 01),
                CostBase = 4500.00m,
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var holding = mockRepository.Create<IHolding>();
            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new OpeningBalanceHandler();
            Action a = () => handler.Apply(transaction, holding.Object, cashAccount.Object);

            a.Should().Throw<StockNotActiveException>();

            mockRepository.Verify();
        }

        [Fact]
        public void OpeningBalance()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var transaction = new OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 01, 01),
                Stock = stock,
                Comment = "Test Opening Balance",
                Units = 100,
                AquisitionDate = new Date(2020, 02, 01),
                CostBase = 4500.00m,
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.AddParcel(new Date(2020, 01, 01), new Date(2020, 02, 01), 100, 4500.00m, 4500.00m, transaction)).Returns(default(IParcel));

            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new OpeningBalanceHandler();
            handler.Apply(transaction, holding.Object, cashAccount.Object);

            mockRepository.Verify();
        }
    }
}
