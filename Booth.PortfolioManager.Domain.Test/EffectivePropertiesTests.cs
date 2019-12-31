using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Booth.Common;
using Booth.PortfolioManager.Domain;

namespace Booth.PortfolioManager.Domain.Test
{
    class EffectivePropertiesTests
    {
        [TestCase]
        public void ChangeWithNoExistingProperties()
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            Assert.Multiple(() =>
            {
                Assert.That(properties.Values.Count(), Is.EqualTo(1));
                Assert.That(properties.Values.First().EffectivePeriod.FromDate, Is.EqualTo(start));
                Assert.That(properties.Values.First().EffectivePeriod.ToDate, Is.EqualTo(Date.MaxValue));
                Assert.That(properties.Values.First().Properties.Value, Is.EqualTo("InitialValue"));
            });
        }


        [TestCase]
        public void ChangeWithExistingProperty()
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            var change = new Date(2019, 12, 31);
            properties.Change(change, new EffectivePropertyTestClass("Change1"));

            Assert.Multiple(() =>
            {
                Assert.That(properties.Values.Count(), Is.EqualTo(2));
                Assert.That(properties.Values.Last().EffectivePeriod.FromDate, Is.EqualTo(start));
                Assert.That(properties.Values.Last().EffectivePeriod.ToDate, Is.EqualTo(change.AddDays(-1)));
                Assert.That(properties.Values.Last().Properties.Value, Is.EqualTo("InitialValue"));

                Assert.That(properties.Values.First().EffectivePeriod.FromDate, Is.EqualTo(change));
                Assert.That(properties.Values.First().EffectivePeriod.ToDate, Is.EqualTo(Date.MaxValue));
                Assert.That(properties.Values.First().Properties.Value, Is.EqualTo("Change1"));
            });
        }

        [TestCase]
        public void ChangeOnSameDateAsExistingProperty()
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            var change = new Date(2019, 12, 31);
            properties.Change(change, new EffectivePropertyTestClass("Change1"));

            properties.Change(change, new EffectivePropertyTestClass("Change2"));

            Assert.Multiple(() =>
            {
                Assert.That(properties.Values.Count(), Is.EqualTo(2));
                Assert.That(properties.Values.Last().EffectivePeriod.FromDate, Is.EqualTo(start));
                Assert.That(properties.Values.Last().EffectivePeriod.ToDate, Is.EqualTo(change.AddDays(-1)));
                Assert.That(properties.Values.Last().Properties.Value, Is.EqualTo("InitialValue"));

                Assert.That(properties.Values.First().EffectivePeriod.FromDate, Is.EqualTo(change));
                Assert.That(properties.Values.First().EffectivePeriod.ToDate, Is.EqualTo(Date.MaxValue));
                Assert.That(properties.Values.First().Properties.Value, Is.EqualTo("Change2"));
            });
        }

        [TestCase]
        public void ChangeNonCurrentProperty()
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            var change1 = new Date(2019, 12, 31);
            properties.Change(change1, new EffectivePropertyTestClass("Change1"));

            var change2 = new Date(2019, 12, 15);
            Assert.That(() => properties.Change(change2, new EffectivePropertyTestClass("Change2")), Throws.TypeOf(typeof(EffectiveDateException)));
        }

        [TestCase]
        public void ChangeOutsideOfEffectivePeriod()
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            var change1 = new Date(2019, 12, 31);
            properties.Change(change1, new EffectivePropertyTestClass("Change1"));

            var change2 = new Date(2019, 11, 15);
            Assert.That(() => properties.Change(change2, new EffectivePropertyTestClass("Change2")), Throws.TypeOf(typeof(EffectiveDateException)));

        }

        [TestCase]
        public void EndWithNoExistingProperties()
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var end = new Date(2019, 11, 15);
            Assert.That(() => properties.End(end), Throws.TypeOf(typeof(EffectiveDateException)));
        }

        [TestCase]
        public void EndWithProperties()
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            var change = new Date(2019, 12, 31);
            properties.Change(change, new EffectivePropertyTestClass("Change1"));

            var end = new Date(2020, 01, 15);
            properties.End(end);

            Assert.Multiple(() =>
            {
                Assert.That(properties.Values.Count(), Is.EqualTo(2));
                Assert.That(properties.Values.Last().EffectivePeriod.FromDate, Is.EqualTo(start));
                Assert.That(properties.Values.Last().EffectivePeriod.ToDate, Is.EqualTo(change.AddDays(-1)));
                Assert.That(properties.Values.Last().Properties.Value, Is.EqualTo("InitialValue"));

                Assert.That(properties.Values.First().EffectivePeriod.FromDate, Is.EqualTo(change));
                Assert.That(properties.Values.First().EffectivePeriod.ToDate, Is.EqualTo(end));
                Assert.That(properties.Values.First().Properties.Value, Is.EqualTo("Change1"));
            });
        }

        [TestCase]
        public void EndOnStartDate()
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            properties.End(start);

            Assert.Multiple(() =>
            {
                Assert.That(properties.Values.Count(), Is.EqualTo(1));
                Assert.That(properties.Values.First().EffectivePeriod.FromDate, Is.EqualTo(start));
                Assert.That(properties.Values.First().EffectivePeriod.ToDate, Is.EqualTo(start));
                Assert.That(properties.Values.First().Properties.Value, Is.EqualTo("InitialValue"));
            });
        }

        [TestCase]
        public void EndInNonCurrentPeriod()
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            var change1 = new Date(2019, 12, 15);
            properties.Change(change1, new EffectivePropertyTestClass("Change1"));

            var change2 = new Date(2019, 12, 31);
            properties.Change(change2, new EffectivePropertyTestClass("Change2"));

            var end = new Date(2019, 12, 17);
            Assert.That(() => properties.End(end), Throws.TypeOf(typeof(EffectiveDateException)));
        }

        [TestCase]
        public void EndAnAlreadyEndedProperty()
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            var change1 = new Date(2019, 12, 15);
            properties.Change(change1, new EffectivePropertyTestClass("Change1"));

            var change2 = new Date(2019, 12, 31);
            properties.Change(change2, new EffectivePropertyTestClass("Change2"));

            var end = new Date(2020, 01, 17);
            properties.End(end);

            Assert.That(() => properties.End(end), Throws.TypeOf(typeof(EffectiveDateException)));
        }


        [TestCaseSource(nameof(PropertyAtDateData))]
        public void PropertyAtDate(Date date, string result, bool exception)
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            var change1 = new Date(2019, 12, 15);
            properties.Change(change1, new EffectivePropertyTestClass("Change1"));

            var change2 = new Date(2019, 12, 31);
            properties.Change(change2, new EffectivePropertyTestClass("Change2"));

            var end = new Date(2020, 01, 17);
            properties.End(end);

            if (!exception)
                Assert.That(properties[date].Value, Is.EqualTo(result));
            else
                Assert.That(() => properties[date], Throws.TypeOf(typeof(KeyNotFoundException)));
        }

        static IEnumerable<object[]> PropertyAtDateData()
        {
            yield return new object[] { new Date(2019, 11, 01), "", true };
            yield return new object[] { new Date(2019, 12, 01), "InitialValue", false };
            yield return new object[] { new Date(2019, 12, 03), "InitialValue", false };
            yield return new object[] { new Date(2019, 12, 15), "Change1", false };
            yield return new object[] { new Date(2019, 12, 17), "Change1", false };
            yield return new object[] { new Date(2019, 12, 30), "Change1", false };
            yield return new object[] { new Date(2019, 12, 31), "Change2", false };
            yield return new object[] { new Date(2020, 01, 01), "Change2", false };
            yield return new object[] { new Date(2020, 01, 17), "Change2", false };
            yield return new object[] { new Date(2020, 01, 20), "", true };
        }

        [TestCaseSource(nameof(ClosestToDateData))]
        public void ClosestToDate(Date date, string result)
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            var change1 = new Date(2019, 12, 15);
            properties.Change(change1, new EffectivePropertyTestClass("Change1"));

            var change2 = new Date(2019, 12, 31);
            properties.Change(change2, new EffectivePropertyTestClass("Change2"));

            var end = new Date(2020, 01, 17);
            properties.End(end);

            Assert.That(properties.ClosestTo(date).Value, Is.EqualTo(result));
        }

        static IEnumerable<object[]> ClosestToDateData()
        {
            yield return new object[] { new Date(2019, 11, 01), "InitialValue" };
            yield return new object[] { new Date(2019, 12, 01), "InitialValue" };
            yield return new object[] { new Date(2019, 12, 03), "InitialValue" };
            yield return new object[] { new Date(2019, 12, 15), "Change1" };
            yield return new object[] { new Date(2019, 12, 17), "Change1" };
            yield return new object[] { new Date(2019, 12, 30), "Change1" };
            yield return new object[] { new Date(2019, 12, 31), "Change2" };
            yield return new object[] { new Date(2020, 01, 01), "Change2" };
            yield return new object[] { new Date(2020, 01, 17), "Change2" };
            yield return new object[] { new Date(2020, 01, 20), "Change2" };
        }

        [TestCaseSource(nameof(MatchesNoDateData))]
        public void MatchesNoDate(string value, bool result)
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            var change1 = new Date(2019, 12, 15);
            properties.Change(change1, new EffectivePropertyTestClass("Change1"));

            var change2 = new Date(2019, 12, 31);
            properties.Change(change2, new EffectivePropertyTestClass("Change2"));

            var end = new Date(2020, 01, 17);
            properties.End(end);

            Assert.That(properties.Matches(x => x.Value == value), Is.EqualTo(result));
        }

        static IEnumerable<object[]> MatchesNoDateData()
        {
            yield return new object[] { "InitialValue", true };
            yield return new object[] { "Change1", true };
            yield return new object[] { "Change2", true };
            yield return new object[] { "Change3", false };
        }

        [TestCaseSource(nameof(MatchesWithDateData))]
        public void MatchesWithDate(Date date, string value, bool result)
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            var change1 = new Date(2019, 12, 15);
            properties.Change(change1, new EffectivePropertyTestClass("Change1"));

            var change2 = new Date(2019, 12, 31);
            properties.Change(change2, new EffectivePropertyTestClass("Change2"));

            var end = new Date(2020, 01, 17);
            properties.End(end);

            Assert.That(properties.Matches(date, x => x.Value == value), Is.EqualTo(result));
        }

        static IEnumerable<object[]> MatchesWithDateData()
        {
            yield return new object[] { new Date(2019, 11, 01), "InitialValue", false };
            yield return new object[] { new Date(2019, 12, 01), "InitialValue", true };
            yield return new object[] { new Date(2019, 12, 03), "InitialValue", true };
            yield return new object[] { new Date(2019, 12, 15), "Change1", true };
            yield return new object[] { new Date(2019, 12, 17), "Change1", true };
            yield return new object[] { new Date(2019, 12, 17), "InitialValue", false };
            yield return new object[] { new Date(2019, 12, 30), "Change1", true };
            yield return new object[] { new Date(2019, 12, 31), "Change1", false };
            yield return new object[] { new Date(2020, 01, 01), "Change2", true };
            yield return new object[] { new Date(2020, 01, 01), "Change1", false };
            yield return new object[] { new Date(2020, 01, 17), "Change2", true };
            yield return new object[] { new Date(2020, 01, 20), "Change2", false };
        }


        [TestCaseSource(nameof(MatchesWithDateRangeData))]
        public void MatchesWithDateRange(DateRange dateRange, string value, bool result)
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            var change1 = new Date(2019, 12, 15);
            properties.Change(change1, new EffectivePropertyTestClass("Change1"));

            var change2 = new Date(2019, 12, 31);
            properties.Change(change2, new EffectivePropertyTestClass("Change2"));

            var end = new Date(2020, 01, 17);
            properties.End(end);

            Assert.That(properties.Matches(dateRange, x => x.Value == value), Is.EqualTo(result));
        }

        static IEnumerable<object[]> MatchesWithDateRangeData()
        {
            yield return new object[] { new DateRange(new Date(2019, 11, 01), new Date(2020, 01, 20)), "InitialValue", true };
            yield return new object[] { new DateRange(new Date(2019, 11, 01), new Date(2020, 01, 20)), "Change1", true };
            yield return new object[] { new DateRange(new Date(2019, 11, 01), new Date(2020, 01, 20)), "Change2", true };
            yield return new object[] { new DateRange(new Date(2019, 11, 01), new Date(2020, 01, 20)), "Change3", false };

            yield return new object[] { new DateRange(new Date(2019, 12, 15), new Date(2019, 12, 17)), "InitialValue", false };
            yield return new object[] { new DateRange(new Date(2019, 12, 15), new Date(2019, 12, 17)), "Change1", true };
            yield return new object[] { new DateRange(new Date(2019, 12, 15), new Date(2019, 12, 17)), "Change2", false };

            yield return new object[] { new DateRange(new Date(2019, 12, 31), new Date(2020, 01, 10)), "InitialValue", false };
            yield return new object[] { new DateRange(new Date(2019, 12, 31), new Date(2020, 01, 10)), "Change1", false };
            yield return new object[] { new DateRange(new Date(2019, 12, 31), new Date(2020, 01, 10)), "Change2", true };

            yield return new object[] { new DateRange(new Date(2019, 11, 01), new Date(2019, 11, 10)), "InitialValue", false };
            yield return new object[] { new DateRange(new Date(2019, 11, 01), new Date(2019, 11, 10)), "Change1", false };
            yield return new object[] { new DateRange(new Date(2019, 11, 01), new Date(2019, 11, 10)), "Change2", false };
        }

    }

    // Class for testing EffectiveProperties
    struct EffectivePropertyTestClass
    {
        public string Value { get; set; }

        public EffectivePropertyTestClass(string value)
        {
            Value = value;
        }
    }
}
