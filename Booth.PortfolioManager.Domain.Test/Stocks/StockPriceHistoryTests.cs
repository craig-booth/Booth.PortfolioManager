using System;
using System.Linq;

using NUnit.Framework;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Test.Stocks
{
    class StockPriceHistoryTests
    {

        [TestCase]
        public void EarliestDate()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 08, 01), 4.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 10.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 05, 01), 6.00m);

            var result = priceHistory.EarliestDate;

            Assert.That(result, Is.EqualTo(new Date(2000, 01, 01)));
        }

        [TestCase]
        public void EarliestDateEmptyList()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());

            var result = priceHistory.EarliestDate;

            Assert.That(result, Is.EqualTo(Date.MinValue));
        }

        [TestCase]
        public void LatestDate()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 08, 01), 4.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 10.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 05, 01), 6.00m);

            var result = priceHistory.LatestDate;

            Assert.That(result, Is.EqualTo(new Date(2000, 08, 01)));
        }

        [TestCase]
        public void LatestDateEmptyList()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());

            var result = priceHistory.LatestDate;

            Assert.That(result, Is.EqualTo(Date.MinValue));
        }

        [TestCase]
        public void GetPriceEmptyList()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());

            var result = priceHistory.GetPrice(new Date(2000, 01, 01));

            Assert.That(result, Is.EqualTo(0.00m));
        }

        [TestCase]
        public void GetPriceExactMatch()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 08, 01), 4.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 10.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 05, 01), 6.00m);

            var result = priceHistory.GetPrice(new Date(2000, 01, 01));

            Assert.That(result, Is.EqualTo(10.00m));
        }

        [TestCase]
        public void GetPriceCurrentDay()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 08, 01), 4.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 10.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 05, 01), 6.00m);
            priceHistory.UpdateCurrentPrice(15.00m);

            var result = priceHistory.GetPrice(Date.Today);

            Assert.That(result, Is.EqualTo(15.00m));
        }

        [TestCase]
        public void GetPriceCurrentDayWitoutCurrentPriceSet()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 08, 01), 4.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 10.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 05, 01), 6.00m);

            var result = priceHistory.GetPrice(Date.Today);

            Assert.That(result, Is.EqualTo(4.00m));
        }

        [TestCase]
        public void GetPriceInTheFuture()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 08, 01), 4.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 10.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 05, 01), 6.00m);
            priceHistory.UpdateCurrentPrice(15.00m);

            var result = priceHistory.GetPrice(Date.Today.AddDays(20));

            Assert.That(result, Is.EqualTo(15.00m));
        }

        [TestCase]
        public void GetPriceNoMatch()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 08, 01), 4.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 10.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 05, 01), 6.00m);
            priceHistory.UpdateCurrentPrice(15.00m);

            var result = priceHistory.GetPrice(new Date(2000, 02, 01));

            Assert.That(result, Is.EqualTo(10.00m));
        }

        [TestCase]
        public void GetPriceBeforeFirstEntry()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 08, 01), 4.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 10.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 05, 01), 6.00m);
            priceHistory.UpdateCurrentPrice(15.00m);

            var result = priceHistory.GetPrice(new Date(1999, 02, 01));

            Assert.That(result, Is.EqualTo(0.00m));
        }

        [TestCase]
        public void GetPricesEmptyList()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());

            var result = priceHistory.GetPrices(new DateRange(new Date(1999, 02, 01), new Date(2000, 06, 01))).ToList();

            Assert.That(result, Is.Empty);
        }

        [TestCase]
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

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(4));

                Assert.That(result[0].Date, Is.EqualTo(new Date(2000, 01, 03)));
                Assert.That(result[0].Price, Is.EqualTo(3.00m));

                Assert.That(result[1].Date, Is.EqualTo(new Date(2000, 01, 05)));
                Assert.That(result[1].Price, Is.EqualTo(5.00m));

                Assert.That(result[2].Date, Is.EqualTo(new Date(2000, 01, 07)));
                Assert.That(result[2].Price, Is.EqualTo(7.00m));

                Assert.That(result[3].Date, Is.EqualTo(new Date(2000, 01, 09)));
                Assert.That(result[3].Price, Is.EqualTo(9.00m));
            });
        }

        [TestCase]
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

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(3));

                Assert.That(result[0].Date, Is.EqualTo(new Date(2000, 01, 03)));
                Assert.That(result[0].Price, Is.EqualTo(3.00m));

                Assert.That(result[1].Date, Is.EqualTo(new Date(2000, 01, 05)));
                Assert.That(result[1].Price, Is.EqualTo(5.00m));

                Assert.That(result[2].Date, Is.EqualTo(new Date(2000, 01, 07)));
                Assert.That(result[2].Price, Is.EqualTo(7.00m));
            });
        }

        [TestCase]
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

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(3));

                Assert.That(result[0].Date, Is.EqualTo(new Date(2000, 01, 05)));
                Assert.That(result[0].Price, Is.EqualTo(5.00m));

                Assert.That(result[1].Date, Is.EqualTo(new Date(2000, 01, 07)));
                Assert.That(result[1].Price, Is.EqualTo(7.00m));

                Assert.That(result[2].Date, Is.EqualTo(new Date(2000, 01, 09)));
                Assert.That(result[2].Price, Is.EqualTo(9.00m));
            });
        }

        [TestCase]
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

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(3));

                Assert.That(result[0].Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(result[0].Price, Is.EqualTo(1.00m));

                Assert.That(result[1].Date, Is.EqualTo(new Date(2000, 01, 03)));
                Assert.That(result[1].Price, Is.EqualTo(3.00m));

                Assert.That(result[2].Date, Is.EqualTo(new Date(2000, 01, 05)));
                Assert.That(result[2].Price, Is.EqualTo(5.00m));
            });
        }

        [TestCase]
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

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(4));

                Assert.That(result[0].Date, Is.EqualTo(new Date(2000, 01, 03)));
                Assert.That(result[0].Price, Is.EqualTo(3.00m));

                Assert.That(result[1].Date, Is.EqualTo(new Date(2000, 01, 05)));
                Assert.That(result[1].Price, Is.EqualTo(5.00m));

                Assert.That(result[2].Date, Is.EqualTo(new Date(2000, 01, 07)));
                Assert.That(result[2].Price, Is.EqualTo(7.00m));

                Assert.That(result[3].Date, Is.EqualTo(new Date(2000, 01, 09)));
                Assert.That(result[3].Price, Is.EqualTo(9.00m));
            });
        }

        [TestCase]
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

            Assert.That(result, Is.Empty);
        }

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

            Assert.That(result, Is.Empty);
        }

        [TestCase]
        public void UpdateCurrentPriceExistingEntryForDate()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);
            priceHistory.UpdateCurrentPrice(10.00m);

            priceHistory.UpdateCurrentPrice(11.00m);

            var result = priceHistory.GetPrice(Date.Today);

            Assert.That(result, Is.EqualTo(11.00m));
        }

        [TestCase]
        public void UpdateCurrentPriceExistingClosingPriceForDate()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);
            priceHistory.UpdateClosingPrice(Date.Today, 10.00m);

            priceHistory.UpdateCurrentPrice(11.00m);

            var result = priceHistory.GetPrice(Date.Today);

            Assert.That(result, Is.EqualTo(11.00m));
        }

        [TestCase]
        public void UpdateCurrentPriceNoExistingEntryForDate()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);

            priceHistory.UpdateCurrentPrice(11.00m);

            var result = priceHistory.GetPrice(Date.Today);

            Assert.That(result, Is.EqualTo(11.00m));
        }

        [TestCase]
        public void UpdateClosingPriceExistingEntryForDate()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);

            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 11.00m);

            var result = priceHistory.GetPrice(new Date(2000, 01, 03));

            Assert.That(result, Is.EqualTo(11.00m));
        }

        [TestCase]
        public void UpdateClosingPriceCurrentPriceForDate()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);
            priceHistory.UpdateCurrentPrice(10.00m);

            priceHistory.UpdateClosingPrice(Date.Today, 11.00m);

            var result = priceHistory.GetPrice(Date.Today);

            Assert.That(result, Is.EqualTo(11.00m));
        }

        [TestCase]
        public void UpdateClosingPriceNoExistingEntryForDate()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);

            priceHistory.UpdateClosingPrice(new Date(2000, 01, 04), 11.00m);

            var result = priceHistory.GetPrice(new Date(2000, 01, 04));

            Assert.That(result, Is.EqualTo(11.00m));
        }

        [TestCase]
        public void UpdateClosingPricesEmptyList()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());

            priceHistory.UpdateClosingPrice(new Date(2000, 01, 04), 11.00m);

            var result = priceHistory.GetPrice(new Date(2000, 01, 04));

            Assert.That(result, Is.EqualTo(11.00m));
        }

        [TestCase]
        public void UpdateClosingPricesNewEntries()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);

            var prices = new Tuple<Date, decimal>[]
            {
                new Tuple<Date, decimal>(new Date(2000, 01, 02), 2.00m),
                new Tuple<Date, decimal>(new Date(2000, 01, 04), 4.00m)
            };
            priceHistory.UpdateClosingPrices(prices);

            var result = priceHistory.GetPrices(new DateRange(Date.MinValue, Date.MaxValue)).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(5));

                Assert.That(result[0].Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(result[0].Price, Is.EqualTo(1.00m));

                Assert.That(result[1].Date, Is.EqualTo(new Date(2000, 01, 02)));
                Assert.That(result[1].Price, Is.EqualTo(2.00m));

                Assert.That(result[2].Date, Is.EqualTo(new Date(2000, 01, 03)));
                Assert.That(result[2].Price, Is.EqualTo(3.00m));

                Assert.That(result[3].Date, Is.EqualTo(new Date(2000, 01, 04)));
                Assert.That(result[3].Price, Is.EqualTo(4.00m));

                Assert.That(result[4].Date, Is.EqualTo(new Date(2000, 01, 05)));
                Assert.That(result[4].Price, Is.EqualTo(5.00m));
            });
        }

        [TestCase]
        public void UpdateClosingPricesUpdateExistingEntries()
        {
            var priceHistory = new StockPriceHistory(Guid.NewGuid());
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 03), 3.00m);
            priceHistory.UpdateClosingPrice(new Date(2000, 01, 05), 5.00m);

            var prices = new Tuple<Date, decimal>[]
            {
                new Tuple<Date, decimal>(new Date(2000, 01, 03), 10.00m),
                new Tuple<Date, decimal>(new Date(2000, 01, 05), 20.00m)
            };
            priceHistory.UpdateClosingPrices(prices);

            var result = priceHistory.GetPrices(new DateRange(Date.MinValue, Date.MaxValue)).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(3));

                Assert.That(result[0].Date, Is.EqualTo(new Date(2000, 01, 01)));
                Assert.That(result[0].Price, Is.EqualTo(1.00m));

                Assert.That(result[1].Date, Is.EqualTo(new Date(2000, 01, 03)));
                Assert.That(result[1].Price, Is.EqualTo(10.00m));

                Assert.That(result[2].Date, Is.EqualTo(new Date(2000, 01, 05)));
                Assert.That(result[2].Price, Is.EqualTo(20.00m));
            });
        }

    }
}
