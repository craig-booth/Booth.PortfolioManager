using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using Booth.Scheduler;
using Booth.Scheduler.Fluent;
using Booth.Common;


namespace Booth.PortfolioManager.Web.DataImporters
{
    class DataImportBackgroundService : BackgroundService
    {
        private CancellationToken _CancellationToken;
        private Scheduler.Scheduler _Scheduler;

        private readonly IServiceProvider _ServiceProvider;


        public DataImportBackgroundService(IServiceProvider servicesProvider)
        {
            _ServiceProvider = servicesProvider;

            _Scheduler = new Scheduler.Scheduler();
            _Scheduler.AddJob("Import Historical Prices", () => ImportHistoricalPrices(), Schedule.EveryDay().At(20, 00), DateTime.Now);
            _Scheduler.AddJob("Import Live Prices", () => ImportLivePrices(), Schedule.EveryWeek().OnWeekdays().EveryMinutes(5).From(9, 30).Until(17, 00), DateTime.Now);
            _Scheduler.AddJob("Import Trading Days", () => ImportTradingDays(), Schedule.EveryMonth().OnLastDay().At(18, 00), DateTime.Now);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _CancellationToken = stoppingToken;

            // Run the data imports initially
            var timer = new Timer(InitialImport, null, TimeSpan.FromSeconds(30), Timeout.InfiniteTimeSpan);

            // and start Scehduler to run data imports on schedule
            _Scheduler.Start();

            await Task.Delay(-1, stoppingToken);

            _Scheduler.Stop();
        }

        private void InitialImport(object state)
        {
            ImportTradingDays();
            ImportHistoricalPrices();
            ImportLivePrices();
        }

        private void ImportHistoricalPrices()
        {
            using (var scope = _ServiceProvider.CreateScope())
            {
                var importer = scope.ServiceProvider.GetRequiredService<HistoricalPriceImporter>();

                var importTask = importer.Import(_CancellationToken);
                importTask.Wait();
            }
        }

        private void ImportLivePrices()
        {
            using (var scope = _ServiceProvider.CreateScope())
            {
                var importer = scope.ServiceProvider.GetRequiredService<LivePriceImporter>();

                var importTask = importer.Import(_CancellationToken);
                importTask.Wait();
            }
        }

        private void ImportTradingDays()
        {
            using (var scope = _ServiceProvider.CreateScope())
            {
                var importer = scope.ServiceProvider.GetRequiredService<TradingDayImporter>();

                var importTask = importer.Import(_CancellationToken);
                importTask.Wait();
            }
        }
    }
}
