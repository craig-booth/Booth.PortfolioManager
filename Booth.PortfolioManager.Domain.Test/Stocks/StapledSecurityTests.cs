using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Test.Stocks
{
    public class StapledSecurityTests
    {
        [Fact]
        public void ListWithoutChildSecurities()
        {
            var listingDate = new Date(2000, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", listingDate, AssetCategory.AustralianProperty, new StapledSecurityChild[0]);

            using (new AssertionScope())
            {
                stock.Should().BeEquivalentTo(new
                {
                    Trust = false,
                    EffectivePeriod = new DateRange(listingDate, Date.MaxValue)
                });
 
                stock.Properties[listingDate].Should().BeEquivalentTo(new
                {
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

                stock.RelativeNTAs[listingDate].Percentages.Should().BeEmpty();

                stock.ChildSecurities.Should().BeEmpty();
            }
        }

        [Fact]
        public void ListWithOneChildSecurity()
        {
            var listingDate = new Date(2000, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());

            var childSecurities = new StapledSecurityChild[]
            {
                new StapledSecurityChild("ABC_1", "Child 1", true)
            };
            stock.List("ABC", "ABC Pty Ltd", listingDate, AssetCategory.AustralianProperty, childSecurities);

            using (new AssertionScope())
            {
                stock.Should().BeEquivalentTo(new
                {
                    Trust = false,
                    EffectivePeriod = new DateRange(listingDate, Date.MaxValue)
                });
   
                stock.RelativeNTAs[listingDate].Percentages.Should().Equal(new decimal[] { 1.00m });

                stock.ChildSecurities.Should().SatisfyRespectively(
                    first => first.Should().BeEquivalentTo(new
                    {
                        AsxCode = "ABC_1",
                        Name = "Child 1",
                        Trust = true
                    }));
            }
        }

        [Fact]
        public void ListWithThreeChildSecurities()
        {
            var listingDate = new Date(2000, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());

            var childSecurities = new StapledSecurityChild[]
            {
                new StapledSecurityChild("ABC_1", "Child 1", true),
                new StapledSecurityChild("ABC_2", "Child 2", false),
                new StapledSecurityChild("ABC_3", "Child 3", true)
            };
            stock.List("ABC", "ABC Pty Ltd", listingDate, AssetCategory.AustralianProperty, childSecurities);

            using (new AssertionScope())
            {
                stock.Should().BeEquivalentTo(new
                {
                    Trust = false,
                    EffectivePeriod = new DateRange(listingDate, Date.MaxValue)
                });

                stock.RelativeNTAs[listingDate].Percentages.Should().Equal(new decimal[] { 0.33m, 0.34m, 0.33m });

                stock.ChildSecurities.Should().SatisfyRespectively(
                    first => first.Should().BeEquivalentTo(new
                    {
                        AsxCode = "ABC_1",
                        Name = "Child 1",
                        Trust = true
                    }),
                    second => second.Should().BeEquivalentTo(new
                    {
                        AsxCode = "ABC_2",
                        Name = "Child 2",
                        Trust = false
                    }),
                    third => third.Should().BeEquivalentTo(new
                    {
                        AsxCode = "ABC_3",
                        Name = "Child 3",
                        Trust = true
                    }));
            }

        }

        [Fact]
        public void ListWhenAlreadyListed()
        {
            var listingDate = new Date(2000, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", listingDate, AssetCategory.AustralianProperty, new StapledSecurityChild[0]);

            Action a = () => stock.List("XYZ", "XYZ Pty Ltd", listingDate, AssetCategory.AustralianProperty, new StapledSecurityChild[0]);
            
            a.Should().Throw<EffectiveDateException>();
        }


        [Fact]
        public void ListWhenDelisted()
        {
            var listingDate = new Date(2000, 01, 01);
            var delistingDate = new Date(2002, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", listingDate, AssetCategory.AustralianProperty, new StapledSecurityChild[0]);
            stock.DeList(delistingDate);

            Action a = () => stock.List("XYZ", "XYZ Pty Ltd", listingDate, AssetCategory.AustralianProperty, new StapledSecurityChild[0]);
           
            a.Should().Throw<EffectiveDateException>();
        }

        [Fact]
        public void DeList()
        {
            var listingDate = new Date(2000, 01, 01);
            var delistingDate = new Date(2002, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", listingDate, AssetCategory.AustralianProperty, new StapledSecurityChild[0]);
            stock.DeList(delistingDate);

            using (new AssertionScope())
            {
                stock.Should().BeEquivalentTo(new
                {
                    Trust = false,
                    EffectivePeriod = new DateRange(listingDate, delistingDate)
                });

                stock.Properties.Values.Last().EffectivePeriod.ToDate.Should().Be(delistingDate);
                stock.DividendRules.Values.Last().EffectivePeriod.ToDate.Should().Be(delistingDate);
                stock.RelativeNTAs.Values.Last().EffectivePeriod.ToDate.Should().Be(delistingDate);
            }
        }

        [Fact]
        public void DeListWithoutBeingListed()
        {
            var listingDate = new Date(2000, 01, 01);
            var delistingDate = new Date(2002, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());

            Action a = () => stock.DeList(delistingDate);

            a.Should().Throw<EffectiveDateException>();
        }

        [Fact]
        public void SetRelativeNTAsBeforeListing()
        {
            var listingDate = new Date(2000, 01, 01);
            var changeDate = new Date(1999, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());

            var childSecurities = new StapledSecurityChild[]
            {
                new StapledSecurityChild("ABC_1", "Child 1", true),
                new StapledSecurityChild("ABC_2", "Child 2", false)
            };
            stock.List("ABC", "ABC Pty Ltd", listingDate, AssetCategory.AustralianProperty, new StapledSecurityChild[0]);

            Action a = () => stock.SetRelativeNTAs(changeDate, new decimal[] { 0.50m, 0.50m });

            a.Should().Throw<EffectiveDateException>();
        }

        [Fact]
        public void SetRelativeNTAsAfterDeListing()
        {
            var listingDate = new Date(2000, 01, 01);
            var delistingDate = new Date(2001, 01, 01);
            var changeDate = new Date(2002, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());

            var childSecurities = new StapledSecurityChild[]
            {
                new StapledSecurityChild("ABC_1", "Child 1", true),
                new StapledSecurityChild("ABC_2", "Child 2", false)
            };
            stock.List("ABC", "ABC Pty Ltd", listingDate, AssetCategory.AustralianProperty, new StapledSecurityChild[0]);
            stock.DeList(delistingDate);

            Action a = () => stock.SetRelativeNTAs(changeDate, new decimal[] { 0.50m, 0.50m });

            a.Should().Throw<EffectiveDateException>();
        }

        [Fact]
        public void SetRelativeNTAsTwiceOnSameDay()
        {
            var listingDate = new Date(2000, 01, 01);
            var changeDate = new Date(2002, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());

            var childSecurities = new StapledSecurityChild[]
            {
                new StapledSecurityChild("ABC_1", "Child 1", true),
                new StapledSecurityChild("ABC_2", "Child 2", false)
            };
            stock.List("ABC", "ABC Pty Ltd", listingDate, AssetCategory.AustralianProperty, childSecurities);

            stock.SetRelativeNTAs(changeDate, new decimal[] { 0.30m, 0.70m });
            stock.SetRelativeNTAs(changeDate, new decimal[] { 0.60m, 0.40m });

            stock.RelativeNTAs[changeDate].Percentages.Should().Equal(new decimal[] { 0.60m, 0.40m });
        }

        [Fact]
        public void SetRelativeNTAsWithLessPercentagesThanChildSecurities()
        {
           var listingDate = new Date(2000, 01, 01);
            var changeDate = new Date(2002, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());

            var childSecurities = new StapledSecurityChild[]
            {
                new StapledSecurityChild("ABC_1", "Child 1", true),
                new StapledSecurityChild("ABC_2", "Child 2", false)
            };
            stock.List("ABC", "ABC Pty Ltd", listingDate, AssetCategory.AustralianProperty, childSecurities);

            Action a = () => stock.SetRelativeNTAs(changeDate, new decimal[] { 1.00m });

            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void SetRelativeNTAsWithMorePercentagesThanChildSecurities()
        {
            var listingDate = new Date(2000, 01, 01);
            var changeDate = new Date(2002, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());

            var childSecurities = new StapledSecurityChild[]
            {
                new StapledSecurityChild("ABC_1", "Child 1", true),
                new StapledSecurityChild("ABC_2", "Child 2", false)
            };
            stock.List("ABC", "ABC Pty Ltd", listingDate, AssetCategory.AustralianProperty, childSecurities);

            Action a = () => stock.SetRelativeNTAs(changeDate, new decimal[] { 0.30m, 0.50m, 0.10m, 0.10m });

            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void SetRelativeNTAsPercentagesDontAddTo100()
        {
            var listingDate = new Date(2000, 01, 01);
            var changeDate = new Date(2002, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());

            var childSecurities = new StapledSecurityChild[]
            {
                new StapledSecurityChild("ABC_1", "Child 1", true),
                new StapledSecurityChild("ABC_2", "Child 2", false)
            };
            stock.List("ABC", "ABC Pty Ltd", listingDate, AssetCategory.AustralianProperty, childSecurities);

            Action a = () => stock.SetRelativeNTAs(changeDate, new decimal[] { 0.60m, 0.60m });

            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void SetRelativeNTAs()
        {
            var listingDate = new Date(2000, 01, 01);
            var changeDate = new Date(2002, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());

            var childSecurities = new StapledSecurityChild[]
            {
                new StapledSecurityChild("ABC_1", "Child 1", true),
                new StapledSecurityChild("ABC_2", "Child 2", false)
            };
            stock.List("ABC", "ABC Pty Ltd", listingDate, AssetCategory.AustralianProperty, childSecurities);

            stock.SetRelativeNTAs(listingDate, new decimal[] { 0.50m, 0.50m });
            stock.SetRelativeNTAs(changeDate, new decimal[] { 0.60m, 0.40m });

            using (new AssertionScope())
            {
                stock.RelativeNTAs[changeDate.AddDays(-1)].Percentages.Should().Equal(new decimal[] { 0.50m, 0.50m });
                stock.RelativeNTAs[changeDate].Percentages.Should().Equal(new decimal[] { 0.60m, 0.40m });
            }
        }
    }
}
