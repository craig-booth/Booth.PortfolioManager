using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using Moq;
using FluentAssertions;
using FluentAssertions.AspNetCore.Mvc;

using Booth.Common;
using Booth.PortfolioManager.Web.Controllers;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Domain.TradingCalendars;

namespace Booth.PortfolioManager.Web.Test.Controllers
{
    public class TradingCalendarControllerTests
    {

        [Fact]
        public void GetYear()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new RestApi.TradingCalendars.TradingCalendar();

            var service = mockRepository.Create<ITradingCalendarService>();
            service.Setup(x => x.Get(TradingCalendarIds.ASX, 2010)).Returns(ServiceResult<RestApi.TradingCalendars.TradingCalendar>.Ok(response)).Verifiable();

            var controller = new TradingCalendarController(service.Object);
            var result = controller.Get(2010);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void UpdateValidationError()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var tradingCalendar = new RestApi.TradingCalendars.TradingCalendar();

            var service = mockRepository.Create<ITradingCalendarService>();
            service.Setup(x => x.Update(TradingCalendarIds.ASX, tradingCalendar)).Returns(ServiceResult.Error("Error message")).Verifiable();

            var controller = new TradingCalendarController(service.Object);
            var result = controller.Update(2010, tradingCalendar);

            result.Should().BeBadRequestObjectResult().Error.Should().BeEquivalentTo(new [] { "Error message" });

            mockRepository.VerifyAll();
        }

        [Fact]
        public void Update()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var tradingCalendar = new RestApi.TradingCalendars.TradingCalendar();

            var service = mockRepository.Create<ITradingCalendarService>();
            service.Setup(x => x.Update(TradingCalendarIds.ASX, tradingCalendar)).Returns(ServiceResult.Ok()).Verifiable();

            var controller = new TradingCalendarController(service.Object);
            var result = controller.Update(2010, tradingCalendar);

            result.Should().BeOkResult();

            mockRepository.VerifyAll();
        }  

    }
}
