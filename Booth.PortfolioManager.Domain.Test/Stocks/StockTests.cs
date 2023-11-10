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
            stock.List("ABC", "ABC Pty Ltd", new Date(3000, 01, 01), false, AssetCategory.AustralianStocks);

            stock.ToString().Should().Be("ABC - ABC Pty Ltd");
        }

        [Fact]
        public void ToStringAfterEndDate()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);
            stock.DeList(new Date(2000, 01, 01));

            stock.ToString().Should().Be("ABC - ABC Pty Ltd");
        }

        [Fact]
        public void ToStringPropertiesChangedToday()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);
            stock.ChangeProperties(Date.Today, "XYZ", "XYZ Pty Ltd", AssetCategory.AustralianStocks);

            stock.ToString().Should().Be("XYZ - XYZ Pty Ltd");
        }

        [Fact]
        public void ToStringPropertiesChangedBeforeToday()
        { 
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);
            stock.ChangeProperties(Date.Today.AddDays(-1), "XYZ", "XYZ Pty Ltd", AssetCategory.AustralianStocks);

            stock.ToString().Should().Be("XYZ - XYZ Pty Ltd");
        }

        [Fact]
        public void ToStringPropertiesChangedAfterToday()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);
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
        public void ListWithoutADate()
        {
            var stock = new Stock(Guid.NewGuid());

            Action a = () => stock.List("XYZ", "XYZ Pty Ltd", Date.MinValue, true, AssetCategory.AustralianProperty);

            a.Should().Throw<ArgumentOutOfRangeException>();
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
