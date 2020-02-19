using System;
using System.Linq;

using NUnit.Framework;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Utils;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain.Test.Transactions
{
    class DisposalTests
    {
        [TestCase]
        public void NoSharesOwned()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var transaction = new Disposal()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Disposal",
                Units = 100,
                AveragePrice = 10.00m,
                TransactionCosts = 19.95m,
                CGTMethod = CGTCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = true
            };

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x[stock.Id]).Returns(default(IHolding));

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);

            var cgtEventCollection = Mock.Of<ICgtEventCollection>(MockBehavior.Strict);

            var handler = new DisposalHandler(holdings, cashAccount, cgtEventCollection);

            Assert.That(() => handler.ApplyTransaction(transaction), Throws.Exception.InstanceOf(typeof(NoParcelsForTransaction)));
        }

        [TestCase]
        public void NotEnoughShares()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var transaction = new Disposal()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Disposal",
                Units = 100,
                AveragePrice = 10.00m,
                TransactionCosts = 19.95m,
                CGTMethod = CGTCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = true
            };

            var parcel = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel).Setup(x => x.Properties[new Date(2020, 01, 01)]).Returns(new ParcelProperties(50, 1000.00m, 1500.00m));

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new HoldingProperties(50, 1000.00m, 1000.00m));
            Mock.Get(holding).Setup(x => x[new Date(2020, 02, 01)]).Returns(new IParcel[] { parcel });

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x[stock.Id]).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);

            var cgtEventCollection = Mock.Of<ICgtEventCollection>(MockBehavior.Strict);

            var handler = new DisposalHandler(holdings, cashAccount, cgtEventCollection);

            Assert.That(() => handler.ApplyTransaction(transaction), Throws.Exception.InstanceOf(typeof(NotEnoughSharesForDisposal)));
        }

        [TestCase]
        public void SingleParcelFullySold()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var transaction = new Disposal()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Disposal",
                Units = 100,
                AveragePrice = 20.00m,
                TransactionCosts = 19.95m,
                CGTMethod = CGTCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = true
            };

            var parcel = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel).Setup(x => x.EffectivePeriod).Returns(new DateRange(new Date(2007, 01, 01), Date.MaxValue));
            Mock.Get(parcel).Setup(x => x.AquisitionDate).Returns(new Date(2007, 01, 01));
            Mock.Get(parcel).Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(100, 1000.00m, 1500.00m));

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            Mock.Get(holding).Setup(x => x[new Date(2020, 02, 01)]).Returns(new IParcel[] { parcel });
            Mock.Get(holding).Setup(x => x.DisposeOfParcel(parcel, new Date(2020, 02, 01), 100, 1980.05m, transaction)).Verifiable();

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x[stock.Id]).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);
            Mock.Get(cashAccount).Setup(x => x.Transfer(new Date(2020, 02, 01), 2000.00m, "Sale of ABC")).Verifiable();
            Mock.Get(cashAccount).Setup(x => x.FeeDeducted(new Date(2020, 02, 01), 19.95m, "Brokerage for sale of ABC")).Verifiable();

            var cgtEventCollection = Mock.Of<ICgtEventCollection>(MockBehavior.Strict);
            Mock.Get(cgtEventCollection).Setup(x => x.Add(new Date(2020, 02, 01), stock, 100, 1500.00m, 1980.05m, 480.05m, CGTMethod.Discount)).Verifiable();

            var handler = new DisposalHandler(holdings, cashAccount, cgtEventCollection);
            handler.ApplyTransaction(transaction);

            Mock.Verify(new Mock[] { Mock.Get(parcel), Mock.Get(holding), Mock.Get(holdings), Mock.Get(cashAccount), Mock.Get(cgtEventCollection) });
        }

        [TestCase]
        public void SingleParcelPartiallySold()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var transaction = new Disposal()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Disposal",
                Units = 100,
                AveragePrice = 20.00m,
                TransactionCosts = 19.95m,
                CGTMethod = CGTCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = true
            };

            var parcel = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel).Setup(x => x.EffectivePeriod).Returns(new DateRange(new Date(2007, 01, 01), Date.MaxValue));
            Mock.Get(parcel).Setup(x => x.AquisitionDate).Returns(new Date(2007, 01, 01));
            Mock.Get(parcel).Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(200, 1000.00m, 1500.00m));

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new HoldingProperties(200, 1000.00m, 1000.00m));
            Mock.Get(holding).Setup(x => x[new Date(2020, 02, 01)]).Returns(new IParcel[] { parcel });
            Mock.Get(holding).Setup(x => x.DisposeOfParcel(parcel, new Date(2020, 02, 01), 100, 1980.05m, transaction)).Verifiable();

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x[stock.Id]).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);
            Mock.Get(cashAccount).Setup(x => x.Transfer(new Date(2020, 02, 01), 2000.00m, "Sale of ABC")).Verifiable();
            Mock.Get(cashAccount).Setup(x => x.FeeDeducted(new Date(2020, 02, 01), 19.95m, "Brokerage for sale of ABC")).Verifiable();

            var cgtEventCollection = Mock.Of<ICgtEventCollection>(MockBehavior.Strict);
            Mock.Get(cgtEventCollection).Setup(x => x.Add(new Date(2020, 02, 01), stock, 100, 750.00m, 1980.05m, 1230.05m, CGTMethod.Discount)).Verifiable();

            var handler = new DisposalHandler(holdings, cashAccount, cgtEventCollection);
            handler.ApplyTransaction(transaction);

            Mock.Verify(new Mock[] { Mock.Get(parcel), Mock.Get(holding), Mock.Get(holdings), Mock.Get(cashAccount), Mock.Get(cgtEventCollection) });
        }

        [TestCase]
        public void MultipleParcelsFullySold()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var transaction = new Disposal()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Disposal",
                Units = 350,
                AveragePrice = 20.00m,
                TransactionCosts = 19.95m,
                CGTMethod = CGTCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = true
            };

            var parcel1 = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel1).Setup(x => x.EffectivePeriod).Returns(new DateRange(new Date(2007, 01, 01), Date.MaxValue));
            Mock.Get(parcel1).Setup(x => x.AquisitionDate).Returns(new Date(2007, 01, 01));
            Mock.Get(parcel1).Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(100, 1000.00m, 1500.00m));

            var parcel2 = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel2).Setup(x => x.EffectivePeriod).Returns(new DateRange(new Date(2008, 01, 01), Date.MaxValue));
            Mock.Get(parcel2).Setup(x => x.AquisitionDate).Returns(new Date(2008, 01, 01));
            Mock.Get(parcel2).Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(50, 200.00m, 300.00m));

            var parcel3 = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel3).Setup(x => x.EffectivePeriod).Returns(new DateRange(new Date(2009, 01, 01), Date.MaxValue));
            Mock.Get(parcel3).Setup(x => x.AquisitionDate).Returns(new Date(2007, 01, 01));
            Mock.Get(parcel3).Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(200, 1500.00m, 2000.00m));


            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new HoldingProperties(350, 2700.00m, 3800.00m));
            Mock.Get(holding).Setup(x => x[new Date(2020, 02, 01)]).Returns(new IParcel[] { parcel1, parcel2, parcel3 });
            Mock.Get(holding).Setup(x => x.DisposeOfParcel(parcel1, new Date(2020, 02, 01), 100, 1994.30m, transaction)).Verifiable();
            Mock.Get(holding).Setup(x => x.DisposeOfParcel(parcel2, new Date(2020, 02, 01), 50, 997.15m, transaction)).Verifiable();
            Mock.Get(holding).Setup(x => x.DisposeOfParcel(parcel3, new Date(2020, 02, 01), 200, 3988.60m, transaction)).Verifiable();

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x[stock.Id]).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);
            Mock.Get(cashAccount).Setup(x => x.Transfer(new Date(2020, 02, 01), 7000.00m, "Sale of ABC")).Verifiable();
            Mock.Get(cashAccount).Setup(x => x.FeeDeducted(new Date(2020, 02, 01), 19.95m, "Brokerage for sale of ABC")).Verifiable();

            var cgtEventCollection = Mock.Of<ICgtEventCollection>(MockBehavior.Strict);
            Mock.Get(cgtEventCollection).Setup(x => x.Add(new Date(2020, 02, 01), stock, 100, 1500.00m, 1994.30m, 494.30m, CGTMethod.Discount)).Verifiable();
            Mock.Get(cgtEventCollection).Setup(x => x.Add(new Date(2020, 02, 01), stock, 50, 300.00m, 997.15m, 697.15m, CGTMethod.Discount)).Verifiable();
            Mock.Get(cgtEventCollection).Setup(x => x.Add(new Date(2020, 02, 01), stock, 200, 2000.00m, 3988.60m, 1988.60m, CGTMethod.Discount)).Verifiable();

            var handler = new DisposalHandler(holdings, cashAccount, cgtEventCollection);
            handler.ApplyTransaction(transaction);

            Mock.Verify(new Mock[] { Mock.Get(parcel1), Mock.Get(parcel2), Mock.Get(parcel3), Mock.Get(holding), Mock.Get(holdings), Mock.Get(cashAccount), Mock.Get(cgtEventCollection) });
        }

        [TestCase]
        public void MultipleParcelsPartiallySold()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var transaction = new Disposal()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Disposal",
                Units = 180,
                AveragePrice = 20.00m,
                TransactionCosts = 19.95m,
                CGTMethod = CGTCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = true
            };

            var parcel1 = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel1).Setup(x => x.EffectivePeriod).Returns(new DateRange(new Date(2007, 01, 01), Date.MaxValue));
            Mock.Get(parcel1).Setup(x => x.AquisitionDate).Returns(new Date(2007, 01, 01));
            Mock.Get(parcel1).Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(100, 1000.00m, 1500.00m));

            var parcel2 = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel2).Setup(x => x.EffectivePeriod).Returns(new DateRange(new Date(2008, 01, 01), Date.MaxValue));
            Mock.Get(parcel2).Setup(x => x.AquisitionDate).Returns(new Date(2008, 01, 01));
            Mock.Get(parcel2).Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(50, 200.00m, 300.00m));

            var parcel3 = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel3).Setup(x => x.EffectivePeriod).Returns(new DateRange(new Date(2009, 01, 01), Date.MaxValue));
            Mock.Get(parcel3).Setup(x => x.AquisitionDate).Returns(new Date(2009, 01, 01));
            Mock.Get(parcel3).Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(200, 1500.00m, 2000.00m));


            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new HoldingProperties(350, 2700.00m, 3800.00m));
            Mock.Get(holding).Setup(x => x[new Date(2020, 02, 01)]).Returns(new IParcel[] { parcel1, parcel2, parcel3 });
            Mock.Get(holding).Setup(x => x.DisposeOfParcel(parcel1, new Date(2020, 02, 01), 100, 1988.92m, transaction)).Verifiable();
            Mock.Get(holding).Setup(x => x.DisposeOfParcel(parcel2, new Date(2020, 02, 01), 50, 994.46m, transaction)).Verifiable();
            Mock.Get(holding).Setup(x => x.DisposeOfParcel(parcel3, new Date(2020, 02, 01), 30, 596.67m, transaction)).Verifiable();

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x[stock.Id]).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);
            Mock.Get(cashAccount).Setup(x => x.Transfer(new Date(2020, 02, 01), 3600.00m, "Sale of ABC")).Verifiable();
            Mock.Get(cashAccount).Setup(x => x.FeeDeducted(new Date(2020, 02, 01), 19.95m, "Brokerage for sale of ABC")).Verifiable();

            var cgtEventCollection = Mock.Of<ICgtEventCollection>(MockBehavior.Strict);
            Mock.Get(cgtEventCollection).Setup(x => x.Add(new Date(2020, 02, 01), stock, 100, 1500.00m, 1988.92m, 488.92m, CGTMethod.Discount)).Verifiable();
            Mock.Get(cgtEventCollection).Setup(x => x.Add(new Date(2020, 02, 01), stock, 50, 300.00m, 994.46m, 694.46m, CGTMethod.Discount)).Verifiable();
            Mock.Get(cgtEventCollection).Setup(x => x.Add(new Date(2020, 02, 01), stock, 30, 300.00m, 596.67m, 296.67m, CGTMethod.Discount)).Verifiable();

            var handler = new DisposalHandler(holdings, cashAccount, cgtEventCollection);
            handler.ApplyTransaction(transaction);

            Mock.Verify(new Mock[] { Mock.Get(parcel1), Mock.Get(parcel2), Mock.Get(parcel3), Mock.Get(holding), Mock.Get(holdings), Mock.Get(cashAccount), Mock.Get(cgtEventCollection) });
        }

        [TestCase]
        public void DoNotCreateCashTransaction()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var transaction = new Disposal()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Disposal",
                Units = 100,
                AveragePrice = 20.00m,
                TransactionCosts = 19.95m,
                CGTMethod = CGTCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = false
            };

            var parcel = Mock.Of<IParcel>(MockBehavior.Strict);
            Mock.Get(parcel).Setup(x => x.EffectivePeriod).Returns(new DateRange(new Date(2007, 01, 01), Date.MaxValue));
            Mock.Get(parcel).Setup(x => x.AquisitionDate).Returns(new Date(2007, 01, 01));
            Mock.Get(parcel).Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(200, 1000.00m, 1500.00m));

            var holding = Mock.Of<IHolding>(MockBehavior.Strict);
            Mock.Get(holding).Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new HoldingProperties(200, 1000.00m, 1000.00m));
            Mock.Get(holding).Setup(x => x[new Date(2020, 02, 01)]).Returns(new IParcel[] { parcel });
            Mock.Get(holding).Setup(x => x.DisposeOfParcel(parcel, new Date(2020, 02, 01), 100, 1980.05m, transaction)).Verifiable();

            var holdings = Mock.Of<IHoldingCollection>(MockBehavior.Strict);
            Mock.Get(holdings).Setup(x => x[stock.Id]).Returns(holding);

            var cashAccount = Mock.Of<ICashAccount>(MockBehavior.Strict);

            var cgtEventCollection = Mock.Of<ICgtEventCollection>(MockBehavior.Strict);
            Mock.Get(cgtEventCollection).Setup(x => x.Add(new Date(2020, 02, 01), stock, 100, 750.00m, 1980.05m, 1230.05m, CGTMethod.Discount)).Verifiable();

            var handler = new DisposalHandler(holdings, cashAccount, cgtEventCollection);
            handler.ApplyTransaction(transaction);

            Mock.Verify(new Mock[] { Mock.Get(parcel), Mock.Get(holding), Mock.Get(holdings), Mock.Get(cashAccount), Mock.Get(cgtEventCollection) });

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
            var cgtEvents = new CgtEventCollection();

            var handler = new DisposalHandler(holdings, cashAccount, cgtEvents);

            Assert.That(() => handler.ApplyTransaction(transaction), Throws.ArgumentException);
        }

    }
}
