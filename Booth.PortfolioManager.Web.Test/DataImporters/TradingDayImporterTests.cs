using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

using Xunit;
using Moq;
using FluentAssertions;

using Booth.PortfolioManager.Domain.TradingCalanders;
using Booth.PortfolioManager.DataServices;
using Booth.PortfolioManager.Web.DataImporters;
using System.Linq;
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

            var tradingCalanderService = mockRepository.Create<ITradingCalanderService>();

            var logger = mockRepository.Create<ILogger<TradingDayImporter>>(MockBehavior.Loose);

            var importer = new TradingDayImporter(tradingCalanderService.Object, dataService.Object, logger.Object);
            
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
            dataService.Setup(x => x.GetNonTradingDays(year, cancellationToken)).Returns(Task<IEnumerable<NonTradingDay>>.FromResult(nonTradingDays.AsEnumerable()));

            var tradingCalanderService = mockRepository.Create<ITradingCalanderService>();
            tradingCalanderService.Setup(x => x.SetNonTradingDays(year, nonTradingDays)).Returns(ServiceResult.Ok());

            var logger = mockRepository.Create<ILogger<TradingDayImporter>>(MockBehavior.Loose);

            var importer = new TradingDayImporter(tradingCalanderService.Object, dataService.Object, logger.Object);

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

            var tradingCalanderService = mockRepository.Create<ITradingCalanderService>();
            tradingCalanderService.Setup(x => x.SetNonTradingDays(year, nonTradingDays)).Returns(ServiceResult.Ok());

            var importer = new TradingDayImporter(tradingCalanderService.Object, dataService.Object, null);

            await importer.Import(cancellationToken);
        }


    }
}
