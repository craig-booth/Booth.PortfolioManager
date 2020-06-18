using System;
using System.Linq;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Utils;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain.Test.Transactions
{
    public class DisposalTests
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

            var handler = new DisposalHandler();

            Action a = () => handler.Apply(transaction, holding.Object, cashAccount.Object);
a.Should().Throw<ArgumentException>();

            mockRepository.Verify();
        }

        [Fact]
        public void NoSharesOwned()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var transaction = new Disposal()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Disposal",
                Units = 100,
                AveragePrice = 10.00m,
                TransactionCosts = 19.95m,
                CgtMethod = CgtCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = true
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.IsEffectiveAt(new Date(2020, 02, 01))).Returns(false);

            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new DisposalHandler();

            Action a = () => handler.Apply(transaction, holding.Object, cashAccount.Object);

            a.Should().Throw<NoSharesOwnedException>();

            mockRepository.Verify();
        }

        [Fact]
        public void NotEnoughShares()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var transaction = new Disposal()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Disposal",
                Units = 100,
                AveragePrice = 10.00m,
                TransactionCosts = 19.95m,
                CgtMethod = CgtCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = true
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var parcel = mockRepository.Create<IParcel>();

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.IsEffectiveAt(new Date(2020, 02, 01))).Returns(true);
            holding.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new HoldingProperties(50, 1000.00m, 1000.00m));
            holding.Setup(x => x.Parcels(new Date(2020, 02, 01))).Returns(new IParcel[] { parcel.Object });

            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new DisposalHandler();

            Action a = () => handler.Apply(transaction, holding.Object,cashAccount.Object);

            a.Should().Throw<NotEnoughSharesForDisposal>();

            mockRepository.Verify();
        }

        [Fact]
        public void SingleParcelFullySold()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var transaction = new Disposal()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Disposal",
                Units = 100,
                AveragePrice = 20.00m,
                TransactionCosts = 19.95m,
                CgtMethod = CgtCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = true
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var parcelId = Guid.NewGuid();
            var parcel = mockRepository.Create<IParcel>();
            parcel.Setup(x => x.Id).Returns(parcelId);
            parcel.Setup(x => x.EffectivePeriod).Returns(new DateRange(new Date(2007, 01, 01), Date.MaxValue));
            parcel.Setup(x => x.AquisitionDate).Returns(new Date(2007, 01, 01));
            parcel.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(100, 1000.00m, 1500.00m));

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.IsEffectiveAt(new Date(2020, 02, 01))).Returns(true);
            holding.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Parcels(new Date(2020, 02, 01))).Returns(new IParcel[] { parcel.Object });
            holding.Setup(x => x.DisposeOfParcel(parcelId, new Date(2020, 02, 01), 100, 1980.05m, 480.05m, CgtMethod.Discount, transaction)).Verifiable();

            var cashAccount = mockRepository.Create<ICashAccount>();
            cashAccount.Setup(x => x.Transfer(new Date(2020, 02, 01), 2000.00m, "Sale of ABC")).Verifiable();
            cashAccount.Setup(x => x.FeeDeducted(new Date(2020, 02, 01), 19.95m, "Brokerage for sale of ABC")).Verifiable();

            var handler = new DisposalHandler();
            handler.Apply(transaction, holding.Object, cashAccount.Object);

            mockRepository.Verify();
        }

        [Fact]
        public void SingleParcelPartiallySold()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var transaction = new Disposal()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Disposal",
                Units = 100,
                AveragePrice = 20.00m,
                TransactionCosts = 19.95m,
                CgtMethod = CgtCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = true
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var parcelId = Guid.NewGuid();
            var parcel = mockRepository.Create<IParcel>();
            parcel.Setup(x => x.Id).Returns(parcelId);
            parcel.Setup(x => x.EffectivePeriod).Returns(new DateRange(new Date(2007, 01, 01), Date.MaxValue));
            parcel.Setup(x => x.AquisitionDate).Returns(new Date(2007, 01, 01));
            parcel.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(200, 1000.00m, 1500.00m));

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.IsEffectiveAt(new Date(2020, 02, 01))).Returns(true);
            holding.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new HoldingProperties(200, 1000.00m, 1000.00m));
            holding.Setup(x => x.Parcels(new Date(2020, 02, 01))).Returns(new IParcel[] { parcel.Object });
            holding.Setup(x => x.DisposeOfParcel(parcelId, new Date(2020, 02, 01), 100, 1980.05m, 1230.05m, CgtMethod.Discount, transaction)).Verifiable();

            var cashAccount = mockRepository.Create<ICashAccount>();
            cashAccount.Setup(x => x.Transfer(new Date(2020, 02, 01), 2000.00m, "Sale of ABC")).Verifiable();
            cashAccount.Setup(x => x.FeeDeducted(new Date(2020, 02, 01), 19.95m, "Brokerage for sale of ABC")).Verifiable();

            var handler = new DisposalHandler();
            handler.Apply(transaction, holding.Object, cashAccount.Object);

            mockRepository.Verify();
        }

        [Fact]
        public void MultipleParcelsFullySold()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var transaction = new Disposal()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Disposal",
                Units = 350,
                AveragePrice = 20.00m,
                TransactionCosts = 19.95m,
                CgtMethod = CgtCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = true
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var parcel1Id = Guid.NewGuid();
            var parcel1 = mockRepository.Create<IParcel>();
            parcel1.Setup(x => x.Id).Returns(parcel1Id);
            parcel1.Setup(x => x.EffectivePeriod).Returns(new DateRange(new Date(2007, 01, 01), Date.MaxValue));
            parcel1.Setup(x => x.AquisitionDate).Returns(new Date(2007, 01, 01));
            parcel1.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(100, 1000.00m, 1500.00m));

            var parcel2Id = Guid.NewGuid();
            var parcel2 = mockRepository.Create<IParcel>();
            parcel2.Setup(x => x.Id).Returns(parcel2Id);
            parcel2.Setup(x => x.EffectivePeriod).Returns(new DateRange(new Date(2008, 01, 01), Date.MaxValue));
            parcel2.Setup(x => x.AquisitionDate).Returns(new Date(2008, 01, 01));
            parcel2.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(50, 200.00m, 300.00m));

            var parcel3Id = Guid.NewGuid();
            var parcel3 = mockRepository.Create<IParcel>();
            parcel3.Setup(x => x.Id).Returns(parcel3Id);
            parcel3.Setup(x => x.EffectivePeriod).Returns(new DateRange(new Date(2009, 01, 01), Date.MaxValue));
            parcel3.Setup(x => x.AquisitionDate).Returns(new Date(2007, 01, 01));
            parcel3.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(200, 1500.00m, 2000.00m));


            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.IsEffectiveAt(new Date(2020, 02, 01))).Returns(true);
            holding.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new HoldingProperties(350, 2700.00m, 3800.00m));
            holding.Setup(x => x.Parcels(new Date(2020, 02, 01))).Returns(new IParcel[] { parcel1.Object, parcel2.Object, parcel3.Object });
            holding.Setup(x => x.DisposeOfParcel(parcel1Id, new Date(2020, 02, 01), 100, 1994.30m, 494.30m, CgtMethod.Discount, transaction)).Verifiable();
            holding.Setup(x => x.DisposeOfParcel(parcel2Id, new Date(2020, 02, 01), 50, 997.15m, 697.15m, CgtMethod.Discount, transaction)).Verifiable();
            holding.Setup(x => x.DisposeOfParcel(parcel3Id, new Date(2020, 02, 01), 200, 3988.60m, 1988.60m, CgtMethod.Discount, transaction)).Verifiable();

            var cashAccount = mockRepository.Create<ICashAccount>();
            cashAccount.Setup(x => x.Transfer(new Date(2020, 02, 01), 7000.00m, "Sale of ABC")).Verifiable();
            cashAccount.Setup(x => x.FeeDeducted(new Date(2020, 02, 01), 19.95m, "Brokerage for sale of ABC")).Verifiable();

             var handler = new DisposalHandler();
            handler.Apply(transaction, holding.Object, cashAccount.Object);

            mockRepository.Verify();
        }

        [Fact]
        public void MultipleParcelsPartiallySold()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var transaction = new Disposal()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Disposal",
                Units = 180,
                AveragePrice = 20.00m,
                TransactionCosts = 19.95m,
                CgtMethod = CgtCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = true
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var parcel1Id = Guid.NewGuid();
            var parcel1 = mockRepository.Create<IParcel>();
            parcel1.Setup(x => x.Id).Returns(parcel1Id);
            parcel1.Setup(x => x.EffectivePeriod).Returns(new DateRange(new Date(2007, 01, 01), Date.MaxValue));
            parcel1.Setup(x => x.AquisitionDate).Returns(new Date(2007, 01, 01));
            parcel1.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(100, 1000.00m, 1500.00m));

            var parcel2Id = Guid.NewGuid();
            var parcel2 = mockRepository.Create<IParcel>();
            parcel2.Setup(x => x.Id).Returns(parcel2Id);
            parcel2.Setup(x => x.EffectivePeriod).Returns(new DateRange(new Date(2008, 01, 01), Date.MaxValue));
            parcel2.Setup(x => x.AquisitionDate).Returns(new Date(2008, 01, 01));
            parcel2.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(50, 200.00m, 300.00m));

            var parcel3Id = Guid.NewGuid();
            var parcel3 = mockRepository.Create<IParcel>();
            parcel3.Setup(x => x.Id).Returns(parcel3Id);
            parcel3.Setup(x => x.EffectivePeriod).Returns(new DateRange(new Date(2009, 01, 01), Date.MaxValue));
            parcel3.Setup(x => x.AquisitionDate).Returns(new Date(2009, 01, 01));
            parcel3.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(200, 1500.00m, 2000.00m));

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.IsEffectiveAt(new Date(2020, 02, 01))).Returns(true);
            holding.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new HoldingProperties(350, 2700.00m, 3800.00m));
            holding.Setup(x => x.Parcels(new Date(2020, 02, 01))).Returns(new IParcel[] { parcel1.Object, parcel2.Object, parcel3.Object });
            holding.Setup(x => x.DisposeOfParcel(parcel1Id, new Date(2020, 02, 01), 100, 1988.92m, 488.92m, CgtMethod.Discount, transaction)).Verifiable();
            holding.Setup(x => x.DisposeOfParcel(parcel2Id, new Date(2020, 02, 01), 50, 994.46m, 694.46m, CgtMethod.Discount, transaction)).Verifiable();
            holding.Setup(x => x.DisposeOfParcel(parcel3Id, new Date(2020, 02, 01), 30, 596.67m, 296.67m, CgtMethod.Discount, transaction)).Verifiable();

            var cashAccount = mockRepository.Create<ICashAccount>();
            cashAccount.Setup(x => x.Transfer(new Date(2020, 02, 01), 3600.00m, "Sale of ABC")).Verifiable();
            cashAccount.Setup(x => x.FeeDeducted(new Date(2020, 02, 01), 19.95m, "Brokerage for sale of ABC")).Verifiable();

            var handler = new DisposalHandler();
            handler.Apply(transaction, holding.Object, cashAccount.Object);

            mockRepository.Verify();
        }

        [Fact]
        public void DoNotCreateCashTransaction()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var transaction = new Disposal()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Disposal",
                Units = 100,
                AveragePrice = 20.00m,
                TransactionCosts = 19.95m,
                CgtMethod = CgtCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = false
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var parcelId = Guid.NewGuid();
            var parcel = mockRepository.Create<IParcel>();
            parcel.Setup(x => x.Id).Returns(parcelId);
            parcel.Setup(x => x.EffectivePeriod).Returns(new DateRange(new Date(2007, 01, 01), Date.MaxValue));
            parcel.Setup(x => x.AquisitionDate).Returns(new Date(2007, 01, 01));
            parcel.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new ParcelProperties(200, 1000.00m, 1500.00m));

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.IsEffectiveAt(new Date(2020, 02, 01))).Returns(true);
            holding.Setup(x => x.Properties[new Date(2020, 02, 01)]).Returns(new HoldingProperties(200, 1000.00m, 1000.00m));
            holding.Setup(x => x.Parcels(new Date(2020, 02, 01))).Returns(new IParcel[] { parcel.Object });
            holding.Setup(x => x.DisposeOfParcel(parcelId, new Date(2020, 02, 01), 100, 1980.05m, 1230.05m, CgtMethod.Discount, transaction)).Verifiable();

            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new DisposalHandler();
            handler.Apply(transaction, holding.Object, cashAccount.Object);

            mockRepository.Verify();
        }
    }
}
