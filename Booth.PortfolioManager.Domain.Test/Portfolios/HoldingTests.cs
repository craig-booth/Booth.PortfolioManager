using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Stocks;


namespace Booth.PortfolioManager.Domain.Test.Portfolios
{
    public class HoldingTests
    {

        [Fact]
        public void AccessParcelsByDateNoParcels()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));

            holding.Parcels(new Date(2000, 01, 01)).Should().BeEmpty();
        }

        [Fact]
        public void AccessParcelsByDateParcelsExistButNotAtDate()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            holding.Parcels(new Date(1999, 01, 01)).Should().BeEmpty();
        }

        [Fact]
        public void AccessParcelsByDateParcelsExistAtDate()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            holding.Parcels(new Date(2002, 01, 01)).Should().HaveCount(2);
        }

        [Fact]
        public void GetParcelsNoParcels()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));

            holding.Parcels().Should().BeEmpty();
        }

        [Fact]
        public void GetParcelsByDateParcelsExiste()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            holding.Parcels().Should().HaveCount(2);
        }

        [Fact]
        public void GetParcelsByDateNoParcels()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));

            holding.Parcels(new Date(2000, 01, 01)).Should().BeEmpty();
        }

        [Fact]
        public void GetParcelsByDateParcelsExistButNotAtDate()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            holding.Parcels(new Date(1999, 01, 01)).Should().BeEmpty();
        }

        [Fact]
        public void GetParcelsByDateParcelsExistAtDate()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            holding.Parcels(new Date(2002, 01, 01)).Should().HaveCount(2);
        }

        [Fact]
        public void GetParcelsByDateRangeNoParcels()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));

            holding.Parcels(new DateRange(new Date(1974, 01, 01), Date.MaxValue)).Should().BeEmpty();
        }

        [Fact]
        public void GetParcelsByDateRangeParcelsExistButNotInRange()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            holding.Parcels(new DateRange(new Date(1999, 01, 01), new Date(1999, 12, 31))).Should().BeEmpty();
        }

        [Fact]
        public void GetParcelsByDateParcelsExistInRange()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            holding.Parcels(new DateRange(new Date(2001, 01, 01), Date.MaxValue)).Should().HaveCount(2);
        }

        [Fact]
        public void AddParcelNoExistingHoldings()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);

            using (new AssertionScope())
            {
                var properties = holding[new Date(2000, 01, 01)];
                properties.Should().Be(new HoldingProperties(100, 1000.00m, 1200.00m));

                holding.Settings.ParticipateInDrp.Should().BeFalse();
            }
        }

        [Fact]
        public void AddParcelExistingHoldings()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            var properties = holding[new Date(2001, 01, 01)];

            properties.Should().Be(new HoldingProperties(300, 3000.00m, 3400.00m));
        }

        [Fact]
        public void DisposeOfParcelNotInHoldings()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);

            var parcel = new Parcel(Guid.NewGuid(), new Date(2000, 01, 01), new Date(2000, 01, 01), new ParcelProperties(100, 1000.00m, 1200.00m), null);

            Action a = () => holding.DisposeOfParcel(parcel.Id, new Date(2001, 01, 01), 100, 1000.00m, 500.00m, CgtMethod.Other, null);
            
            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void DisposeOfParcelPartialSale()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var eventSink = mockRepository.Create<ICgtEventSink>();
            CgtEventArgs cgtEvent = null;
            eventSink.Setup(x => x.OnCgtEventOccured(It.IsAny<Holding>(), It.IsAny<CgtEventArgs>()))
                .Callback<object, CgtEventArgs>((o, e) => cgtEvent = e)
                .Verifiable();

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.CgtEventOccurred += eventSink.Object.OnCgtEventOccured;

            var parcel = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 200, 2000.00m, 2200.00m, null);

            var transaction = new Disposal();
            holding.DisposeOfParcel(parcel.Id, new Date(2001, 01, 01), 99, 500.00m, 10.00m, CgtMethod.Discount, transaction);

            using (new AssertionScope())
            {
                holding[new Date(2000, 12, 31)].Should().Be(new HoldingProperties(300, 3000.00m, 3400.00m));
                holding[new Date(2001, 01, 01)].Should().Be(new HoldingProperties(201, 2010.00m, 2212.00m));

                parcel.Properties[new Date(2000, 12, 31)].Should().Be(new ParcelProperties(100, 1000.00m, 1200.00m));
                parcel.Properties[new Date(2001, 01, 01)].Should().Be(new ParcelProperties(1, 10.00m, 12.00m));

                cgtEvent.Should().BeEquivalentTo(new
                {
                    EventDate= new Date(2001, 01, 01),
                    Stock = stock,
                    AmountReceived = 500.00m,
                    CapitalGain = 10.00m,
                    CgtMethod = CgtMethod.Discount,
                    Transaction = transaction
                });
            }

            mockRepository.Verify();
        }

        [Fact]
        public void DisposeOfParcelFullSale()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var eventSink = mockRepository.Create<ICgtEventSink>();
            CgtEventArgs cgtEvent = null;
            eventSink.Setup(x => x.OnCgtEventOccured(It.IsAny<Holding>(), It.IsAny<CgtEventArgs>()))
                .Callback<object, CgtEventArgs>((o, e) => cgtEvent = e)
                .Verifiable();

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.CgtEventOccurred += eventSink.Object.OnCgtEventOccured;

            var parcel = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            var transaction = new Disposal();
            holding.DisposeOfParcel(parcel.Id, new Date(2002, 01, 01), 100, 500.00m, 10.00m, CgtMethod.Discount, transaction);

            using (new AssertionScope())
            {
                holding.Properties[new Date(2001, 12, 31)].Should().Be(new HoldingProperties(300, 3000.00m, 3400.00m));
                holding.Properties[new Date(2002, 01, 01)].Should().Be(new HoldingProperties(200, 2000.00m, 2200.00m));

                parcel.Properties[new Date(2001, 12, 31)].Should().Be(new ParcelProperties(100, 1000.00m, 1200.00m));

                parcel.EffectivePeriod.ToDate.Should().Be(new Date(2002, 01, 01));

                cgtEvent.Should().BeEquivalentTo(new
                {
                    EventDate = new Date(2002, 01, 01),
                    Stock = stock,
                    AmountReceived = 500.00m,
                    CapitalGain = 10.00m,
                    CgtMethod = CgtMethod.Discount,
                    Transaction = transaction
                });
            }
        }

        [Fact]
        public void DisposeOfParcelMoreUnitsThanInParcel()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            var parcel = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);

            Action a = () => holding.DisposeOfParcel(parcel.Id, new Date(2001, 01, 01), 200, 1000.00m, 10.00m, CgtMethod.Discount, null);

            a.Should().Throw<NotEnoughSharesForDisposal>();
        }

        [Fact]
        public void DisposeOfLastParcel()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var eventSink = mockRepository.Create<ICgtEventSink>();
            CgtEventArgs cgtEvent = null;
            eventSink.Setup(x => x.OnCgtEventOccured(It.IsAny<Holding>(), It.IsAny<CgtEventArgs>()))
                .Callback<object, CgtEventArgs>((o, e) => cgtEvent = e)
                .Verifiable();

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.CgtEventOccurred += eventSink.Object.OnCgtEventOccured;

            var parcel = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);

            var transaction = new Disposal();
            holding.DisposeOfParcel(parcel.Id, new Date(2001, 01, 01), 100, 500.00m, 10.00m, CgtMethod.Discount, transaction);

            using (new AssertionScope())
            {
                holding.Properties[new Date(2000, 12, 31)].Should().Be(new HoldingProperties(100, 1000.00m, 1200.00m));
                holding.Properties[new Date(2001, 01, 01)].Should().Be(new HoldingProperties(0, 0.00m, 0.00m));

                holding.EffectivePeriod.ToDate.Should().Be(new Date(2001, 01, 01));

                cgtEvent.Should().BeEquivalentTo(new
                {
                    EventDate = new Date(2001, 01, 01),
                    Stock = stock,
                    AmountReceived = 500.00m,
                    CapitalGain = 10.00m,
                    CgtMethod = CgtMethod.Discount,
                    Transaction = transaction
                });
            }

        }

        [Fact]
        public void ChangeParcelUnitCountNoParcels()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));

            var parcel = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);

            var transaction = new UnitCountAdjustment();
            Action a = () => holding.ChangeParcelUnitCount(Guid.NewGuid(), new Date(2001, 01, 01), 50, transaction);

            a.Should().Throw<ArgumentException>();

        }

        [Fact]
        public void ChangeParcelUnitCountSingleParcel()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));

            var parcel = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);

            var transaction = new UnitCountAdjustment();
            holding.ChangeParcelUnitCount(parcel.Id, new Date(2001, 01, 01), 150, transaction);

            using (new AssertionScope())
            {
                holding.Properties[new Date(2000, 12, 31)].Should().Be(new HoldingProperties(100, 1000.00m, 1200.00m));
                holding.Properties[new Date(2001, 01, 01)].Should().Be(new HoldingProperties(150, 1000.00m, 1200.00m));

                parcel.Properties[new Date(2000, 12, 31)].Should().Be(new ParcelProperties(100, 1000.00m, 1200.00m));
                parcel.Properties[new Date(2001, 01, 01)].Should().Be(new ParcelProperties(150, 1000.00m, 1200.00m));
            }
        }

        [Fact]
        public void ChangeParcelUnitCountMultipleParcels()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));

            var parcel = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            var parcel2 = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 200, 2000.00m, 2200.00m, null);

            var transaction = new UnitCountAdjustment();
            holding.ChangeParcelUnitCount(parcel.Id, new Date(2001, 01, 01), 150, transaction);

            using (new AssertionScope())
            {
                holding.Properties[new Date(2000, 12, 31)].Should().Be(new HoldingProperties(300, 3000.00m, 3400.00m));
                holding.Properties[new Date(2001, 01, 01)].Should().Be(new HoldingProperties(350, 3000.00m, 3400.00m));

                parcel.Properties[new Date(2000, 12, 31)].Should().Be(new ParcelProperties(100, 1000.00m, 1200.00m));
                parcel.Properties[new Date(2001, 01, 01)].Should().Be(new ParcelProperties(150, 1000.00m, 1200.00m));

                parcel2.Properties[new Date(2000, 12, 31)].Should().Be(new ParcelProperties(200, 2000.00m, 2200.00m));
                parcel2.Properties[new Date(2001, 01, 01)].Should().Be(new ParcelProperties(200, 2000.00m, 2200.00m));
            }
        }

        [Fact]
        public void ChangeParcelUnitCountToZero()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));

            var parcel = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 200.00m, null);

            var transaction = new UnitCountAdjustment();
            holding.ChangeParcelUnitCount(parcel.Id, new Date(2001, 01, 01), 0, transaction);

            using (new AssertionScope())
            {
                holding.Properties[new Date(2000, 12, 31)].Should().Be(new HoldingProperties(100, 1000.00m, 200.00m));
                holding.EffectivePeriod.ToDate.Should().Be(new Date(2001, 01, 01));

                parcel.Properties[new Date(2000, 12, 31)].Should().Be(new ParcelProperties(100, 1000.00m, 200.00m));
                parcel.EffectivePeriod.ToDate.Should().Be(new Date(2001, 01, 01));
            }
        }

        [Fact]
        public void ChangeParcelUnitCountToNegative()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var eventSink = mockRepository.Create<ICgtEventSink>();
            CgtEventArgs cgtEvent = null;
            eventSink.Setup(x => x.OnCgtEventOccured(It.IsAny<Holding>(), It.IsAny<CgtEventArgs>()))
                .Callback<object, CgtEventArgs>((o, e) => cgtEvent = e)
                .Verifiable();

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.CgtEventOccurred += eventSink.Object.OnCgtEventOccured;

            var parcel = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 500.00m, null);

            var transaction = new UnitCountAdjustment();
            Action a = () => holding.ChangeParcelUnitCount(parcel.Id, new Date(2001, 01, 01), -50, transaction);

            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ReduceParcelCostBaseNoParcels()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));

            var parcel = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);

            var transaction = new UnitCountAdjustment();
            Action a = () => holding.ReduceParcelCostBase(Guid.NewGuid(), new Date(2001, 01, 01), -20.00m, transaction);

            a.Should().Throw<ArgumentException>();

        }

        [Fact]
        public void ReduceParcelCostBaseSingleParcel()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));

            var parcel = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);

            var transaction = new UnitCountAdjustment();
            holding.ReduceParcelCostBase(parcel.Id, new Date(2001, 01, 01), 20.00m, transaction);

            using (new AssertionScope())
            {
                holding.Properties[new Date(2000, 12, 31)].Should().Be(new HoldingProperties(100, 1000.00m, 1200.00m));
                holding.Properties[new Date(2001, 01, 01)].Should().Be(new HoldingProperties(100, 1000.00m, 1180.00m));

                parcel.Properties[new Date(2000, 12, 31)].Should().Be(new ParcelProperties(100, 1000.00m, 1200.00m));
                parcel.Properties[new Date(2001, 01, 01)].Should().Be(new ParcelProperties(100, 1000.00m, 1180.00m));
            }
        }

        [Fact]
        public void ReduceParcelCostBaseMultipleParcels()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));

            var parcel = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            var parcel2 = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 200, 2000.00m, 2200.00m, null);

            var transaction = new UnitCountAdjustment();
            holding.ReduceParcelCostBase(parcel.Id, new Date(2001, 01, 01), 20.00m, transaction);

            using (new AssertionScope())
            {
                holding.Properties[new Date(2000, 12, 31)].Should().Be(new HoldingProperties(300, 3000.00m, 3400.00m));
                holding.Properties[new Date(2001, 01, 01)].Should().Be(new HoldingProperties(300, 3000.00m, 3380.00m));

                parcel.Properties[new Date(2000, 12, 31)].Should().Be(new ParcelProperties(100, 1000.00m, 1200.00m));
                parcel.Properties[new Date(2001, 01, 01)].Should().Be(new ParcelProperties(100, 1000.00m, 1180.00m));

                parcel2.Properties[new Date(2000, 12, 31)].Should().Be(new ParcelProperties(200, 2000.00m, 2200.00m));
                parcel2.Properties[new Date(2001, 01, 01)].Should().Be(new ParcelProperties(200, 2000.00m, 2200.00m));
            }
        }

        [Fact]
        public void ReduceParcelCostBaseWhenAlreadyZero()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var eventSink = mockRepository.Create<ICgtEventSink>();
            CgtEventArgs cgtEvent = null;
            eventSink.Setup(x => x.OnCgtEventOccured(It.IsAny<Holding>(), It.IsAny<CgtEventArgs>()))
                .Callback<object, CgtEventArgs>((o, e) => cgtEvent = e)
                .Verifiable();

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.CgtEventOccurred += eventSink.Object.OnCgtEventOccured;

            var parcel = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 0.00m, null);

            var transaction = new CostBaseAdjustment();
            holding.ReduceParcelCostBase(parcel.Id, new Date(2001, 01, 01), 200.00m, transaction);

            using (new AssertionScope())
            {
                holding.Properties[new Date(2000, 12, 31)].Should().Be(new HoldingProperties(100, 1000.00m, 0.00m));
                holding.Properties[new Date(2001, 01, 01)].Should().Be(new HoldingProperties(100, 1000.00m, 0.00m));

                parcel.Properties[new Date(2000, 12, 31)].Should().Be(new ParcelProperties(100, 1000.00m, 0.00m));
                parcel.Properties[new Date(2001, 01, 01)].Should().Be(new ParcelProperties(100, 1000.00m, 0.00m));

                cgtEvent.Should().BeEquivalentTo(new
                {
                    EventDate = new Date(2001, 01, 01),
                    Stock = stock,
                    AmountReceived = 200.00m,
                    CapitalGain = 200.00m,
                    CgtMethod = CgtMethod.Discount,
                    Transaction = transaction
                });
            }
        }

        [Fact]
        public void ReduceParcelCostBaseGreaterThanCurrentCostBase()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var eventSink = mockRepository.Create<ICgtEventSink>();
            CgtEventArgs cgtEvent = null;
            eventSink.Setup(x => x.OnCgtEventOccured(It.IsAny<Holding>(), It.IsAny<CgtEventArgs>()))
                .Callback<object, CgtEventArgs>((o, e) => cgtEvent = e)
                .Verifiable();

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.CgtEventOccurred += eventSink.Object.OnCgtEventOccured;

            var parcel = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 500.00m, null);

            var transaction = new CostBaseAdjustment();
            holding.ReduceParcelCostBase(parcel.Id, new Date(2001, 01, 01), 800.00m, transaction);

            using (new AssertionScope())
            {
                holding.Properties[new Date(2000, 12, 31)].Should().Be(new HoldingProperties(100, 1000.00m, 500.00m));
                holding.Properties[new Date(2001, 01, 01)].Should().Be(new HoldingProperties(100, 1000.00m, 0.00m));

                parcel.Properties[new Date(2000, 12, 31)].Should().Be(new ParcelProperties(100, 1000.00m, 500.00m));
                parcel.Properties[new Date(2001, 01, 01)].Should().Be(new ParcelProperties(100, 1000.00m, 0.00m));

                cgtEvent.Should().BeEquivalentTo(new
                {
                    EventDate = new Date(2001, 01, 01),
                    Stock = stock,
                    AmountReceived = 800.00m,
                    CapitalGain = 300.00m,
                    CgtMethod = CgtMethod.Discount,
                    Transaction = transaction
                });
            }
        }

        [Fact]
        public void ReduceParcelCostBaseCostBaseToZero()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var eventSink = mockRepository.Create<ICgtEventSink>();
            CgtEventArgs cgtEvent = null;
            eventSink.Setup(x => x.OnCgtEventOccured(It.IsAny<Holding>(), It.IsAny<CgtEventArgs>()))
                .Callback<object, CgtEventArgs>((o, e) => cgtEvent = e)
                .Verifiable();

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.CgtEventOccurred += eventSink.Object.OnCgtEventOccured;

            var parcel = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);

            var transaction = new CostBaseAdjustment();
            holding.ReduceParcelCostBase(parcel.Id, new Date(2001, 01, 01), 1200.00m, transaction);

            using (new AssertionScope())
            {
                holding.Properties[new Date(2000, 12, 31)].Should().Be(new HoldingProperties(100, 1000.00m, 1200.00m));
                holding.Properties[new Date(2001, 01, 01)].Should().Be(new HoldingProperties(100, 1000.00m, 0.00m));

                parcel.Properties[new Date(2000, 12, 31)].Should().Be(new ParcelProperties(100, 1000.00m, 1200.00m));
                parcel.Properties[new Date(2001, 01, 01)].Should().Be(new ParcelProperties(100, 1000.00m, 0.00m));
            }
        }

        [Fact]
        public void GetValueNoParcels()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));

            var value = holding.Value(new Date(2001, 01, 01));

            value.Should().Be(0.00m);
        }

        [Fact]
        public void GetValueSingleParcel()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var priceHistory = mockRepository.Create<IStockPriceHistory>();
            priceHistory.Setup(x => x.GetPrice(new Date(2001, 01, 01))).Returns(10.00m).Verifiable();

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);
            stock.SetPriceHistory(priceHistory.Object);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);

            var value = holding.Value(new Date(2001, 01, 01));

            value.Should().Be(1000.00m);
        }

        [Fact]
        public void GetValueMultipleParcels()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var priceHistory = mockRepository.Create<IStockPriceHistory>();
            priceHistory.Setup(x => x.GetPrice(new Date(2001, 01, 01))).Returns(10.00m).Verifiable();

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);
            stock.SetPriceHistory(priceHistory.Object);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            var value = holding.Value(new Date(2001, 01, 01));

            value.Should().Be(3000.00m);
        }

        [Fact]
        public void ChangeDrpParticipation()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));

            holding.ChangeDrpParticipation(true);

            holding.Settings.ParticipateInDrp.Should().BeTrue();
        }

        [Fact]
        public void AddDrpAmount()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddDrpAccountAmount(new Date(2001, 01, 01), 100.00m);
            holding.AddDrpAccountAmount(new Date(2002, 01, 01), 100.00m);

            holding.DrpAccount.Balance().Should().Be(200.00m);
        }

        [Fact]
        public void AddDrpNegativeAmount()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddDrpAccountAmount(new Date(2001, 01, 01), 100.00m);
            holding.AddDrpAccountAmount(new Date(2002, 01, 01), -50.00m);

            holding.DrpAccount.Balance().Should().Be(50.00m);
        }

    }

    public interface ICgtEventSink
    {
        void OnCgtEventOccured(object sender, CgtEventArgs e);
    }
}
