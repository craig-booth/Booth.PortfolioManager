using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Test.Stocks
{
    class StapledSecurityTests
    {
        [TestCase]
        public void ListWithoutChildSecurities()
        {
            var listingDate = new Date(2000, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", listingDate, AssetCategory.AustralianProperty, new StapledSecurityChild[0]);

            Assert.Multiple(() =>
            {
                Assert.That(stock.Trust, Is.False);

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

                var ntas = stock.RelativeNTAs[listingDate];
                Assert.That(ntas.Percentages, Is.Empty);

                Assert.That(stock.ChildSecurities, Is.Empty);
            });
        }

        [TestCase]
        public void ListWithOneChildSecurity()
        {
            var listingDate = new Date(2000, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());

            var childSecurities = new StapledSecurityChild[]
            {
                new StapledSecurityChild("ABC_1", "Child 1", true)
            };
            stock.List("ABC", "ABC Pty Ltd", listingDate, AssetCategory.AustralianProperty, childSecurities);

            Assert.Multiple(() =>
            {
                Assert.That(stock.Trust, Is.False);

                var ntas = stock.RelativeNTAs[listingDate];
                Assert.That(ntas.Percentages, Is.EqualTo(new decimal[] { 1.00m }));

                Assert.That(stock.ChildSecurities.Count, Is.EqualTo(1));
                if (stock.ChildSecurities.Count >= 1)
                {
                    Assert.That(stock.ChildSecurities[0].ASXCode, Is.EqualTo("ABC_1"));
                    Assert.That(stock.ChildSecurities[0].Name, Is.EqualTo("Child 1"));
                    Assert.That(stock.ChildSecurities[0].Trust, Is.EqualTo(true));
                }
            });
        }

        [TestCase]
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

            Assert.Multiple(() =>
            {
                Assert.That(stock.Trust, Is.False);

                var ntas = stock.RelativeNTAs[listingDate];
                Assert.That(ntas.Percentages, Is.EqualTo(new decimal[] { 0.33m, 0.34m, 0.33m }));

                Assert.That(stock.ChildSecurities.Count, Is.EqualTo(3));
                if (stock.ChildSecurities.Count >= 1)
                {
                    Assert.That(stock.ChildSecurities[0].ASXCode, Is.EqualTo("ABC_1"));
                    Assert.That(stock.ChildSecurities[0].Name, Is.EqualTo("Child 1"));
                    Assert.That(stock.ChildSecurities[0].Trust, Is.EqualTo(true));
                }

                if (stock.ChildSecurities.Count >= 2)
                {
                    Assert.That(stock.ChildSecurities[1].ASXCode, Is.EqualTo("ABC_2"));
                    Assert.That(stock.ChildSecurities[1].Name, Is.EqualTo("Child 2"));
                    Assert.That(stock.ChildSecurities[1].Trust, Is.EqualTo(false));
                }

                if (stock.ChildSecurities.Count >= 3)
                {
                    Assert.That(stock.ChildSecurities[2].ASXCode, Is.EqualTo("ABC_3"));
                    Assert.That(stock.ChildSecurities[2].Name, Is.EqualTo("Child 3"));
                    Assert.That(stock.ChildSecurities[2].Trust, Is.EqualTo(true));
                }
            });
        }

        [TestCase]
        public void ListWhenAlreadyListed()
        {
            var listingDate = new Date(2000, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", listingDate, AssetCategory.AustralianProperty, new StapledSecurityChild[0]);

            Assert.That(() => stock.List("XYZ", "XYZ Pty Ltd", listingDate, AssetCategory.AustralianProperty, new StapledSecurityChild[0]), Throws.TypeOf(typeof(EffectiveDateException)));
        }


        [TestCase]
        public void ListWhenDelisted()
        {
            var listingDate = new Date(2000, 01, 01);
            var delistingDate = new Date(2002, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", listingDate, AssetCategory.AustralianProperty, new StapledSecurityChild[0]);
            stock.DeList(delistingDate);

            Assert.That(() => stock.List("XYZ", "XYZ Pty Ltd", listingDate, AssetCategory.AustralianProperty, new StapledSecurityChild[0]), Throws.TypeOf(typeof(EffectiveDateException)));
        }

        [TestCase]
        public void DeList()
        {
            var listingDate = new Date(2000, 01, 01);
            var delistingDate = new Date(2002, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", listingDate, AssetCategory.AustralianProperty, new StapledSecurityChild[0]);
            stock.DeList(delistingDate);

            Assert.Multiple(() =>
            {
                Assert.That(stock.Trust, Is.False);

                Assert.That(stock.EffectivePeriod.FromDate, Is.EqualTo(listingDate));
                Assert.That(stock.EffectivePeriod.ToDate, Is.EqualTo(delistingDate));

                var propertyValues = stock.Properties.Values.ToList();
                Assert.That(propertyValues.Last().EffectivePeriod.ToDate, Is.EqualTo(delistingDate));

                var dividendRules = stock.DividendRules.Values.ToList();
                Assert.That(dividendRules.Last().EffectivePeriod.ToDate, Is.EqualTo(delistingDate));

                var ntas = stock.RelativeNTAs.Values.ToList();
                Assert.That(ntas.Last().EffectivePeriod.ToDate, Is.EqualTo(delistingDate));
            });
        }

        [TestCase]
        public void DeListWithoutBeingListed()
        {
            var listingDate = new Date(2000, 01, 01);
            var delistingDate = new Date(2002, 01, 01);

            var stock = new StapledSecurity(Guid.NewGuid());

            Assert.That(() => stock.DeList(delistingDate), Throws.TypeOf(typeof(EffectiveDateException)));

        }

        [TestCase]
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

            Assert.That(() => stock.SetRelativeNTAs(changeDate, new decimal[] {0.50m, 0.50m}), Throws.TypeOf(typeof(EffectiveDateException)));
        }

        [TestCase]
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

            Assert.That(() => stock.SetRelativeNTAs(changeDate, new decimal[] { 0.50m, 0.50m }), Throws.TypeOf(typeof(EffectiveDateException)));
        }

        [TestCase]
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

            Assert.Multiple(() =>
            {
                Assert.That(stock.RelativeNTAs[changeDate].Percentages, Is.EqualTo(new decimal[] { 0.60m, 0.40m }));
            });
        }

        [TestCase]
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



            Assert.That(() => stock.SetRelativeNTAs(changeDate, new decimal[] { 1.00m }), Throws.TypeOf(typeof(ArgumentException)));
        }

        [TestCase]
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



            Assert.That(() => stock.SetRelativeNTAs(changeDate, new decimal[] { 0.30m, 0.50m, 0.10m, 0.10m }), Throws.TypeOf(typeof(ArgumentException)));
        }

        [TestCase]
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


            Assert.That(() => stock.SetRelativeNTAs(changeDate, new decimal[] { 0.60m, 0.60m }), Throws.TypeOf(typeof(ArgumentException)));
        }

        [TestCase]
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

            Assert.Multiple(() =>
            {
                Assert.That(stock.RelativeNTAs[changeDate.AddDays(-1)].Percentages, Is.EqualTo(new decimal[] { 0.50m, 0.50m }));

                Assert.That(stock.RelativeNTAs[changeDate].Percentages, Is.EqualTo(new decimal[] { 0.60m, 0.40m }));
            });
        }
    }
}
