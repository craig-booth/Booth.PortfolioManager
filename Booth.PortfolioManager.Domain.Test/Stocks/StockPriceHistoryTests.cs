using System;
using System.Linq;

using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;


using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Test.Stocks
{
    public class StockPriceHistoryTests
    {

        [Fact]
        public void EarliestDate()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 08, 01), 4.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 10.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 05, 01), 6.00m);

            var result = priceHistory.EarliestDate;

            result.Should().Be(new Date(2000, 01, 01));
        }

        [Fact]
        public void EarliestDateEmptyList()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());

            var result = priceHistory.EarliestDate;

            result.Should().Be(Date.MinValue);
        }

        [Fact]
        public void LatestDate()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 08, 01), 4.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 10.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 05, 01), 6.00m);

            var result = priceHistory.LatestDate;

            result.Should().Be(new Date(2000, 08, 01));
        }

        [Fact]
        public void LatestDateEmptyList()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());

            var result = priceHistory.LatestDate;

            result.Should().Be(Date.MinValue);
        }

        [Fact]
        public void GetPriceEmptyList()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());

            var result = priceHistory.GetPrice(new Date(2000, 01, 01));

            result.Should().Be(0.00m);
        }

        [Fact]
        public void GetPriceExactMatch()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 08, 01), 4.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 10.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 05, 01), 6.00m);

            var result = priceHistory.GetPrice(new Date(2000, 01, 01));

            result.Should().Be(10.00m);
        }

        [Fact]
        public void GetPriceCurrentDay()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 08, 01), 4.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 10.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 05, 01), 6.00m);
            priceHistory.UpdateCurrentPrice(15.00m);

            var result = priceHistory.GetPrice(Date.Today);

            result.Should().Be(15.00m);
        }

        [Fact]
        public void GetPriceCurrentDayWitoutCurrentPriceSet()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 08, 01), 4.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 10.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 05, 01), 6.00m);

            var result = priceHistory.GetPrice(Date.Today);

            result.Should().Be(4.00m);
        }

        [Fact]
        public void GetPriceInTheFuture()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 08, 01), 4.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 10.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 05, 01), 6.00m);
            priceHistory.UpdateCurrentPrice(15.00m);

            var result = priceHistory.GetPrice(Date.Today.AddDays(20));

            result.Should().Be(15.00m);
        }

        [Fact]
        public void GetPriceNoMatch()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 08, 01), 4.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 10.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 05, 01), 6.00m);
            priceHistory.UpdateCurrentPrice(15.00m);

            var result = priceHistory.GetPrice(new Date(2000, 02, 01));

            result.Should().Be(10.00m);
        }

        [Fact]
        public void GetPriceBeforeFirstEntry()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 08, 01), 4.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 10.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 05, 01), 6.00m);
            priceHistory.UpdateCurrentPrice(15.00m);

            var result = priceHistory.GetPrice(new Date(1999, 02, 01));

            result.Should().Be(0.00m);
        }

        [Fact]
        public void GetPricesEmptyList()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());

            var result = priceHistory.GetPrices(new DateRange(new Date(1999, 02, 01), new Date(2000, 06, 01))).ToList();

            result.Should().BeEmpty();
        }

        [Fact]
        public void GetPricesStartAndEndMatches()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 07), 7.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 09), 9.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 11), 11.00m);

            var result = priceHistory.GetPrices(new DateRange(new Date(2000, 01, 03), new Date(2000, 01, 09))).ToList();

            result.Should().Equal(new[]
            {
                new StockPrice(new Date(2000, 01, 03), 3.00m),
                new StockPrice(new Date(2000, 01, 05), 5.00m),
                new StockPrice(new Date(2000, 01, 07), 7.00m),
                new StockPrice(new Date(2000, 01, 09), 9.00m)
            });
        }

        [Fact]
        public void GetPricesStartMatchesAndEndNoMatch()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 07), 7.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 09), 9.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 11), 11.00m);

            var result = priceHistory.GetPrices(new DateRange(new Date(2000, 01, 03), new Date(2000, 01, 08))).ToList();

            result.Should().Equal(new[]
            {
                new StockPrice(new Date(2000, 01, 03), 3.00m),
                new StockPrice(new Date(2000, 01, 05), 5.00m),
                new StockPrice(new Date(2000, 01, 07), 7.00m),
            });
        }

        [Fact]
        public void GetPricesStartNoMatchAndEndMatches()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 07), 7.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 09), 9.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 11), 11.00m);

            var result = priceHistory.GetPrices(new DateRange(new Date(2000, 01, 04), new Date(2000, 01, 09))).ToList();

            result.Should().Equal(new[]
            {
                new StockPrice(new Date(2000, 01, 05), 5.00m),
                new StockPrice(new Date(2000, 01, 07), 7.00m),
                new StockPrice(new Date(2000, 01, 09), 9.00m)
            });
        }

        [Fact]
        public void GetPricesStartBeforeFirstEntryAndEndMatches()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 07), 7.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 09), 9.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 11), 11.00m);

            var result = priceHistory.GetPrices(new DateRange(new Date(1999, 01, 03), new Date(2000, 01, 05))).ToList();

            result.Should().Equal(new[]
            {
                new StockPrice(new Date(2000, 01, 01), 1.00m),
                new StockPrice(new Date(2000, 01, 03), 3.00m),
                new StockPrice(new Date(2000, 01, 05), 5.00m)
            });
        }

        [Fact]
        public void GetPricesStartAndEndDontMatch()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 07), 7.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 09), 9.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 11), 11.00m);

            var result = priceHistory.GetPrices(new DateRange(new Date(2000, 01, 02), new Date(2000, 01, 10))).ToList();

            result.Should().Equal(new[]
            {
                new StockPrice(new Date(2000, 01, 03), 3.00m),
                new StockPrice(new Date(2000, 01, 05), 5.00m),
                new StockPrice(new Date(2000, 01, 07), 7.00m),
                new StockPrice(new Date(2000, 01, 09), 9.00m)
            });
        }

        [Fact]
        public void GetPricesWhollyBefore()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 07), 7.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 09), 9.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 11), 11.00m);

            var result = priceHistory.GetPrices(new DateRange(new Date(1999, 01, 01), new Date(1999, 01, 10))).ToList();

            result.Should().BeEmpty();
        }

        [Fact]
        public void GetPricesWhollyAfter()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 07), 7.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 09), 9.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 11), 11.00m);

            var result = priceHistory.GetPrices(new DateRange(new Date(2001, 01, 01), new Date(2001, 01, 10))).ToList();

            result.Should().BeEmpty();
        }

        [Fact]
        public void UpdateCurrentPriceExistingEntryForDate()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);
            priceHistory.UpdateCurrentPrice(10.00m);

            priceHistory.UpdateCurrentPrice(11.00m);

            var result = priceHistory.GetPrice(Date.Today);

            result.Should().Be(11.00m);
        }

        [Fact]
        public void UpdateCurrentPriceExistingClosingPriceForDate()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);
            priceHistory.UpdateClosingPrice(Date.Today, 10.00m);

            priceHistory.UpdateCurrentPrice(11.00m);

            var result = priceHistory.GetPrice(Date.Today);

            result.Should().Be(11.00m);
        }

        [Fact]
        public void UpdateCurrentPriceNoExistingEntryForDate()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);

            priceHistory.UpdateCurrentPrice(11.00m);

            var result = priceHistory.GetPrice(Date.Today);

            result.Should().Be(11.00m);
        }

        [Fact]
        public void UpdateClosingPriceExistingEntryForDate()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);

            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 11.00m);

            var result = priceHistory.GetPrice(new Date(2000, 01, 03));

            result.Should().Be(11.00m);
        }

        [Fact]
        public void UpdateClosingPriceCurrentPriceForDate()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);
            priceHistory.UpdateCurrentPrice(10.00m);

            priceHistory.UpdateClosingPrice(Date.Today, 11.00m);

            var result = priceHistory.GetPrice(Date.Today);

            result.Should().Be(11.00m);
        }

        [Fact]
        public void UpdateClosingPriceNoExistingEntryForDate()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);

            priceHistory.UpdateClosingPrice(new Date(2000, 01, 04), 11.00m);

            var result = priceHistory.GetPrice(new Date(2000, 01, 04));

            result.Should().Be(11.00m);
        }

        [Fact]
        public void UpdateClosingPricesEmptyList()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());

            priceHistory.UpdateClosingPrice(new Date(2000, 01, 04), 11.00m);

            var result = priceHistory.GetPrice(new Date(2000, 01, 04));

            result.Should().Be(11.00m);
        }

        [Fact]
        public void UpdateClosingPricesNewEntries()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);

            var prices = new StockPrice[]
            {
                new StockPrice(new Date(2000, 01, 02), 2.00m),
                new StockPrice(new Date(2000, 01, 04), 4.00m)
            };
            priceHistory.UpdateClosingPrices(prices);

            var result = priceHistory.GetPrices(new DateRange(Date.MinValue, Date.MaxValue)).ToList();

            result.Should().Equal(new[]
            {
                new StockPrice(new Date(2000, 01, 01), 1.00m),
                new StockPrice(new Date(2000, 01, 02), 2.00m),
                new StockPrice(new Date(2000, 01, 03), 3.00m),
                new StockPrice(new Date(2000, 01, 04), 4.00m),
                new StockPrice(new Date(2000, 01, 05), 5.00m)
            });
        }

        [Fact]
        public void UpdateClosingPricesUpdateExistingEntries()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);

            var prices = new StockPrice[]
            {
                new StockPrice(new Date(2000, 01, 03), 10.00m),
                new StockPrice(new Date(2000, 01, 05), 20.00m)
            };
            priceHistory.UpdateClosingPrices(prices);

            var result = priceHistory.GetPrices(new DateRange(Date.MinValue, Date.MaxValue)).ToList();

            result.Should().Equal(new[]
            {
                new StockPrice(new Date(2000, 01, 01), 1.00m),
                new StockPrice(new Date(2000, 01, 03), 10.00m),
                new StockPrice(new Date(2000, 01, 05), 20.00m)
            });
        }

    }
}
