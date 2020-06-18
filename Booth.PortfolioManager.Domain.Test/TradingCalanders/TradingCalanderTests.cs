using System;
using System.Linq;

using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;

using Booth.Common;
using Booth.EventStore;
using Booth.PortfolioManager.Domain.TradingCalanders;
using Booth.PortfolioManager.Domain.TradingCalanders.Events;

namespace Booth.PortfolioManager.Domain.Test.TradingCalanders
{
    public class TradingCalanderTests
    {

        [Fact]
        public void SetNewTradingDays()
        {
            var tradingCalander = new TradingCalander(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            tradingCalander.SetNonTradingDays(2019, nonTradingDays);

            var events = tradingCalander.FetchEvents().ToList();

            events.Should().SatisfyRespectively(

                first => first.Should().BeOfType<NonTradingDaysSetEvent>().Which.NonTradingDays.Should().SatisfyRespectively(
                    day1 => day1.Should().BeEquivalentTo(new { Date = new Date(2019, 01, 01), Description = "New Years Day" }),
                    day2 => day2.Should().BeEquivalentTo(new { Date = new Date(2019, 12, 25), Description = "Christmas Day" })
                    )
               );
        }

        [Fact]
        public void SetNewTradingDaysForInvalidYear()
        {
            var tradingCalander = new TradingCalander(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2020, 12, 25), "Christmas Day")
            };

            Action a = () => tradingCalander.SetNonTradingDays(2019, nonTradingDays);

            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ApplyNonTradingDaysSetEvent()
        {
            var tradingCalander = new TradingCalander(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            var @event = new NonTradingDaysSetEvent(tradingCalander.Id, 0, 2019, nonTradingDays);

            tradingCalander.ApplyEvents(new Event[] { @event });

            using (new AssertionScope())
            {
                tradingCalander.IsTradingDay(new Date(2019, 01, 01)).Should().BeFalse();
                tradingCalander.IsTradingDay(new Date(2019, 12, 25)).Should().BeFalse();
                tradingCalander.IsTradingDay(new Date(2019, 01, 02)).Should().BeTrue();
            }
        }

        [Fact]
        public void ApplyNonTradingDaysSetEventReplaceExisting()
        {
            var tradingCalander = new TradingCalander(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            var @event1 = new NonTradingDaysSetEvent(tradingCalander.Id, 0, 2019, nonTradingDays);

            var nonTradingDays2 = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 02), "Still Hungover")
            };
            var @event2 = new NonTradingDaysSetEvent(tradingCalander.Id, 0, 2019, nonTradingDays2);

            tradingCalander.ApplyEvents(new Event[] { @event1, @event2 });

            using (new AssertionScope())
            { 
                tradingCalander.IsTradingDay(new Date(2019, 01, 01)).Should().BeTrue();
                tradingCalander.IsTradingDay(new Date(2019, 12, 25)).Should().BeTrue();
                tradingCalander.IsTradingDay(new Date(2019, 01, 02)).Should().BeFalse();
            }
        }


        [Fact]
        public void RetriveNonTradingDaysForYear()
        {
            var tradingCalander = new TradingCalander(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                    new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                    new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
                };
            tradingCalander.SetNonTradingDays(2019, nonTradingDays);

            tradingCalander.NonTradingDays(2019).Should().HaveCount(2);
        }

        [Fact]
        public void RetriveNonTradingDaysForYearWithNoData()
        {
            var tradingCalander = new TradingCalander(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                    new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                    new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
                };
            tradingCalander.SetNonTradingDays(2019, nonTradingDays);

            tradingCalander.NonTradingDays(2020).Should().BeEmpty();
        }

        [Fact]
        public void CheckIfDateIsATradingDayReturnTrue()
        {
            var tradingCalander = new TradingCalander(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            var @event = new NonTradingDaysSetEvent(tradingCalander.Id, 0, 2019, nonTradingDays);

            tradingCalander.ApplyEvents(new Event[] { @event });

            tradingCalander.IsTradingDay(new Date(2019, 01, 02)).Should().BeTrue();
        }

        [Fact]
        public void CheckIfDateIsAtradingDayReturnFalse()
        {
            var tradingCalander = new TradingCalander(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            var @event = new NonTradingDaysSetEvent(tradingCalander.Id, 0, 2019, nonTradingDays);

            tradingCalander.ApplyEvents(new Event[] { @event });

            tradingCalander.IsTradingDay(new Date(2019, 01, 01)).Should().BeFalse();
        }

        [Fact]
        public void EnumerateTradingDays()
        {
            var tradingCalander = new TradingCalander(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                    new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                    new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
                };
            tradingCalander.SetNonTradingDays(2019, nonTradingDays);

            var tradingDays = tradingCalander.TradingDays(new DateRange(new Date(2019, 01, 01), new Date(2019, 01, 10))).ToList();

            tradingDays.Should().HaveCount(7).And.BeInAscendingOrder().And.OnlyHaveUniqueItems();           
        }
    }
}
