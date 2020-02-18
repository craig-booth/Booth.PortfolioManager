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
            Assert.That(false);
            /*
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

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

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.AddParcel(new Date(2020, 01, 01), new Date(2020, 02, 01), 100, 4500.00m, 4500.00m, transaction));

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x.Get(stock.Id)).Returns(default(IHolding));
            Mock.Get(holdings).Setup(x => x.Add(stock, new Date(2020, 01, 01))).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);

            var handler = new OpeningBalanceHandler(holdings, cashAccount);

            handler.ApplyTransaction(transaction);*/
        }

        [TestCase]
        public void ExistingHoldings()
        {Assert.That(false);
            /*
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

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

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.AddParcel(new Date(2020, 01, 01), new Date(2020, 02, 01), 100, 4500.00m, 4500.00m, transaction));

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x.Get(stock.Id)).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);

            var handler = new OpeningBalanceHandler(holdings, cashAccount);

            handler.ApplyTransaction(transaction);*/
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

            var holdings = new HoldingCollection();
            var cashAccount = new CashAccount();

            var handler = new OpeningBalanceHandler(holdings, cashAccount);

            Assert.That(() => handler.ApplyTransaction(transaction), Throws.ArgumentException);
        }
    }
}
