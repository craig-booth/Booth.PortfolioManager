using System;
using System.Linq;

using NUnit.Framework;

using Booth.Common;
using Booth.EventStore;
using Booth.PortfolioManager.Domain.TradingCalanders;
using Booth.PortfolioManager.Domain.TradingCalanders.Events;

namespace Booth.PortfolioManager.Domain.Test.TradingCalanders
{
    class TradingCalanderTests
    {

        [TestCase]
        public void SetNewTradingDays()
        {
            var tradingCalander = new TradingCalander(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            tradingCalander.SetNonTradingDays(2019, nonTradingDays);

            var events = tradingCalander.FetchEvents().ToList();

            Assert.Multiple(() => {
                Assert.That(events, Has.Count.EqualTo(1));
                var e1 = events[0] as NonTradingDaysSetEvent;
                Assert.That(e1.NonTradingDays.Count(), Is.EqualTo(2));
                Assert.That(e1.NonTradingDays[0].Date, Is.EqualTo(new Date(2019, 01, 01)));
                Assert.That(e1.NonTradingDays[0].Desciption, Is.EqualTo("New Years Day"));
                Assert.That(e1.NonTradingDays[1].Date, Is.EqualTo(new Date(2019, 12, 25)));
                Assert.That(e1.NonTradingDays[1].Desciption, Is.EqualTo("Christmas Day"));
            });
        }

        [TestCase]
        public void SetNewTradingDaysForInvalidYear()
        {
            var tradingCalander = new TradingCalander(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2020, 12, 25), "Christmas Day")
            };

            Assert.That(() => tradingCalander.SetNonTradingDays(2019, nonTradingDays), Throws.InstanceOf(typeof(ArgumentException)));
        }

        [TestCase]
        public void ApplyNonTradingDaysSetEvent()
        {
            var tradingCalander = new TradingCalander(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            var @event = new NonTradingDaysSetEvent(tradingCalander.Id, 0, 2019, nonTradingDays);

            tradingCalander.Apply(@event);

            Assert.Multiple(() => {
                Assert.That(tradingCalander.IsTradingDay(new Date(2019, 01, 01)), Is.False);
                Assert.That(tradingCalander.IsTradingDay(new Date(2019, 12, 25)), Is.False);
                Assert.That(tradingCalander.IsTradingDay(new Date(2019, 01, 02)), Is.True);
            });
        }

        [TestCase]
        public void ApplyNonTradingDaysSetEventReplaceExisting()
        {
            var tradingCalander = new TradingCalander(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            var @event1 = new NonTradingDaysSetEvent(tradingCalander.Id, 0, 2019, nonTradingDays);
            tradingCalander.Apply(@event1);

            var nonTradingDays2 = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 02), "Still Hungover")
            };
            var @event2 = new NonTradingDaysSetEvent(tradingCalander.Id, 0, 2019, nonTradingDays2);
            tradingCalander.Apply(@event2);

            Assert.Multiple(() => {
                Assert.That(tradingCalander.IsTradingDay(new Date(2019, 01, 01)), Is.True);
                Assert.That(tradingCalander.IsTradingDay(new Date(2019, 12, 25)), Is.True);
                Assert.That(tradingCalander.IsTradingDay(new Date(2019, 01, 02)), Is.False);
            });
        }


        [TestCase]
        public void RetriveNonTradingDaysForYear()
        {
            var tradingCalander = new TradingCalander(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                    new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                    new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
                };
            tradingCalander.SetNonTradingDays(2019, nonTradingDays);

            Assert.That(tradingCalander.NonTradingDays(2019).Count(), Is.EqualTo(2));
        }

        [TestCase]
        public void RetriveNonTradingDaysForYearWithNoData()
        {
            var tradingCalander = new TradingCalander(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                    new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                    new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
                };
            tradingCalander.SetNonTradingDays(2019, nonTradingDays);

            Assert.That(tradingCalander.NonTradingDays(2020), Is.Empty);
        }

        [TestCase]
        public void CheckIfDateIsATradingDayReturnTrue()
        {
            var tradingCalander = new TradingCalander(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            var @event = new NonTradingDaysSetEvent(tradingCalander.Id, 0, 2019, nonTradingDays);

            tradingCalander.Apply(@event);

            Assert.That(tradingCalander.IsTradingDay(new Date(2019, 01, 02)), Is.True);
        }

        [TestCase]
        public void CheckIfDateIsAtradingDayReturnFalse()
        {
            var tradingCalander = new TradingCalander(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
            };
            var @event = new NonTradingDaysSetEvent(tradingCalander.Id, 0, 2019, nonTradingDays);

            tradingCalander.Apply(@event);

            Assert.That(tradingCalander.IsTradingDay(new Date(2019, 01, 01)), Is.False);
        }

        [TestCase]
        public void EnumerateTradingDays()
        {
            var tradingCalander = new TradingCalander(Guid.NewGuid());

            var nonTradingDays = new NonTradingDay[] {
                    new NonTradingDay(new Date(2019, 01, 01), "New Years Day"),
                    new NonTradingDay(new Date(2019, 12, 25), "Christmas Day")
                };
            tradingCalander.SetNonTradingDays(2019, nonTradingDays);

            var tradingDays = tradingCalander.TradingDays(new DateRange(new Date(2019, 01, 01), new Date(2019, 01, 10))).ToList();
            Assert.Multiple(() =>
            {
                Assert.That(tradingDays, Has.Count.EqualTo(7));
                Assert.That(tradingDays, Is.Ordered);
                Assert.That(tradingDays, Is.Unique);
            });
            
        }
    }
}
