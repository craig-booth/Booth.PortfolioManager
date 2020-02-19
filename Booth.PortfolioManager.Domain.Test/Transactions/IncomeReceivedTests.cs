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
    class IncomeReceivedTests
    {
        [TestCase]
        public void NoSharesOwned()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var transaction = new IncomeReceived()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Dividend",
                RecordDate = new Date(2020, 01, 01),
                FrankedAmount = 10.00m,
                UnfrankedAmount = 20.00m,
                FrankingCredits = 30.00m,
                Interest = 40.00m,
                TaxDeferred = 0.00m,
                CreateCashTransaction = false,
                DRPCashBalance = 0.00m
            };

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x[stock.Id]).Returns(default(IHolding));

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);

            var handler = new IncomeReceivedHandler(holdings, cashAccount);

            Assert.That(() => handler.ApplyTransaction(transaction), Throws.Exception.InstanceOf(typeof(NoParcelsForTransaction)));
        }

        [TestCase]
        public void NoSharesAtRecordDated()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var transaction = new IncomeReceived()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Dividend",
                RecordDate = new Date(2020, 01, 01),
                FrankedAmount = 10.00m,
                UnfrankedAmount = 20.00m,
                FrankingCredits = 30.00m,
                Interest = 40.00m,
                TaxDeferred = 0.00m,
                CreateCashTransaction = false,
                DRPCashBalance = 0.00m
            };

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.IsEffectiveAt(new Date(2020, 01, 01))).Returns(false);

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x[stock.Id]).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);

            var handler = new IncomeReceivedHandler(holdings, cashAccount);

            Assert.That(() => handler.ApplyTransaction(transaction), Throws.Exception.InstanceOf(typeof(NoParcelsForTransaction)));
        }

        [TestCase]
        public void DoNotCreateCashTransaction()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var transaction = new IncomeReceived()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Dividend",
                RecordDate = new Date(2020, 01, 01),
                FrankedAmount = 10.00m,
                UnfrankedAmount = 20.00m,
                FrankingCredits = 30.00m,
                Interest = 40.00m,
                TaxDeferred = 0.00m,
                CreateCashTransaction = false,
                DRPCashBalance = 0.00m
            };

            var drpAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);
            Mock.Get(drpAccount).Setup(x => x.Balance(new Date(2020, 02, 01))).Returns(0.00m);

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.IsEffectiveAt(new Date(2020, 01, 01))).Returns(true);
            Mock.Get(holding).Setup(x => x.DrpAccount).Returns(drpAccount);

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x[stock.Id]).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);

            var handler = new IncomeReceivedHandler(holdings, cashAccount);
            handler.ApplyTransaction(transaction);

            Mock.Verify(new Mock[] { Mock.Get(drpAccount), Mock.Get(holding), Mock.Get(holdings), Mock.Get(cashAccount) });
        }

        [TestCase]
        public void CreateCashTransaction()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var transaction = new IncomeReceived()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Dividend",
                RecordDate = new Date(2020, 01, 01),
                FrankedAmount = 10.00m,
                UnfrankedAmount = 20.00m,
                FrankingCredits = 30.00m,
                Interest = 40.00m,
                TaxDeferred = 0.00m,
                CreateCashTransaction = true,
                DRPCashBalance = 0.00m
            };

            var drpAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);
            Mock.Get(drpAccount).Setup(x => x.Balance(new Date(2020, 02, 01))).Returns(0.00m);

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.IsEffectiveAt(new Date(2020, 01, 01))).Returns(true);
            Mock.Get(holding).Setup(x => x.DrpAccount).Returns(drpAccount);

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x[stock.Id]).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);
            Mock.Get(cashAccount).Setup(x => x.Transfer(new Date(2020, 02, 01), 70.00m, "Distribution for ABC")).Verifiable();

            var handler = new IncomeReceivedHandler(holdings, cashAccount);
            handler.ApplyTransaction(transaction);

            Mock.Verify(new Mock[] { Mock.Get(drpAccount), Mock.Get(holding), Mock.Get(holdings), Mock.Get(cashAccount) });
        }

        [TestCase]
        public void OutstandingDrpBalance()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var transaction = new IncomeReceived()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Dividend",
                RecordDate = new Date(2020, 01, 01),
                FrankedAmount = 10.00m,
                UnfrankedAmount = 20.00m,
                FrankingCredits = 30.00m,
                Interest = 40.00m,
                TaxDeferred = 0.00m,
                CreateCashTransaction = false,
                DRPCashBalance = 50.00m
            };

            var drpAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);
            Mock.Get(drpAccount).Setup(x => x.Balance(new Date(2020, 02, 01))).Returns(100.00m);

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.IsEffectiveAt(new Date(2020, 01, 01))).Returns(true);
            Mock.Get(holding).Setup(x => x.DrpAccount).Returns(drpAccount);
            Mock.Get(holding).Setup(x => x.AddDrpAccountAmount(new Date(2020, 02, 01), -50.00m)).Verifiable();

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x[stock.Id]).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);

            var handler = new IncomeReceivedHandler(holdings, cashAccount);
            handler.ApplyTransaction(transaction);

            Mock.Verify(new Mock[] { Mock.Get(drpAccount), Mock.Get(holding), Mock.Get(holdings), Mock.Get(cashAccount) });
        }

        [TestCase]
        public void TaxDeferredAmount()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var transaction = new IncomeReceived()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Dividend",
                RecordDate = new Date(2020, 01, 01),
                FrankedAmount = 10.00m,
                UnfrankedAmount = 20.00m,
                FrankingCredits = 30.00m,
                Interest = 40.00m,
                TaxDeferred = 50.00m,
                CreateCashTransaction = false,
                DRPCashBalance = 0.00m
            };

            var drpAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);
            Mock.Get(drpAccount).Setup(x => x.Balance(new Date(2020, 02, 01))).Returns(0.00m);

            var parcel1 = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel1).Setup(x => x.Properties[new Date(2020, 01, 01)]).Returns(new ParcelProperties(100, 1000.00m, 1000.00m));
            Mock.Get(parcel1).Setup(x => x.Change(new Date(2020, 01, 01), 0, 0.00m, 14.29m, transaction)).Verifiable();
            var parcel2 = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel2).Setup(x => x.Properties[new Date(2020, 01, 01)]).Returns(new ParcelProperties(50, 500.00m, 500.00m));
            Mock.Get(parcel2).Setup(x => x.Change(new Date(2020, 01, 01), 0, 0.00m, 7.14m, transaction)).Verifiable();
            var parcel3 = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel3).Setup(x => x.Properties[new Date(2020, 01, 01)]).Returns(new ParcelProperties(200, 1000.00m, 1000.00m));
            Mock.Get(parcel3).Setup(x => x.Change(new Date(2020, 01, 01), 0, 0.00m, 28.57m, transaction)).Verifiable(); 

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.IsEffectiveAt(new Date(2020, 01, 01))).Returns(true);
            Mock.Get(holding).Setup(x => x.DrpAccount).Returns(drpAccount);
            Mock.Get(holding).Setup(x => x[new Date(2020, 01, 01)]).Returns(new IParcel[] { parcel1, parcel2, parcel3 });

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x[stock.Id]).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);

            var handler = new IncomeReceivedHandler(holdings, cashAccount);
            handler.ApplyTransaction(transaction);

            Mock.Verify(new Mock[] { Mock.Get(drpAccount), Mock.Get(holding), Mock.Get(holdings), Mock.Get(cashAccount), Mock.Get(parcel1), Mock.Get(parcel2), Mock.Get(parcel3) });
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

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);

            var handler = new IncomeReceivedHandler(holdings, cashAccount);

            Assert.That(() => handler.ApplyTransaction(transaction), Throws.ArgumentException);
        }

    }
}
