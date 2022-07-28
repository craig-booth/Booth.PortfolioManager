using System;
using System.Linq;

using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;

using Booth.Common;
using Booth.PortfolioManager.Domain.TradingCalendars;

namespace Booth.PortfolioManager.Domain.Test.TradingCalendars
{
    public class TradingCalendarTests
    {

        [Fact]
        public void SetNewTradingDays()
        {
            var tradingCalendar = new TradingCalendar(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            tradingCalendar.SetNonTradingDays(2019, nonTradingDays);


            using (new AssertionScope())
            {
                tradingCalendar.IsTradingDay(new Date(2019, 01, 01)).Should().BeFalse();
                tradingCalendar.IsTradingDay(new Date(2019, 12, 25)).Should().BeFalse();
                tradingCalendar.IsTradingDay(new Date(2019, 01, 02)).Should().BeTrue();
            }
        }

        [Fact]
        public void SetNewTradingDaysForInvalidYear()
        {
            var tradingCalendar = new TradingCalendar(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2020, 12, 25), "Christmas Day")
            };

            Action a = () => tradingCalendar.SetNonTradingDays(2019, nonTradingDays);

            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void RetriveNonTradingDaysForYear()
        {
            var tradingCalendar = new TradingCalendar(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                    new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                    new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
                };
            tradingCalendar.SetNonTradingDays(2019, nonTradingDays);

            tradingCalendar.NonTradingDays(2019).Should().HaveCount(2);
        }

        [Fact]
        public void RetriveNonTradingDaysForYearWithNoData()
        {
            var tradingCalendar = new TradingCalendar(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                    new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                    new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
                };
            tradingCalendar.SetNonTradingDays(2019, nonTradingDays);

            tradingCalendar.NonTradingDays(2020).Should().BeEmpty();
        }

        [Fact]
        public void CheckIfDateIsATradingDayReturnTrue()
        {
            var tradingCalendar = new TradingCalendar(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            tradingCalendar.SetNonTradingDays(2019, nonTradingDays);

            tradingCalendar.IsTradingDay(new Date(2019, 01, 02)).Should().BeTrue();
        }

        [Fact]
        public void CheckIfDateIsAtradingDayReturnFalse()
        {
            var tradingCalendar = new TradingCalendar(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            tradingCalendar.SetNonTradingDays(2019, nonTradingDays);

            tradingCalendar.IsTradingDay(new Date(2019, 01, 01)).Should().BeFalse();
        }

        [Fact]
        public void CheckIfWeekEndIsATradingDay()
        {
            var tradingCalendar = new TradingCalendar(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            tradingCalendar.SetNonTradingDays(2019, nonTradingDays);

            tradingCalendar.IsTradingDay(new Date(2019, 11, 10)).Should().BeFalse();
        }

        [Fact]
        public void NextTradingDayForTradingDay()
        {
            var tradingCalendar = new TradingCalendar(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            tradingCalendar.SetNonTradingDays(2019, nonTradingDays);

            tradingCalendar.NextTradingDay(new Date(2019, 11, 08)).Should().Be(new Date(2019, 11, 08));
        }

        [Fact]
        public void NextTradingDayForNonTradingDay()
        {
            var tradingCalendar = new TradingCalendar(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            tradingCalendar.SetNonTradingDays(2019, nonTradingDays);

            tradingCalendar.NextTradingDay(new Date(2019, 01, 01)).Should().Be(new Date(2019, 01, 02));
        }

        [Fact]
        public void NextTradingDayForWeekEnd()
        {
            var tradingCalendar = new TradingCalendar(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            tradingCalendar.SetNonTradingDays(2019, nonTradingDays);

            tradingCalendar.NextTradingDay(new Date(2019, 11, 10)).Should().Be(new Date(2019, 11, 11));
        }

        [Fact]
        public void PreviousTradingDayForTradingDay()
        {
            var tradingCalendar = new TradingCalendar(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            tradingCalendar.SetNonTradingDays(2019, nonTradingDays);

            tradingCalendar.PreviousTradingDay(new Date(2019, 11, 08)).Should().Be(new Date(2019, 11, 08));
        }

        [Fact]
        public void PreviousTradingDayForNonTradingDay()
        {
            var tradingCalendar = new TradingCalendar(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            tradingCalendar.SetNonTradingDays(2019, nonTradingDays);

            tradingCalendar.PreviousTradingDay(new Date(2019, 01, 01)).Should().Be(new Date(2018, 12, 31));
        }

        [Fact]
        public void PreviousTradingDayForWeekEnd()
        {
            var tradingCalendar = new TradingCalendar(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            tradingCalendar.SetNonTradingDays(2019, nonTradingDays);

            tradingCalendar.PreviousTradingDay(new Date(2019, 11, 10)).Should().Be(new Date(2019, 11, 08));
        }

        [Fact]
        public void EnumerateTradingDays()
        {
            var tradingCalendar = new TradingCalendar(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                    new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                    new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
                };
            tradingCalendar.SetNonTradingDays(2019, nonTradingDays);

            var tradingDays = tradingCalendar.TradingDays(new DateRange(new Date(2019, 01, 01), new Date(2019, 01, 10))).ToList();

            tradingDays.Should().HaveCount(7).And.BeInAscendingOrder().And.OnlyHaveUniqueItems();           
        }
    }
}
