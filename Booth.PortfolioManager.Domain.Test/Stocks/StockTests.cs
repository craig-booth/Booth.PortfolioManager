using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using FluentAssertions.Execution;

namespace Booth.PortfolioManager.Domain.Test.Stocks
{
    public class StockTests
    {
        [Fact]
        public void ToStringNoProperties()
        {
            var stock = new Stock(Guid.NewGuid());

            stock.ToString().Should().Be(" - ");
        }

        [Fact]
        public void ToStringBeforeStartDate()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MaxValue, false, AssetCategory.AustralianStocks);

            stock.ToString().Should().Be("ABC - ABC Pty Ltd");
        }

        [Fact]
        public void ToStringAfterEndDate()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);
            stock.DeList(new Date(2000, 01, 01));

            stock.ToString().Should().Be("ABC - ABC Pty Ltd");
        }

        [Fact]
        public void ToStringPropertiesChangedToday()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);
            stock.ChangeProperties(Date.Today, "XYZ", "XYZ Pty Ltd", AssetCategory.AustralianStocks);

            stock.ToString().Should().Be("XYZ - XYZ Pty Ltd");
        }

        [Fact]
        public void ToStringPropertiesChangedBeforeToday()
        { 
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);
            stock.ChangeProperties(Date.Today.AddDays(-1), "XYZ", "XYZ Pty Ltd", AssetCategory.AustralianStocks);

            stock.ToString().Should().Be("XYZ - XYZ Pty Ltd");
        }

        [Fact]
        public void ToStringPropertiesChangedAfterToday()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);
            stock.ChangeProperties(Date.Today.AddDays(1), "XYZ", "XYZ Pty Ltd", AssetCategory.AustralianStocks);

            stock.ToString().Should().Be("ABC - ABC Pty Ltd");
        }

        [Fact]
        public void List()
        {
            var listingDate = new Date(2000, 01, 01);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", listingDate, true, AssetCategory.AustralianProperty);

            using (new AssertionScope())
            {
                stock.Should().BeEquivalentTo(new
                {
                    Trust = true,
                    EffectivePeriod = new DateRange(listingDate, Date.MaxValue)
                }); ;

                stock.Properties[listingDate].Should().BeEquivalentTo(new {
                    AsxCode = "ABC",
                    Name = "ABC Pty Ltd",
                    Category = AssetCategory.AustralianProperty
                });

                // Check default values are set
                stock.DividendRules[listingDate].Should().BeEquivalentTo(new
                {
                    CompanyTaxRate = 0.30m,
                    DividendRoundingRule = RoundingRule.Round,
                    DrpActive = false,
                    DrpMethod = DrpMethod.Round
                });

            }
        }

        [Fact]
        public void ListWhenAlreadyListed()
        {
            var listingDate = new Date(2000, 01, 01);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", listingDate, true, AssetCategory.AustralianProperty);

            Action a = () => stock.List("XYZ", "XYZ Pty Ltd", listingDate, true, AssetCategory.AustralianProperty);

            a.Should().Throw<EffectiveDateException>();
        }


        [Fact]
        public void ListWhenDelisted()
        {
            var listingDate = new Date(2000, 01, 01);
            var delistingDate = new Date(2002, 01, 01);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", listingDate, true, AssetCategory.AustralianProperty);
            stock.DeList(delistingDate);

            Action a = () => stock.List("XYZ", "XYZ Pty Ltd", listingDate, true, AssetCategory.AustralianProperty);

            a.Should().Throw<EffectiveDateException>();
        }

        [Fact]
        public void DeList()
        {
            var listingDate = new Date(2000, 01, 01);
            var delistingDate = new Date(2002, 01, 01);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", listingDate, true, AssetCategory.AustralianProperty);
            stock.DeList(delistingDate);

            using (new AssertionScope())
            {
                stock.Should().BeEquivalentTo(new
                {
                    Trust = true,
                    EffectivePeriod = new DateRange(listingDate, delistingDate)
                }); ;

                stock.Properties.Values.Last().EffectivePeriod.ToDate.Should().Be(delistingDate);
                stock.DividendRules.Values.Last().EffectivePeriod.ToDate.Should().Be(delistingDate);
            }
        }

        [Fact]
        public void DeListWithoutBeingListed()
        {
            var listingDate = new Date(2000, 01, 01);
            var delistingDate = new Date(2002, 01, 01);

            var stock = new Stock(Guid.NewGuid());

            Action a = () => stock.DeList(delistingDate);

            a.Should().Throw<EffectiveDateException>();

        }

        [Fact]
        public void GetPrice()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var priceHistory = mockRepository.Create<IStockPriceHistory>();
            priceHistory.Setup(x => x.GetPrice(new Date(2000, 01, 01))).Returns(10.00m).Verifiable();

            var stock = new Stock(Guid.NewGuid());
            stock.SetPriceHistory(priceHistory.Object);

            var price = stock.GetPrice(new Date(2000, 01, 01));

            price.Should().Be(10.00m);

            mockRepository.Verify();
        }

        [Fact]
        public void GetPricePriceHistoryNotSet()
        {
            var stock = new Stock(Guid.NewGuid());

            var price = stock.GetPrice(new Date(2000, 01, 01));
        }

        [Fact]
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

            prices.Should().Equal(historicPrices);

            mockRepository.Verify();
        }

        [Fact]
        public void GetPricesPriceHistoryNotSet()
        {
            var stock = new Stock(Guid.NewGuid());

            var prices = stock.GetPrices(new DateRange(new Date(2000, 01, 01), new Date(2000, 01, 10)));
        }

        [Fact]
        public void DateOfLastestPrice()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var priceHistory = mockRepository.Create<IStockPriceHistory>();
            priceHistory.Setup(x => x.LatestDate).Returns((new Date(2000, 01, 10)));

            var stock = new Stock(Guid.NewGuid());
            stock.SetPriceHistory(priceHistory.Object);

            var date = stock.DateOfLastestPrice();

            date.Should().Be(new Date(2000, 01, 10));

            mockRepository.Verify();
        }

        [Fact]
        public void DateOfLastestPricePriceHistoryNotSet()
        {
            var stock = new Stock(Guid.NewGuid());

            var date = stock.DateOfLastestPrice();

            date.Should().Be(Date.MinValue);
        }

        [Fact]
        public void ChangeProperties()
        {
            var listingDate = new Date(2000, 01, 01);
            var changeDate = new Date(2001, 01, 01);

            var stock = new Stock(Guid.NewGuid());

            stock.List("ABC", "ABC Pty Ltd", listingDate, false, AssetCategory.AustralianStocks);
            stock.ChangeProperties(changeDate, "XYZ", "XYZ Pty Ltd", AssetCategory.AustralianProperty);

            using (new AssertionScope())
            {
                stock.Properties[changeDate.AddDays(-1)].Should().BeEquivalentTo(new
                {
                    AsxCode = "ABC",
                    Name = "ABC Pty Ltd",
                    Category = AssetCategory.AustralianStocks
                });

                stock.Properties[changeDate].Should().BeEquivalentTo(new
                {
                    AsxCode = "XYZ",
                    Name = "XYZ Pty Ltd",
                    Category = AssetCategory.AustralianProperty
                });
            }

        }

        [Fact]
        public void ChangePropertiesBeforeListing()
        {
            var listingDate = new Date(2000, 01, 01);
            var changeDate = new Date(1999, 01, 01);

            var stock = new Stock(Guid.NewGuid());

            stock.List("ABC", "ABC Pty Ltd", listingDate, false, AssetCategory.AustralianStocks);
            Action a = () => stock.ChangeProperties(changeDate, "XYZ", "XYZ Pty Ltd", AssetCategory.AustralianProperty);

            a.Should().Throw<EffectiveDateException>();
        }

        [Fact]
        public void ChangePropertiesAfterDeListing()
        {
            var listingDate = new Date(2000, 01, 01);
            var delistingDate = new Date(2002, 01, 01);
            var changeDate = new Date(2003, 01, 01);

            var stock = new Stock(Guid.NewGuid());

            stock.List("ABC", "ABC Pty Ltd", listingDate, false, AssetCategory.AustralianStocks);
            stock.DeList(delistingDate);
            Action a = () => stock.ChangeProperties(changeDate, "XYZ", "XYZ Pty Ltd", AssetCategory.AustralianProperty);

            a.Should().Throw<EffectiveDateException>();
        }

        [Fact]
        public void ChangePropertiesTwiceOnSameDate()
        {
            var listingDate = new Date(2000, 01, 01);
            var changeDate = new Date(2001, 01, 01);

            var stock = new Stock(Guid.NewGuid());

            stock.List("ABC", "ABC Pty Ltd", listingDate, false, AssetCategory.AustralianStocks);
            stock.ChangeProperties(changeDate, "DEF", "DEF Pty Ltd", AssetCategory.InternationalProperty);
            stock.ChangeProperties(changeDate, "XYZ", "XYZ Pty Ltd", AssetCategory.AustralianProperty);

            stock.Properties[changeDate].Should().BeEquivalentTo(new
            {
                AsxCode = "XYZ",
                Name = "XYZ Pty Ltd",
                Category = AssetCategory.AustralianProperty
            });
        }

        [Fact]
        public void ChangeDividendRules()
        {
            var listingDate = new Date(2000, 01, 01);
            var changeDate = new Date(2001, 01, 01);

            var stock = new Stock(Guid.NewGuid());

            stock.List("ABC", "ABC Pty Ltd", listingDate, false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(changeDate, 0.40m, RoundingRule.Truncate, true, DrpMethod.RetainCashBalance);

            using (new AssertionScope())
            {
                stock.DividendRules[changeDate.AddDays(-1)].Should().BeEquivalentTo(new
                {
                    CompanyTaxRate = 0.30m,
                    DividendRoundingRule = RoundingRule.Round,
                    DrpActive = false,
                    DrpMethod = DrpMethod.Round
                });

                stock.DividendRules[changeDate].Should().BeEquivalentTo(new
                {
                    CompanyTaxRate = 0.40m,
                    DividendRoundingRule = RoundingRule.Truncate,
                    DrpActive = true,
                    DrpMethod = DrpMethod.RetainCashBalance
                });
            }
        }

        [Fact]
        public void ChangeDividendRulesBeforeListing()
        {
            var listingDate = new Date(2000, 01, 01);
            var changeDate = new Date(1999, 01, 01);

            var stock = new Stock(Guid.NewGuid());

            stock.List("ABC", "ABC Pty Ltd", listingDate, false, AssetCategory.AustralianStocks);
            Action a = () => stock.ChangeDividendRules(changeDate, 0.40m, RoundingRule.Truncate, true, DrpMethod.RetainCashBalance);

            a.Should().Throw<EffectiveDateException>();
        }

        [Fact]
        public void ChangeDividendRulesAfterDeListing()
        {
            var listingDate = new Date(2000, 01, 01);
            var delistingDate = new Date(2002, 01, 01);
            var changeDate = new Date(2003, 01, 01);

            var stock = new Stock(Guid.NewGuid());

            stock.List("ABC", "ABC Pty Ltd", listingDate, false, AssetCategory.AustralianStocks);
            stock.DeList(delistingDate);
            Action a = () => stock.ChangeDividendRules(changeDate, 0.40m, RoundingRule.Truncate, true, DrpMethod.RetainCashBalance);

            a.Should().Throw<EffectiveDateException>();
        }

        [Fact]
        public void ChangeDividendRulesTwiceOnSameDate()
        {
            var listingDate = new Date(2000, 01, 01);
            var changeDate = new Date(2001, 01, 01);

            var stock = new Stock(Guid.NewGuid());

            stock.List("ABC", "ABC Pty Ltd", listingDate, false, AssetCategory.AustralianStocks);
            stock.ChangeDividendRules(changeDate, 0.50m, RoundingRule.Round, true, DrpMethod.RoundDown);
            stock.ChangeDividendRules(changeDate, 0.40m, RoundingRule.Truncate, true, DrpMethod.RetainCashBalance);

            stock.DividendRules[changeDate].Should().BeEquivalentTo(new
            {
                CompanyTaxRate = 0.40m,
                DividendRoundingRule = RoundingRule.Truncate,
                DrpActive = true,
                DrpMethod = DrpMethod.RetainCashBalance
            });
        }
    }
}
