using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;


namespace Booth.PortfolioManager.Domain.Test.Portfolios
{
    class HoldingTests
    {

        [TestCase]
        public void AccessParcelsByDateNoParcels()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));

            Assert.That(holding[new Date(2000, 01, 01)].ToList(), Is.Empty);
        }

        [TestCase]
        public void AccessParcelsByDateParcelsExistButNotAtDate()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            Assert.That(holding[new Date(1999, 01, 01)].ToList(), Is.Empty);
        }

        [TestCase]
        public void AccessParcelsByDateParcelsExistAtDate()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            Assert.That(holding[new Date(2002, 01, 01)].ToList(), Has.Count.EqualTo(2));
        }

        [TestCase]
        public void GetParcelsNoParcels()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));

            Assert.That(holding.Parcels().ToList(), Is.Empty);
        }

        [TestCase]
        public void GetParcelsByDateParcelsExiste()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            Assert.That(holding.Parcels().ToList(), Has.Count.EqualTo(2));
        }

        [TestCase]
        public void GetParcelsByDateNoParcels()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));

            Assert.That(holding.Parcels(new Date(2000, 01, 01)).ToList(), Is.Empty);
        }

        [TestCase]
        public void GetParcelsByDateParcelsExistButNotAtDate()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            Assert.That(holding.Parcels(new Date(1999, 01, 01)).ToList(), Is.Empty);
        }

        [TestCase]
        public void GetParcelsByDateParcelsExistAtDate()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            Assert.That(holding.Parcels(new Date(2002, 01, 01)).ToList(), Has.Count.EqualTo(2));
        }

        [TestCase]
        public void GetParcelsByDateRangeNoParcels()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));

            Assert.That(holding.Parcels(new DateRange(Date.MinValue, Date.MaxValue)).ToList(), Is.Empty);
        }

        [TestCase]
        public void GetParcelsByDateRangeParcelsExistButNotInRange()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            Assert.That(holding.Parcels(new DateRange(new Date(1999, 01, 01), new Date(1999, 12, 31))).ToList(), Is.Empty);
        }

        [TestCase]
        public void GetParcelsByDateParcelsExistInRange()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            Assert.That(holding.Parcels(new DateRange(new Date(2001, 01, 01), Date.MaxValue)).ToList(), Has.Count.EqualTo(2));
        }

        [TestCase]
        public void AddParcelNoExistingHoldings()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);

            Assert.Multiple(() =>
            {
                var properties = holding.Properties[new Date(2000, 01, 01)];
                Assert.That(properties, Is.EqualTo(new HoldingProperties(100, 1000.00m, 1200.00m)));

                Assert.That(holding.Settings.ParticipateInDrp, Is.EqualTo(false));
            });
        }

        [TestCase]
        public void AddParcelExistingHoldings()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            var properties = holding.Properties[new Date(2001, 01, 01)];
            Assert.That(properties, Is.EqualTo(new HoldingProperties(300, 3000.00m, 3400.00m)));
        }

        [TestCase]
        public void DisposeOfParcelNotInHoldings()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);

            var parcel = new Parcel(Guid.NewGuid(), new Date(2000, 01, 01), new Date(2000, 01, 01), new ParcelProperties(100, 1000.00m, 1200.00m), null);

            Assert.That(() => holding.DisposeOfParcel(parcel, new Date(2001, 01, 01), 100, 1000.00m, null), Throws.TypeOf(typeof(ArgumentException)));
        }

        [TestCase]
        public void DisposeOfParcelPartialSale()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            var parcel = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 200, 2000.00m, 2200.00m, null);

            holding.DisposeOfParcel(parcel, new Date(2001, 01, 01), 99, 500.00m, null);

            Assert.Multiple(() =>
            {
                var holdingProperties = holding.Properties[new Date(2000, 12, 31)];
                Assert.That(holdingProperties, Is.EqualTo(new HoldingProperties(300, 3000.00m, 3400.00m)));

                holdingProperties = holding.Properties[new Date(2001, 01, 01)];
                Assert.That(holdingProperties, Is.EqualTo(new HoldingProperties(201, 2010.00m, 2212.00m)));

                var parcelProperties = parcel.Properties[new Date(2000, 12, 31)];
                Assert.That(parcelProperties, Is.EqualTo(new ParcelProperties(100, 1000.00m, 1200.00m)));

                parcelProperties = parcel.Properties[new Date(2001, 01, 01)];
                Assert.That(parcelProperties, Is.EqualTo(new ParcelProperties(1, 10.00m, 12.00m)));
            });
        }

        [TestCase]
        public void DisposeOfParcelFullSale()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            var parcel = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            holding.DisposeOfParcel(parcel, new Date(2002, 01, 01), 100, 500.00m, null);

            Assert.Multiple(() =>
            {
                var holdingProperties = holding.Properties[new Date(2001, 12, 31)];
                Assert.That(holdingProperties, Is.EqualTo(new HoldingProperties(300, 3000.00m, 3400.00m)));

                holdingProperties = holding.Properties[new Date(2002, 01, 01)];
                Assert.That(holdingProperties, Is.EqualTo(new HoldingProperties(200, 2000.00m, 2200.00m)));

                var parcelProperties = parcel.Properties[new Date(2001, 12, 31)];
                Assert.That(parcelProperties, Is.EqualTo(new ParcelProperties(100, 1000.00m, 1200.00m)));

                Assert.That(parcel.EffectivePeriod.ToDate, Is.EqualTo(new Date(2002, 01, 01)));
            });
        }

        [TestCase]
        public void DisposeOfParcelMoreUnitsThanInParcel()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            var parcel = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);

            Assert.That(() => holding.DisposeOfParcel(parcel, new Date(2001, 01, 01), 200, 1000.00m, null), Throws.TypeOf(typeof(NotEnoughSharesForDisposal)));
        }

        [TestCase]
        public void DisposeOfLastParcel()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            var parcel = holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);

            holding.DisposeOfParcel(parcel, new Date(2001, 01, 01), 100, 500.00m, null);

            Assert.Multiple(() =>
            {
                var holdingProperties = holding.Properties[new Date(2000, 12, 31)];
                Assert.That(holdingProperties, Is.EqualTo(new HoldingProperties(100, 1000.00m, 1200.00m)));

                holdingProperties = holding.Properties[new Date(2001, 01, 01)];
                Assert.That(holdingProperties, Is.EqualTo(new HoldingProperties(0, 0.00m, 0.00m)));

                Assert.That(holding.EffectivePeriod.ToDate, Is.EqualTo(new Date(2001, 01, 01)));
            });

        }

        [TestCase]
        public void GetValueNoParcels()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));

            var value = holding.Value(new Date(2001, 01, 01));

            Assert.That(value, Is.EqualTo(0.00m));
        }

        [TestCase]
        public void GetValueSingleParcel()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var priceHistory = mockRepository.Create<IStockPriceHistory>();
            priceHistory.Setup(x => x.GetPrice(new Date(2001, 01, 01))).Returns(10.00m).Verifiable();

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);
            stock.SetPriceHistory(priceHistory.Object);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);

            var value = holding.Value(new Date(2001, 01, 01));

            Assert.That(value, Is.EqualTo(1000.00m));
        }

        [TestCase]
        public void GetValueMultipleParcels()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var priceHistory = mockRepository.Create<IStockPriceHistory>();
            priceHistory.Setup(x => x.GetPrice(new Date(2001, 01, 01))).Returns(10.00m).Verifiable();

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);
            stock.SetPriceHistory(priceHistory.Object);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddParcel(new Date(2000, 01, 01), new Date(2000, 01, 01), 100, 1000.00m, 1200.00m, null);
            holding.AddParcel(new Date(2001, 01, 01), new Date(2001, 01, 01), 200, 2000.00m, 2200.00m, null);

            var value = holding.Value(new Date(2001, 01, 01));

            Assert.That(value, Is.EqualTo(3000.00m));
        }

        [TestCase]
        public void ChangeDrpParticipation()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));

            holding.ChangeDrpParticipation(true);

            Assert.That(holding.Settings.ParticipateInDrp, Is.EqualTo(true));
        }

        [TestCase]
        public void AddDrpAmount()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddDrpAccountAmount(new Date(2001, 01, 01), 100.00m);
            holding.AddDrpAccountAmount(new Date(2002, 01, 01), 100.00m);

            Assert.That(holding.DrpAccount.Balance, Is.EqualTo(200.00m));
        }

        [TestCase]
        public void AddDrpNegativeAmount()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holding = new Holding(stock, new Date(2000, 01, 01));
            holding.AddDrpAccountAmount(new Date(2001, 01, 01), 100.00m);
            holding.AddDrpAccountAmount(new Date(2002, 01, 01), -50.00m);

            Assert.That(holding.DrpAccount.Balance, Is.EqualTo(50.00m));
        }
    }
}
