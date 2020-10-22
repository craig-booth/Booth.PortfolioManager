using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Test.Portfolios
{
    public class HoldingCollectionTests
    {

        [Fact]
        public void AccessHoldingsByStockNoEntries()
        {
            var holdings = new HoldingCollection();

            holdings[Guid.NewGuid()].Should().BeNull();
        }

        [Fact]
        public void AccessHoldingsByStockSingleEntry()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock, new Date(2000, 01, 01));

            var holding = holdings[stock.Id];

            using (new AssertionScope())
            {
                holding.Should().BeEquivalentTo(new
                {
                    Stock = stock,
                    EffectivePeriod = new DateRange(new Date(2000, 01, 01), Date.MaxValue),
                });

                holding[new Date(2000, 01, 01)].Should().Be(new HoldingProperties(0, 0.00m, 0.00m));
            }
     
        }

        [Fact]
        public void AccessHoldingsByStockMultipleEntries()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));
            holdings.Add(stock2, new Date(2000, 01, 01));

            var holding = holdings[stock1.Id];
            using (new AssertionScope())
            {
                holding.Should().BeEquivalentTo(new
                {
                    Stock = stock1,
                    EffectivePeriod = new DateRange(new Date(2000, 01, 01), Date.MaxValue),
                });

                holding[new Date(2000, 01, 01)].Should().Be(new HoldingProperties(0, 0.00m, 0.00m));
            }
  
        }

        [Fact]
        public void AllHoldingsNoEntries()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();

            holdings.All().Should().BeEmpty();
        }

        [Fact]
        public void AllHoldingsSingleEntry()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));

            holdings.All().Should().HaveCount(1);
        }

        [Fact]
        public void AllHoldingsMultipleEntries()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));
            holdings.Add(stock2, new Date(2000, 01, 01));

            holdings.All().Should().HaveCount(2);
        }

        [Fact]
        public void AllHoldingsByDateNoEntries()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();

            holdings.All(new Date(2000, 01, 01)).Should().BeEmpty();
        }

        [Fact]
        public void AllHoldingsByDateNoEntriesAtDate()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));
            holdings.Add(stock2, new Date(2002, 01, 01));

            holdings.All(new Date(1999, 01, 01)).Should().BeEmpty();
        }

        [Fact]
        public void AllHoldingsByDateEntriesAtDate()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));
            holdings.Add(stock2, new Date(2002, 01, 01));

            holdings.All(new Date(2003, 01, 01)).Should().HaveCount(2);
        }

        [Fact]
        public void AllHoldingsByDateSomeEntriesAtDate()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));
            holdings.Add(stock2, new Date(2002, 01, 01));

            holdings.All(new Date(2001, 01, 01)).Should().HaveCount(1);
        }

        [Fact]
        public void AllHoldingsByDateRangeNoEntries()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();

            holdings.All(new DateRange(new Date(2000, 01, 01), new Date(2005, 01, 01))).Should().BeEmpty();
        }

        [Fact]
        public void AllHoldingsByDateRangeNoEntriesInRange()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));
            holdings.Add(stock2, new Date(2002, 01, 01));

            holdings.All(new DateRange(new Date(1999, 01, 01), new Date(1999, 12, 01))).Should().BeEmpty();
        }

        [Fact]
        public void AllHoldingsByDateRangeEntriesInRange()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));
            holdings.Add(stock2, new Date(2002, 01, 01));

            holdings.All(new DateRange(new Date(2000, 01, 01), new Date(2005, 12, 01))).Should().HaveCount(2);
        }

        [Fact]
        public void AllHoldingsByDateRangeSomeEntriesInRange()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));
            holdings.Add(stock2, new Date(2002, 01, 01));

            holdings.All(new DateRange(new Date(2000, 01, 01), new Date(2001, 12, 01))).Should().HaveCount(1);
        }

        [Fact]
        public void GetHoldingsByStockNoEntries()
        {
            var holdings = new HoldingCollection();

            holdings.Get(Guid.NewGuid()).Should().BeNull();
        }

        [Fact]
        public void GetHoldingsByStockSingleEntry()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));

            var holding = holdings.Get(stock1.Id);
            using (new AssertionScope())
            {
                holding.Should().BeEquivalentTo(new
                {
                    Stock = stock1,
                    EffectivePeriod = new DateRange(new Date(2000, 01, 01), Date.MaxValue),
                });

                holding.Properties[new Date(2000, 01, 01)].Should().Be(new HoldingProperties(0, 0.00m, 0.00m));
            }
        }

        [Fact]
        public void GetHoldingsByStockMultipleEntries()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));
            holdings.Add(stock2, new Date(2000, 01, 01));

            var holding = holdings.Get(stock1.Id);
            using (new AssertionScope())
            {
                holding.Should().BeEquivalentTo(new
                {
                    Stock = stock1,
                    EffectivePeriod = new DateRange(new Date(2000, 01, 01), Date.MaxValue),
                });

                holding.Properties[new Date(2000, 01, 01)].Should().Be(new HoldingProperties(0, 0.00m, 0.00m));
            }
        }

        [Fact]
        public void AddHoldingNewStock()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));

            var holding = holdings[stock1.Id];
            using (new AssertionScope())
            {
                holding.Should().BeEquivalentTo(new
                {
                    Stock = stock1,
                    EffectivePeriod = new DateRange(new Date(2000, 01, 01), Date.MaxValue),
                });

                holding.Properties[new Date(2000, 01, 01)].Should().Be(new HoldingProperties(0, 0.00m, 0.00m));
            }
        }

        [Fact]
        public void AddHoldingExisingStock()
        {
            var stock1 = new Stock(Guid.NewGuid());
            stock1.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "ZYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var holdings = new HoldingCollection();
            holdings.Add(stock1, new Date(2000, 01, 01));

            Action a = () => holdings.Add(stock1, new Date(2001, 01, 01));
            
            a.Should().Throw<ArgumentException>();
        }

    }
}
