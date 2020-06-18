using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Domain;

namespace Booth.PortfolioManager.Domain.Test
{
    public class EffectivePropertiesTests
    {
        [Fact]
        public void ChangeWithNoExistingProperties()
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            properties.Values.Should().SatisfyRespectively(
                first =>
                {
                    first.EffectivePeriod.Should().Be(new DateRange(start, Date.MaxValue));
                    first.Properties.Value.Should().Be("InitialValue");
                }
            );
        }


        [Fact]
        public void ChangeWithExistingProperty()
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            var change = new Date(2019, 12, 31);
            properties.Change(change, new EffectivePropertyTestClass("Change1"));

            properties.Values.Should().SatisfyRespectively(
                first =>
                {
                    first.EffectivePeriod.Should().Be(new DateRange(change, Date.MaxValue));
                    first.Properties.Value.Should().Be("Change1");
                },
                second =>
                {
                    second.EffectivePeriod.Should().Be(new DateRange(start, change.AddDays(-1)));
                    second.Properties.Value.Should().Be("InitialValue");
                }
            );
        }

        [Fact]
        public void ChangeOnSameDateAsExistingProperty()
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            var change = new Date(2019, 12, 31);
            properties.Change(change, new EffectivePropertyTestClass("Change1"));

            properties.Change(change, new EffectivePropertyTestClass("Change2"));

            properties.Values.Should().SatisfyRespectively(
                first =>
                {
                    first.EffectivePeriod.Should().Be(new DateRange(change, Date.MaxValue));
                    first.Properties.Value.Should().Be("Change2");
                },
                second =>
                {
                    second.EffectivePeriod.Should().Be(new DateRange(start, change.AddDays(-1)));
                    second.Properties.Value.Should().Be("InitialValue");
                }
            );
        }

        [Fact]
        public void ChangeNonCurrentProperty()
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            var change1 = new Date(2019, 12, 31);
            properties.Change(change1, new EffectivePropertyTestClass("Change1"));

            var change2 = new Date(2019, 12, 15);

            Action a = () => properties.Change(change2, new EffectivePropertyTestClass("Change2"));
            
            a.Should().Throw<EffectiveDateException>();
        }

        [Fact]
        public void ChangeOutsideOfEffectivePeriod()
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            var change1 = new Date(2019, 12, 31);
            properties.Change(change1, new EffectivePropertyTestClass("Change1"));

            var change2 = new Date(2019, 11, 15);
            Action a = () => properties.Change(change2, new EffectivePropertyTestClass("Change2"));

            a.Should().Throw<EffectiveDateException>();
        }

        [Fact]
        public void EndWithNoExistingProperties()
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var end = new Date(2019, 11, 15);
            Action a = () => properties.End(end);

            a.Should().Throw<EffectiveDateException>();
        }

        [Fact]
        public void EndWithProperties()
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            var change = new Date(2019, 12, 31);
            properties.Change(change, new EffectivePropertyTestClass("Change1"));

            var end = new Date(2020, 01, 15);
            properties.End(end);

            properties.Values.Should().SatisfyRespectively(
                first =>
                {
                    first.EffectivePeriod.Should().Be(new DateRange(change, end));
                    first.Properties.Value.Should().Be("Change1");
                },
                second =>
                {
                    second.EffectivePeriod.Should().Be(new DateRange(start, change.AddDays(-1)));
                    second.Properties.Value.Should().Be("InitialValue");
                }
            );
        }

        [Fact]
        public void EndOnStartDate()
        {
            var properties = new EffectiveProperties<EffectivePropertyTestClass>();

            var start = new Date(2019, 12, 01);
            properties.Change(start, new EffectivePropertyTestClass("InitialValue"));

            properties.End(start);

            properties.Values.Should().SatisfyRespectively(
                first =>
                {
                    first.EffectivePeriod.Should().Be(new DateRange(start, start));
                    first.Properties.Value.Should().Be("InitialValue");
                }
            );
        }

        [Fact]
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
            Action a = () => properties.End(end);

            a.Should().Throw<EffectiveDateException>();
        }

        [Fact]
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

            Action a = () => properties.End(end);

            a.Should().Throw<EffectiveDateException>();
        }


        [Theory]
        [MemberData(nameof(PropertyAtDateData))]
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
                properties[date].Value.Should().Be(result);
            else
            {
                Action a = () => { var x = properties[date]; };
                a.Should().Throw<KeyNotFoundException>();
            }
                
        }

        public static IEnumerable<object[]> PropertyAtDateData()
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

        [Fact]
        public void Value()
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

            properties.Value(new Date(2019, 12, 17)).EffectivePeriod.Should().Be(new DateRange(new Date(2019, 12, 15), new Date(2019, 12, 30)));
        }

        [Theory]
        [MemberData(nameof(ClosestToDateData))]
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

            properties.ClosestTo(date).Value.Should().Be(result);
        }

        public static IEnumerable<object[]> ClosestToDateData()
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

        [Theory]
        [MemberData(nameof(MatchesNoDateData))]
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

            properties.Matches(x => x.Value == value).Should().Be(result);
        }

        public static IEnumerable<object[]> MatchesNoDateData()
        {
            yield return new object[] { "InitialValue", true };
            yield return new object[] { "Change1", true };
            yield return new object[] { "Change2", true };
            yield return new object[] { "Change3", false };
        }

        [Theory]
        [MemberData(nameof(MatchesWithDateData))]
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

            properties.Matches(date, x => x.Value == value).Should().Be(result);
        }

        public static IEnumerable<object[]> MatchesWithDateData()
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


        [Theory]
        [MemberData(nameof(MatchesWithDateRangeData))]
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

            properties.Matches(dateRange, x => x.Value == value).Should().Be(result);
        }

        public static IEnumerable<object[]> MatchesWithDateRangeData()
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
