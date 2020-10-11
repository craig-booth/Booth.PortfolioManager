using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using Moq;
using FluentAssertions;
using FluentAssertions.Execution;

using Booth.Common;
using Booth.EventStore;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Domain.TradingCalendars;

namespace Booth.PortfolioManager.Web.Test.Services
{
    public class TradingCalendarServiceTests
    {

        [Fact]
        public void CreateServiceForExistingCalendar()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var tradingCalendar = new TradingCalendar(Guid.NewGuid());
            tradingCalendar.SetNonTradingDays(2000, new[] { new NonTradingDay(new Date(2000, 01, 01), "New Year's Day") });
            var repository = mockRepository.Create<IRepository<TradingCalendar>>();
            repository.Setup(x => x.Get(tradingCalendar.Id)).Returns(tradingCalendar);

            var service = new TradingCalendarService(repository.Object, tradingCalendar.Id);

            service.TradingCalendar.Should().Be(tradingCalendar);

            mockRepository.Verify();
        }

        [Fact]
        public void CreateServiceForNewCalendar()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var repository = mockRepository.Create<IRepository<TradingCalendar>>();
            repository.Setup(x => x.Get(id)).Returns(default(TradingCalendar));
            repository.Setup(x => x.Add(It.Is<TradingCalendar>(y => y.Id == id)));

            var service = new TradingCalendarService(repository.Object, id);

            mockRepository.Verify();
        }

        [Fact]
        public void GetYearNotExists()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var tradingCalendar = new TradingCalendar(Guid.NewGuid());
            tradingCalendar.SetNonTradingDays(2000, new[] { new NonTradingDay(new Date(2000, 01, 01), "New Year's Day") });
            var repository = mockRepository.Create<IRepository<TradingCalendar>>();
            repository.Setup(x => x.Get(tradingCalendar.Id)).Returns(tradingCalendar);

            var service = new TradingCalendarService(repository.Object, tradingCalendar.Id);

            var result = service.Get(2010);
            var response = result.Result;

            response.Should().BeEquivalentTo(new
            {
                Year = 2010,
                NonTradingDays = new NonTradingDay[] { },
            });
        }

        [Fact]
        public void GetYearExists()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var tradingCalendar = new TradingCalendar(Guid.NewGuid());
            tradingCalendar.SetNonTradingDays(2000, new[] { new NonTradingDay(new Date(2000, 01, 01), "New Year's Day") });
            var repository = mockRepository.Create<IRepository<TradingCalendar>>();
            repository.Setup(x => x.Get(tradingCalendar.Id)).Returns(tradingCalendar);

            var service = new TradingCalendarService(repository.Object, tradingCalendar.Id);

            var result = service.Get(2000);
            var response = result.Result;

            response.Should().BeEquivalentTo(new
            {
                Year = 2000,
                NonTradingDays = new [] 
                { 
                    new RestApi.TradingCalendars.TradingCalendar.NonTradingDay() { Date = new Date(2000, 01, 01), Description = "New Year's Day"} 
                },
            });
        }

        [Fact]
        public void Update()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var tradingCalendar = new TradingCalendar(Guid.NewGuid());
            tradingCalendar.SetNonTradingDays(2000, new[] { new NonTradingDay(new Date(2000, 01, 01), "New Year's Day") });
            var repository = mockRepository.Create<IRepository<TradingCalendar>>();
            repository.Setup(x => x.Get(tradingCalendar.Id)).Returns(tradingCalendar);
            repository.Setup(x => x.Update(tradingCalendar));

            var service = new TradingCalendarService(repository.Object, tradingCalendar.Id);

            var calendarReference = service.TradingCalendar;

            using (new AssertionScope())
            {
                calendarReference.IsTradingDay(new Date(2000, 01, 01)).Should().BeFalse();
                calendarReference.IsTradingDay(new Date(2000, 12, 25)).Should().BeTrue();
            }

            var updatedTradingCalendar = new RestApi.TradingCalendars.TradingCalendar();
            updatedTradingCalendar.Year = 2000;
            updatedTradingCalendar.NonTradingDays.Add(new RestApi.TradingCalendars.TradingCalendar.NonTradingDay() { Date = new Date(2000, 12, 25), Description = "Christmas Day" });

            var result = service.Update(updatedTradingCalendar);

            using (new AssertionScope())
            {
                result.Should().HaveOkStatus();

                calendarReference.IsTradingDay(new Date(2000, 01, 01)).Should().BeTrue();
                calendarReference.IsTradingDay(new Date(2000, 12, 25)).Should().BeFalse();
            }

            mockRepository.Verify();
        }

        [Fact]
        public void SetNonTradingDays()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var tradingCalendar = new TradingCalendar(Guid.NewGuid());
            tradingCalendar.SetNonTradingDays(2000, new[] { new NonTradingDay(new Date(2000, 01, 01), "New Year's Day") });
            var repository = mockRepository.Create<IRepository<TradingCalendar>>();
            repository.Setup(x => x.Get(tradingCalendar.Id)).Returns(tradingCalendar);
            repository.Setup(x => x.Update(tradingCalendar));

            var service = new TradingCalendarService(repository.Object, tradingCalendar.Id);

            var calendarReference = service.TradingCalendar;

            using (new AssertionScope())
            {
                calendarReference.IsTradingDay(new Date(2000, 01, 01)).Should().BeFalse();
                calendarReference.IsTradingDay(new Date(2000, 12, 25)).Should().BeTrue();
            }

            service.SetNonTradingDays(2000, new[] { new NonTradingDay(new Date(2000, 12, 25), "Christmas Day") });

            using (new AssertionScope())
            {
                calendarReference.IsTradingDay(new Date(2000, 01, 01)).Should().BeTrue();
                calendarReference.IsTradingDay(new Date(2000, 12, 25)).Should().BeFalse();
            }

            mockRepository.Verify();
        }
    }
}
