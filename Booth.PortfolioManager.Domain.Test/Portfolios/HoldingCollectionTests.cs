using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Test.Portfolios
{
    class HoldingCollectionTests
    {

        [TestCase]
        public void AccessHoldingsByStockNoEntries()
        {
            var holdings = new HoldingCollection();

            Assert.That(() => holdings[Guid.NewGuid()], Is.Null);
        }

        [TestCase]
        public void AccessHoldingsByStockSingleEntry()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock, new Date(2000, 01, 01));

            var holding = holdings[stock.Id];
            Assert.Multiple(() =>
            {
                Assert.That(holding.Stock, Is.EqualTo(stock));
                Assert.That(holding.EffectivePeriod, Is.EqualTo(new DateRange(new Date(2000, 01, 01), Date.MaxValue)));
                Assert.That(holding.Properties[new Date(2000, 01, 01)], Is.EqualTo(new HoldingProperties(0, 0.00m, 0.00m)));
            });
            
        }

        [TestCase]
        public void AccessHoldingsByStockMultipleEntries()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));
            holdings.Add(stock2, new Date(2000, 01, 01));

            var holding = holdings[stock1.Id];
            Assert.Multiple(() =>
            {
                Assert.That(holding.Stock, Is.EqualTo(stock1));
                Assert.That(holding.EffectivePeriod, Is.EqualTo(new DateRange(new Date(2000, 01, 01), Date.MaxValue)));
                Assert.That(holding.Properties[new Date(2000, 01, 01)], Is.EqualTo(new HoldingProperties(0, 0.00m, 0.00m)));
            });
        }

        [TestCase]
        public void AllHoldingsNoEntries()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();

            Assert.That(holdings.All().ToList(), Is.Empty);
        }

        [TestCase]
        public void AllHoldingsSingleEntry()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));

            Assert.That(holdings.All().ToList(), Has.Count.EqualTo(1));
        }

        [TestCase]
        public void AllHoldingsMultipleEntries()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));
            holdings.Add(stock2, new Date(2000, 01, 01));

            Assert.That(holdings.All().ToList(), Has.Count.EqualTo(2));
        }

        [TestCase]
        public void AllHoldingsByDateNoEntries()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();

            Assert.That(holdings.All(new Date(2000, 01, 01)).ToList(), Is.Empty);
        }

        [TestCase]
        public void AllHoldingsByDateNoEntriesAtDate()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));
            holdings.Add(stock2, new Date(2002, 01, 01));

            Assert.That(holdings.All(new Date(1999, 01, 01)).ToList(), Is.Empty);
        }

        [TestCase]
        public void AllHoldingsByDateEntriesAtDate()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));
            holdings.Add(stock2, new Date(2002, 01, 01));

            Assert.That(holdings.All(new Date(2003, 01, 01)).ToList(), Has.Count.EqualTo(2));
        }

        [TestCase]
        public void AllHoldingsByDateSomeEntriesAtDate()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));
            holdings.Add(stock2, new Date(2002, 01, 01));

            Assert.That(holdings.All(new Date(2001, 01, 01)).ToList(), Has.Count.EqualTo(1));
        }

        [TestCase]
        public void AllHoldingsByDateRangeNoEntries()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();

            Assert.That(holdings.All(new DateRange(new Date(2000, 01, 01), new Date(2005, 01, 01))).ToList(), Is.Empty);
        }

        [TestCase]
        public void AllHoldingsByDateRangeNoEntriesInRange()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));
            holdings.Add(stock2, new Date(2002, 01, 01));

            Assert.That(holdings.All(new DateRange(new Date(1999, 01, 01), new Date(1999, 12, 01))).ToList(), Is.Empty);
        }

        [TestCase]
        public void AllHoldingsByDateRangeEntriesInRange()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));
            holdings.Add(stock2, new Date(2002, 01, 01));

            Assert.That(holdings.All(new DateRange(new Date(2000, 01, 01), new Date(2005, 12, 01))).ToList(), Has.Count.EqualTo(2));
        }

        [TestCase]
        public void AllHoldingsByDateRangeSomeEntriesInRange()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));
            holdings.Add(stock2, new Date(2002, 01, 01));

            Assert.That(holdings.All(new DateRange(new Date(2000, 01, 01), new Date(2001, 12, 01))).ToList(), Has.Count.EqualTo(1));
        }

        [TestCase]
        public void GetHoldingsByStockNoEntries()
        {
            var holdings = new HoldingCollection();

            Assert.That(() => holdings.Get(Guid.NewGuid()), Is.Null);
        }

        [TestCase]
        public void GetHoldingsByStockSingleEntry()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));

            var holding = holdings.Get(stock1.Id);
            Assert.Multiple(() =>
            {
                Assert.That(holding.Stock, Is.EqualTo(stock1));
                Assert.That(holding.EffectivePeriod, Is.EqualTo(new DateRange(new Date(2000, 01, 01), Date.MaxValue)));
                Assert.That(holding.Properties[new Date(2000, 01, 01)], Is.EqualTo(new HoldingProperties(0, 0.00m, 0.00m)));
            });
        }

        [TestCase]
        public void GetHoldingsByStockMultipleEntries()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));
            holdings.Add(stock2, new Date(2000, 01, 01));

            var holding = holdings.Get(stock1.Id);
            Assert.Multiple(() =>
            {
                Assert.That(holding.Stock, Is.EqualTo(stock1));
                Assert.That(holding.EffectivePeriod, Is.EqualTo(new DateRange(new Date(2000, 01, 01), Date.MaxValue)));
                Assert.That(holding.Properties[new Date(2000, 01, 01)], Is.EqualTo(new HoldingProperties(0, 0.00m, 0.00m)));
            });
        }

        [TestCase]
        public void AddHoldingNewStock()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));

            var holding = holdings[stock1.Id];
            Assert.Multiple(() =>
            {
                Assert.That(holding.Stock, Is.EqualTo(stock1));
                Assert.That(holding.EffectivePeriod, Is.EqualTo(new DateRange(new Date(2000, 01, 01), Date.MaxValue)));
                Assert.That(holding.Properties[new Date(2000, 01, 01)], Is.EqualTo(new HoldingProperties(0, 0.00m, 0.00m)));
            });
        }

        [TestCase]
        public void AddHoldingExisingStock()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));

            Assert.That(() => holdings.Add(stock1, new Date(2001, 01, 01)), Throws.TypeOf(typeof(ArgumentException)));
        }

    }
}
