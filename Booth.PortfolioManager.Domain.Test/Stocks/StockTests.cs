using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Test.Stocks
{
    class StockTests
    {
        [TestCase]
        public void ToStringNoProperties()
        {
            var stock = new Stock(Guid.NewGuid());

            Assert.That(stock.ToString(), Is.EqualTo(" - "));
        }

        [TestCase]
        public void ToStringBeforeStartDate()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MaxValue, false, AssetCategory.AustralianStocks);

            Assert.That(stock.ToString(), Is.EqualTo("ABC - ABC Pty Ltd"));
        }

        [TestCase]
        public void ToStringAfterEndDate()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);
            stock.DeList(new Date(2000, 01, 01));

            Assert.That(stock.ToString(), Is.EqualTo("ABC - ABC Pty Ltd"));
        }

        [TestCase]
        public void ToStringPropertiesChangedToday()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);
            stock.ChangeProperties(Date.Today, "XYZ", "XYZ Pty Ltd", AssetCategory.AustralianStocks);

            Assert.That(stock.ToString(), Is.EqualTo("XYZ - XYZ Pty Ltd"));
        }

        [TestCase]
        public void ToStringPropertiesChangedBeforeToday()
        { 
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);
            stock.ChangeProperties(Date.Today.AddDays(-1), "XYZ", "XYZ Pty Ltd", AssetCategory.AustralianStocks);

            Assert.That(stock.ToString(), Is.EqualTo("XYZ - XYZ Pty Ltd"));
        }

        [TestCase]
        public void ToStringPropertiesChangedAfterToday()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);
            stock.ChangeProperties(Date.Today.AddDays(1), "XYZ", "XYZ Pty Ltd", AssetCategory.AustralianStocks);

            Assert.That(stock.ToString(), Is.EqualTo("ABC - ABC Pty Ltd"));
        }

        [TestCase]
        public void List()
        {
            var listingDate = new Date(2000, 01, 01);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", listingDate, true, AssetCategory.AustralianProperty);

            Assert.Multiple(() =>
            {
                Assert.That(stock.Trust, Is.True);

                Assert.That(stock.EffectivePeriod.FromDate, Is.EqualTo(listingDate));
                Assert.That(stock.EffectivePeriod.ToDate, Is.EqualTo(Date.MaxValue));

                var properties = stock.Properties[listingDate];
                Assert.That(properties.ASXCode, Is.EqualTo("ABC"));
                Assert.That(properties.Name, Is.EqualTo("ABC Pty Ltd"));
                Assert.That(properties.Category, Is.EqualTo(AssetCategory.AustralianProperty));

                // Check default values are set
                var dividendRules = stock.DividendRules[listingDate];
                Assert.That(dividendRules.CompanyTaxRate, Is.EqualTo(0.30m));
                Assert.That(dividendRules.DividendRoundingRule, Is.EqualTo(RoundingRule.Round));
                Assert.That(dividendRules.DRPActive, Is.EqualTo(false));
                Assert.That(dividendRules.DRPMethod, Is.EqualTo(DRPMethod.Round));
            });
        }

        [TestCase]
        public void ListWhenAlreadyListed()
        {
            var listingDate = new Date(2000, 01, 01);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", listingDate, true, AssetCategory.AustralianProperty);

            Assert.That(() => stock.List("XYZ", "XYZ Pty Ltd", listingDate, true, AssetCategory.AustralianProperty), Throws.Exception.InstanceOf(typeof(EffectiveDateException)));
        }


        [TestCase]
        public void ListWhenDelisted()
        {
            var listingDate = new Date(2000, 01, 01);
            var delistingDate = new Date(2002, 01, 01);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", listingDate, true, AssetCategory.AustralianProperty);
            stock.DeList(delistingDate);

            Assert.That(() => stock.List("XYZ", "XYZ Pty Ltd", listingDate, true, AssetCategory.AustralianProperty), Throws.Exception.InstanceOf(typeof(EffectiveDateException)));
        }

        [TestCase]
        public void DeList()
        {
            var listingDate = new Date(2000, 01, 01);
            var delistingDate = new Date(2002, 01, 01);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", listingDate, true, AssetCategory.AustralianProperty);
            stock.DeList(delistingDate);

            Assert.Multiple(() =>
            {
                Assert.That(stock.Trust, Is.True);

                Assert.That(stock.EffectivePeriod.FromDate, Is.EqualTo(listingDate));
                Assert.That(stock.EffectivePeriod.ToDate, Is.EqualTo(delistingDate));

                var propertyValues = stock.Properties.Values.ToList();
                Assert.That(propertyValues.Last().EffectivePeriod.ToDate, Is.EqualTo(delistingDate));
            });
        }

        [TestCase]
        public void DeListWithoutBeingListed()
        {
            var listingDate = new Date(2000, 01, 01);
            var delistingDate = new Date(2002, 01, 01);

            var stock = new Stock(Guid.NewGuid());

            Assert.That(() => stock.DeList(delistingDate), Throws.Exception.InstanceOf(typeof(EffectiveDateException)));

        }

        [TestCase]
        public void GetPrice()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var priceHistory = mockRepository.Create<IStockPriceHistory>();
            priceHistory.Setup(x => x.GetPrice(new Date(2000, 01, 01))).Returns(10.00m).Verifiable();

            var stock = new Stock(Guid.NewGuid());
            stock.SetPriceHistory(priceHistory.Object);

            var price = stock.GetPrice(new Date(2000, 01, 01));

            Assert.That(price, Is.EqualTo(10.00m));

            mockRepository.Verify();
        }

        [TestCase]
        public void GetPricePriceHistoryNotSet()
        {
            var stock = new Stock(Guid.NewGuid());

            var price = stock.GetPrice(new Date(2000, 01, 01));
        }

        [TestCase]
        public void GetPrices()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var historicPrices = new StockPrice[]
                {
                    new StockPrice(new Date(2000, 01, 01), 10.00m),
                    new StockPrice(new Date(2000, 01, 02), 11.00m),
                    new StockPrice(new Date(2000, 01, 03), 12.00m),
                    new StockPrice(new Date(2000, 01, 04), 13.00m),
                    new StockPrice(new Date(2000, 01, 05), 14.00m),
                    new StockPrice(new Date(2000, 01, 06), 15.00m),
                    new StockPrice(new Date(2000, 01, 07), 16.00m),
                    new StockPrice(new Date(2000, 01, 08), 17.00m),
                    new StockPrice(new Date(2000, 01, 09), 18.00m),
                    new StockPrice(new Date(2000, 01, 10), 19.00m)
                };


            var priceHistory = mockRepository.Create<IStockPriceHistory>();
            priceHistory.Setup(x => x.GetPrices(new DateRange(new Date(2000, 01, 01), new Date(2000, 01, 10)))).Returns(historicPrices).Verifiable();

            var stock = new Stock(Guid.NewGuid());
            stock.SetPriceHistory(priceHistory.Object);

            var prices = stock.GetPrices(new DateRange(new Date(2000, 01, 01), new Date(2000, 01, 10)));

            Assert.That(prices, Is.EqualTo(historicPrices));

            mockRepository.Verify();
        }

        [TestCase]
        public void GetPricesPriceHistoryNotSet()
        {
            var stock = new Stock(Guid.NewGuid());

            var prices = stock.GetPrices(new DateRange(new Date(2000, 01, 01), new Date(2000, 01, 10)));
        }

        [TestCase]
        public void DateOfLastestPrice()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var priceHistory = mockRepository.Create<IStockPriceHistory>();
            priceHistory.Setup(x => x.LatestDate).Returns((new Date(2000, 01, 10)));

            var stock = new Stock(Guid.NewGuid());
            stock.SetPriceHistory(priceHistory.Object);

            var date = stock.DateOfLastestPrice();

            Assert.That(date, Is.EqualTo(new Date(2000, 01, 10)));

            mockRepository.Verify();
        }

        [TestCase]
        public void DateOfLastestPricePriceHistoryNotSet()
        {
            var stock = new Stock(Guid.NewGuid());

            var date = stock.DateOfLastestPrice();

            Assert.That(date, Is.EqualTo(Date.MinValue));
        }

        [TestCase]
        public void ChangeProperties()
        {
            var listingDate = new Date(2000, 01, 01);
            var changeDate = new Date(2001, 01, 01);

            var stock = new Stock(Guid.NewGuid());

            stock.List("ABC", "ABC Pty Ltd", listingDate, false, AssetCategory.AustralianStocks);
            stock.ChangeProperties(changeDate, "XYZ", "XYZ Pty Ltd", AssetCategory.AustralianProperty);

            Assert.Multiple(() => 
            {
                Assert.That(stock.Properties[changeDate.AddDays(-1)].ASXCode, Is.EqualTo("ABC"));
                Assert.That(stock.Properties[changeDate.AddDays(-1)].Name, Is.EqualTo("ABC Pty Ltd"));
                Assert.That(stock.Properties[changeDate.AddDays(-1)].Category, Is.EqualTo(AssetCategory.AustralianStocks));

                Assert.That(stock.Properties[changeDate].ASXCode, Is.EqualTo("XYZ"));
                Assert.That(stock.Properties[changeDate].Name, Is.EqualTo("XYZ Pty Ltd"));
                Assert.That(stock.Properties[changeDate].Category, Is.EqualTo(AssetCategory.AustralianProperty));
            });
        }

        [TestCase]
        public void ChangePropertiesBeforeListing()
        {
            var listingDate = new Date(2000, 01, 01);
            var changeDate = new Date(1999, 01, 01);

            var stock = new Stock(Guid.NewGuid());

            stock.List("ABC", "ABC Pty Ltd", listingDate, false, AssetCategory.AustralianStocks);
            Assert.That(() => stock.ChangeProperties(changeDate, "XYZ", "XYZ Pty Ltd", AssetCategory.AustralianProperty), Throws.Exception.InstanceOf(typeof(EffectiveDateException)));
        }

        [TestCase]
        public void ChangePropertiesAfterDeListing()
        {
            var listingDate = new Date(2000, 01, 01);
            var delistingDate = new Date(2002, 01, 01);
            var changeDate = new Date(2003, 01, 01);

            var stock = new Stock(Guid.NewGuid());

            stock.List("ABC", "ABC Pty Ltd", listingDate, false, AssetCategory.AustralianStocks);
            stock.DeList(delistingDate);
            Assert.That(() => stock.ChangeProperties(changeDate, "XYZ", "XYZ Pty Ltd", AssetCategory.AustralianProperty), Throws.Exception.InstanceOf(typeof(EffectiveDateException)));
        }

        [TestCase]
        public void ChangePropertiesTwiceOnSameDate()
        {
            var listingDate = new Date(2000, 01, 01);
            var changeDate = new Date(2001, 01, 01);

            var stock = new Stock(Guid.NewGuid());

            stock.List("ABC", "ABC Pty Ltd", listingDate, false, AssetCategory.AustralianStocks);
            stock.ChangeProperties(changeDate, "DEF", "DEF Pty Ltd", AssetCategory.InternationalProperty);
            stock.ChangeProperties(changeDate, "XYZ", "XYZ Pty Ltd", AssetCategory.AustralianProperty);

            Assert.Multiple(() =>
            {
                Assert.That(stock.Properties[changeDate].ASXCode, Is.EqualTo("XYZ"));
                Assert.That(stock.Properties[changeDate].Name, Is.EqualTo("XYZ Pty Ltd"));
                Assert.That(stock.Properties[changeDate].Category, Is.EqualTo(AssetCategory.AustralianProperty));
            });
        }

        [TestCase]
        public void ChangeDividendRules()
        {
            var listingDate = new Date(2000, 01, 01);
            var changeDate = new Date(2001, 01, 01);

            var stock = new Stock(Guid.NewGuid());

            stock.List("ABC", "ABC Pty Ltd", listingDate, false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(changeDate, 0.40m, RoundingRule.Truncate, true, DRPMethod.RetainCashBalance);

            Assert.Multiple(() =>
            {
                Assert.That(stock.DividendRules[changeDate.AddDays(-1)].CompanyTaxRate, Is.EqualTo(0.30m));
                Assert.That(stock.DividendRules[changeDate.AddDays(-1)].DividendRoundingRule, Is.EqualTo(RoundingRule.Round));
                Assert.That(stock.DividendRules[changeDate.AddDays(-1)].DRPActive, Is.EqualTo(false));
                Assert.That(stock.DividendRules[changeDate.AddDays(-1)].DRPMethod, Is.EqualTo(DRPMethod.Round));

                Assert.That(stock.DividendRules[changeDate].CompanyTaxRate, Is.EqualTo(0.40m));
                Assert.That(stock.DividendRules[changeDate].DividendRoundingRule, Is.EqualTo(RoundingRule.Truncate));
                Assert.That(stock.DividendRules[changeDate].DRPActive, Is.EqualTo(true));
                Assert.That(stock.DividendRules[changeDate].DRPMethod, Is.EqualTo(DRPMethod.RetainCashBalance));
            });
        }

        [TestCase]
        public void ChangeDividendRulesBeforeListing()
        {
            var listingDate = new Date(2000, 01, 01);
            var changeDate = new Date(1999, 01, 01);

            var stock = new Stock(Guid.NewGuid());

            stock.List("ABC", "ABC Pty Ltd", listingDate, false, AssetCategory.AustralianStocks);
            Assert.That(() => stock.ChangeDividendRules(changeDate, 0.40m, RoundingRule.Truncate, true, DRPMethod.RetainCashBalance), Throws.Exception.InstanceOf(typeof(EffectiveDateException)));
        }

        [TestCase]
        public void ChangeDividendRulesAfterDeListing()
        {
            var listingDate = new Date(2000, 01, 01);
            var delistingDate = new Date(2002, 01, 01);
            var changeDate = new Date(2003, 01, 01);

            var stock = new Stock(Guid.NewGuid());

            stock.List("ABC", "ABC Pty Ltd", listingDate, false, AssetCategory.AustralianStocks);
            stock.DeList(delistingDate);
            Assert.That(() => stock.ChangeDividendRules(changeDate, 0.40m, RoundingRule.Truncate, true, DRPMethod.RetainCashBalance), Throws.Exception.InstanceOf(typeof(EffectiveDateException)));
        }

        [TestCase]
        public void ChangeDividendRulesTwiceOnSameDate()
        {
            var listingDate = new Date(2000, 01, 01);
            var changeDate = new Date(2001, 01, 01);

            var stock = new Stock(Guid.NewGuid());

            stock.List("ABC", "ABC Pty Ltd", listingDate, false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(changeDate, 0.50m, RoundingRule.Round, true, DRPMethod.RoundDown);
            stock.ChangeDividendRules(changeDate, 0.40m, RoundingRule.Truncate, true, DRPMethod.RetainCashBalance);

            Assert.Multiple(() =>
            {
                Assert.That(stock.DividendRules[changeDate].CompanyTaxRate, Is.EqualTo(0.40m));
                Assert.That(stock.DividendRules[changeDate].DividendRoundingRule, Is.EqualTo(RoundingRule.Truncate));
                Assert.That(stock.DividendRules[changeDate].DRPActive, Is.EqualTo(true));
                Assert.That(stock.DividendRules[changeDate].DRPMethod, Is.EqualTo(DRPMethod.RetainCashBalance));
            });
        }
    }
}
