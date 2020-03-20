using System;
using System.Linq;

using NUnit.Framework;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain.Test.Transactions
{
    class OpeningBalanceTests
    {
        [TestCase]
        public void NoExistingHoldings()
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

            var holdings = mockRepository.Create<IHoldingCollection>();
            holdings.Setup(x => x[stock.Id]).Returns(default(IHolding));
            holdings.Setup(x => x.Add(stock, new Date(2020, 01, 01))).Returns(holding.Object);

            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new OpeningBalanceHandler(holdings.Object, cashAccount.Object);
            handler.ApplyTransaction(transaction);

            mockRepository.Verify();
        }

        [TestCase]
        public void ExistingHoldings()
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

            var holdings = mockRepository.Create<IHoldingCollection>();
            holdings.Setup(x => x[stock.Id]).Returns(holding.Object);

            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new OpeningBalanceHandler(holdings.Object, cashAccount.Object);
            handler.ApplyTransaction(transaction);

            mockRepository.Verify();
        }

        [TestCase]
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

            var holdings = mockRepository.Create<IHoldingCollection>();
            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new OpeningBalanceHandler(holdings.Object, cashAccount.Object);

            Assert.That(() => handler.ApplyTransaction(transaction), Throws.ArgumentException);

            mockRepository.Verify();
        }
    }
}
