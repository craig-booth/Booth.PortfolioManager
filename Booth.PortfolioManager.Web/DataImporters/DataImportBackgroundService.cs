using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Booth.Scheduler;
using Booth.Scheduler.Fluent;
using Booth.Common;

namespace Booth.PortfolioManager.Web.DataImporters
{
    class DataImportBackgroundService : BackgroundService
    {
        private Scheduler.Scheduler _Scheduler;

        private readonly HistoricalPriceImporter _HistoricalPriceImporter;
        private readonly LivePriceImporter _LivePriceImporter;
        private readonly TradingDayImporter _TradingDayImporter;

        public DataImportBackgroundService(Scheduler.Scheduler scheduler, HistoricalPriceImporter historicalPriceImporter, LivePriceImporter livePriceImporter, TradingDayImporter tradingDayImporter)
        {
            _Scheduler = scheduler;
            _HistoricalPriceImporter = historicalPriceImporter;
            _LivePriceImporter = livePriceImporter;
            _TradingDayImporter = tradingDayImporter;

            _Scheduler.AddJob("Import Historical Prices", () => ImportHistoricalPrices(), Schedule.EveryDay().At(20, 00), DateTime.Now);
            _Scheduler.AddJob("Import Live Prices", () => ImportLivePrices(), Schedule.EveryWeek().OnWeekdays().EveryMinutes(5).From(9, 30).Until(17, 00), DateTime.Now);
            _Scheduler.AddJob("Import Trading Days", () => ImportTradingDays(), Schedule.EveryMonth().OnLastDay().At(18, 00), DateTime.Now);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _Scheduler.Start();

            await Task.Delay(-1, stoppingToken);

            _Scheduler.Stop();
        }

        private void ImportHistoricalPrices()
        {
            var importTask = _HistoricalPriceImporter.Import(CancellationToken.None);
            importTask.Wait();
        }

        private void ImportLivePrices()
        {
            var importTask = _LivePriceImporter.Import(CancellationToken.None);
            importTask.Wait();
        }

        private void ImportTradingDays()
        {
            var importTask = _TradingDayImporter.Import(CancellationToken.None);
            importTask.Wait();
        }
    }
}
