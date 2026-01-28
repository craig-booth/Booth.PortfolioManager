using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Moq;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Web.Controllers;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Domain.TradingCalendars;

namespace Booth.PortfolioManager.Web.Test.Controllers
{
    public class TradingCalendarControllerTests
    {

        [Fact]
        public async Task GetYear()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new RestApi.TradingCalendars.TradingCalendar();

            var service = mockRepository.Create<ITradingCalendarService>();
            service.Setup(x => x.GetAsync(TradingCalendarIds.ASX, 2010)).Returns(Task.FromResult(ServiceResult<RestApi.TradingCalendars.TradingCalendar>.Ok(response))).Verifiable();

            var controller = new TradingCalendarController(service.Object);
            var result = await controller.Get(2010);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task UpdateValidationError()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var tradingCalendar = new RestApi.TradingCalendars.TradingCalendar();

            var service = mockRepository.Create<ITradingCalendarService>();
            service.Setup(x => x.UpdateAsync(TradingCalendarIds.ASX, tradingCalendar)).Returns(Task.FromResult(ServiceResult.Error("Error message"))).Verifiable();

            var controller = new TradingCalendarController(service.Object);
            var result = await controller.Update(2010, tradingCalendar);

            result.Should().BeBadRequestObjectResult().Which.Value.Should().BeEquivalentTo(new [] { "Error message" });

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task Update()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var tradingCalendar = new RestApi.TradingCalendars.TradingCalendar();

            var service = mockRepository.Create<ITradingCalendarService>();
            service.Setup(x => x.UpdateAsync(TradingCalendarIds.ASX, tradingCalendar)).Returns(Task.FromResult(ServiceResult.Ok())).Verifiable();

            var controller = new TradingCalendarController(service.Object);
            var result = await controller.Update(2010, tradingCalendar);

            result.Should().BeOkResult();

            mockRepository.VerifyAll();
        }  

    }
}
