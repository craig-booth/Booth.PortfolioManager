using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using Moq;
using FluentAssertions;
using FluentAssertions.Execution;

using Booth.Common;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.Web.Utilities;

namespace Booth.PortfolioManager.Web.Test.Services
{
    public class TradingCalendarServiceTests
    {

        [Fact]
        public void GetYearCalanderNotExist()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var repository = mockRepository.Create<ITradingCalendarRepository>();
            repository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(default(TradingCalendar));

            var service = new TradingCalendarService(repository.Object);

            var result = service.Get(Guid.NewGuid(), 2010);

            result.Should().HaveNotFoundStatus();

            mockRepository.Verify();
        }

        [Fact]
        public void GetYearNotExists()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var tradingCalendar = new TradingCalendar(TradingCalendarIds.ASX);
            tradingCalendar.SetNonTradingDays(2000, new[] { new NonTradingDay(new Date(2000, 01, 01), "New Year's Day") });
            var repository = mockRepository.Create<ITradingCalendarRepository>();
            repository.Setup(x => x.Get(tradingCalendar.Id)).Returns(tradingCalendar);

            var service = new TradingCalendarService(repository.Object);

            var result = service.Get(tradingCalendar.Id, 2010);
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

            var tradingCalendar = new TradingCalendar(TradingCalendarIds.ASX);
            tradingCalendar.SetNonTradingDays(2000, new[] { new NonTradingDay(new Date(2000, 01, 01), "New Year's Day") });
            var repository = mockRepository.Create<ITradingCalendarRepository>();
            repository.Setup(x => x.Get(tradingCalendar.Id)).Returns(tradingCalendar);

            var service = new TradingCalendarService(repository.Object);

            var result = service.Get(tradingCalendar.Id, 2000);
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

            var tradingCalendar = new TradingCalendar(TradingCalendarIds.ASX);
            tradingCalendar.SetNonTradingDays(2001, new[] { new NonTradingDay(new Date(2001, 01, 01), "New Year's Day") });
            var repository = mockRepository.Create<ITradingCalendarRepository>();
            repository.Setup(x => x.UpdateYear(tradingCalendar, 2001));
            repository.Setup(x => x.Get(tradingCalendar.Id)).Returns(tradingCalendar);

            var service = new TradingCalendarService(repository.Object);

            var calendarReference = tradingCalendar;

            using (new AssertionScope())
            {
                calendarReference.IsTradingDay(new Date(2001, 01, 01)).Should().BeFalse();
                calendarReference.IsTradingDay(new Date(2001, 12, 25)).Should().BeTrue();
            }

            var updatedTradingCalendar = new RestApi.TradingCalendars.TradingCalendar();
            updatedTradingCalendar.Year = 2001;
            updatedTradingCalendar.NonTradingDays.Add(new RestApi.TradingCalendars.TradingCalendar.NonTradingDay() { Date = new Date(2001, 12, 25), Description = "Christmas Day" });

            var result = service.Update(tradingCalendar.Id, updatedTradingCalendar);

            using (new AssertionScope())
            {
                result.Should().HaveOkStatus();

                calendarReference.IsTradingDay(new Date(2001, 01, 01)).Should().BeTrue();
                calendarReference.IsTradingDay(new Date(2001, 12, 25)).Should().BeFalse();
            }

            mockRepository.Verify();
        }

        [Fact]
        public void SetNonTradingDays()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var tradingCalendar = new TradingCalendar(TradingCalendarIds.ASX);
            tradingCalendar.SetNonTradingDays(2001, new[] { new NonTradingDay(new Date(2001, 01, 01), "New Year's Day") });
            var repository = mockRepository.Create<ITradingCalendarRepository>();
            repository.Setup(x => x.UpdateYear(tradingCalendar, 2001));
            repository.Setup(x => x.Get(tradingCalendar.Id)).Returns(tradingCalendar);

            var service = new TradingCalendarService(repository.Object);

            var calendarReference = tradingCalendar;

            using (new AssertionScope())
            {
                calendarReference.IsTradingDay(new Date(2001, 01, 01)).Should().BeFalse();
                calendarReference.IsTradingDay(new Date(2001, 12, 25)).Should().BeTrue();
            }

            service.SetNonTradingDays(tradingCalendar.Id, 2001, new[] { new NonTradingDay(new Date(2001, 12, 25), "Christmas Day") });

            using (new AssertionScope())
            {
                calendarReference.IsTradingDay(new Date(2001, 01, 01)).Should().BeTrue();
                calendarReference.IsTradingDay(new Date(2001, 12, 25)).Should().BeFalse();
            }

            mockRepository.Verify();
        }
    }
}
