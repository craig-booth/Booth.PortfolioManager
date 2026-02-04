using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

using Xunit;
using Moq;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.DataServices;
using Booth.PortfolioManager.Web.DataImporters;
using Booth.PortfolioManager.Web.Services;

namespace Booth.PortfolioManager.Web.Test.DataImporters
{
    public class TradingDayImporterTests
    {

        [Fact]
        public async Task ImportNoDataReturned()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var year = DateTime.Now.Year;

            var cancellationToken = new CancellationToken();
            var nonTradingDays = new NonTradingDay[] { };
            var dataService = mockRepository.Create<ITradingDayService>();
            dataService.Setup(x => x.GetNonTradingDays(year, cancellationToken)).Returns(Task<IEnumerable<NonTradingDay>>.FromResult(nonTradingDays.AsEnumerable()));

            var tradingCalendarService = mockRepository.Create<ITradingCalendarService>();
            tradingCalendarService.Setup(x => x.GetAsync(TradingCalendarIds.ASX, It.IsAny<int>())).Returns((Guid id, int year) => Task.FromResult(GetTradingCalendar(id, year)));

            var logger = mockRepository.Create<ILogger<TradingDayImporter>>(MockBehavior.Loose);

            var importer = new TradingDayImporter(tradingCalendarService.Object, dataService.Object, logger.Object);
            
            await importer.Import(cancellationToken);

            mockRepository.Verify();
        }

        [Fact]
        public async Task ImportDataReturned()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var year = DateTime.Now.Year;

            var cancellationToken = new CancellationToken();
            var nonTradingDays = new NonTradingDay[]
            {
                new NonTradingDay(new Common.Date(year, 01, 01), "New Year's Day"),
                new NonTradingDay(new Common.Date(year, 12, 25), "Christmas Day")
            };
            var dataService = mockRepository.Create<ITradingDayService>();
            dataService.Setup(x => x.GetNonTradingDays(year, cancellationToken)).Returns(Task.FromResult(nonTradingDays.AsEnumerable()));

            var tradingCalendarService = mockRepository.Create<ITradingCalendarService>();
            tradingCalendarService.Setup(x => x.GetAsync(TradingCalendarIds.ASX, It.IsAny<int>())).Returns((Guid id, int year) => Task.FromResult(GetTradingCalendar(id, year)));
            tradingCalendarService.Setup(x => x.SetNonTradingDaysAsync(TradingCalendarIds.ASX, year, nonTradingDays)).Returns(Task.FromResult(ServiceResult.Ok())).Verifiable();

            var logger = mockRepository.Create<ILogger<TradingDayImporter>>(MockBehavior.Loose);

            var importer = new TradingDayImporter(tradingCalendarService.Object, dataService.Object, logger.Object);

            await importer.Import(cancellationToken);

            mockRepository.Verify();
        }


        [Fact]
        public async Task ImportNoLogger()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var year = DateTime.Now.Year;

            var cancellationToken = new CancellationToken();
            var nonTradingDays = new NonTradingDay[]
            {
                new NonTradingDay(new Common.Date(year, 01, 01), "New Year's Day"),
                new NonTradingDay(new Common.Date(year, 12, 25), "Christmas Day")
            };
            var dataService = mockRepository.Create<ITradingDayService>();
            dataService.Setup(x => x.GetNonTradingDays(year, cancellationToken)).Returns(Task<IEnumerable<NonTradingDay>>.FromResult(nonTradingDays.AsEnumerable()));

            var tradingCalendarService = mockRepository.Create<ITradingCalendarService>();
            tradingCalendarService.Setup(x => x.GetAsync(TradingCalendarIds.ASX, It.IsAny<int>())).Returns((Guid id, int year) => Task.FromResult(GetTradingCalendar(id, year)));
            tradingCalendarService.Setup(x => x.SetNonTradingDaysAsync(TradingCalendarIds.ASX, year, nonTradingDays)).Returns(Task.FromResult(ServiceResult.Ok())).Verifiable();

            var importer = new TradingDayImporter(tradingCalendarService.Object, dataService.Object, null);

            await importer.Import(cancellationToken);

            mockRepository.Verify();
        }

        private ServiceResult<Models.TradingCalendar.TradingCalendar> GetTradingCalendar(Guid id, int requestedYear)
        {
            var calendar = new Models.TradingCalendar.TradingCalendar()
            {
                Year = requestedYear
            };
            if (requestedYear < DateTime.Now.Year)
            {
                calendar.AddNonTradingDay(new Date(requestedYear, 05, 06), "Birthday");
            }

            return ServiceResult<Models.TradingCalendar.TradingCalendar>.Ok(calendar);
        }

    }
}
