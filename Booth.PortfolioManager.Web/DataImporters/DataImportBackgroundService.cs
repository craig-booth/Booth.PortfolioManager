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
        private readonly ILogger<DataImportBackgroundService> _Logger;
        private readonly Scheduler.Scheduler _Scheduler;
        private readonly IServiceScopeFactory _ServiceScopeFactory;

        private CancellationToken _CancellationToken;

        public DataImportBackgroundService(IServiceScopeFactory scopeFactory, ILogger<DataImportBackgroundService> logger)
        {
            _ServiceScopeFactory = scopeFactory;
            _Logger = logger;

            _Scheduler = new Scheduler.Scheduler();
            _Scheduler.AddJob("Import Historical Prices", () => ImportHistoricalPrices(), Schedule.EveryDay().At(20, 00), DateTime.Now);
            _Scheduler.AddJob("Import Live Prices", () => ImportLivePrices(), Schedule.EveryWeek().OnWeekdays().EveryMinutes(5).From(9, 30).Until(17, 00), DateTime.Now);
            _Scheduler.AddJob("Import Trading Days", () => ImportTradingDays(), Schedule.EveryMonth().OnLastDay().At(18, 00), DateTime.Now);   
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _CancellationToken = stoppingToken;

            // Run the data imports initially (wait 30 seconds to allow for application setup)
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken).ContinueWith(t => InitialImport());

            // and start Scheduler to run data imports on schedule
             _Scheduler.Start();

            await Task.Delay(-1, stoppingToken);

            _Scheduler.Stop();
        }

        private void InitialImport()
        {
            if (_CancellationToken.IsCancellationRequested) 
                return;

            try
            {
                ImportTradingDays();

            }
            catch (Exception e)
            {
                _Logger.LogError("Error occured importing trading days: {error}", e.Message);
            }

            try
            {
                ImportHistoricalPrices();
            }
            catch (Exception e)
            {
                _Logger.LogError("Error occured importing historical prices: {error}", e.Message);
            }

            try
            {
                ImportLivePrices();
            }
            catch (Exception e)
            {
                _Logger.LogError("Error occured importing live prices: {error}", e.Message);
            }
            
        }

        private void ImportHistoricalPrices()
        {
            if (_CancellationToken.IsCancellationRequested)
                return;

            using var serviceScope = _ServiceScopeFactory.CreateScope();
            {
                var importer = serviceScope.ServiceProvider.GetRequiredService<HistoricalPriceImporter>();

                var importTask = importer.Import(_CancellationToken);
                importTask.Wait();
            }
        }

        private void ImportLivePrices()
        {
            if (_CancellationToken.IsCancellationRequested) 
                return;

            using var serviceScope = _ServiceScopeFactory.CreateScope();
            {
                var importer = serviceScope.ServiceProvider.GetRequiredService<LivePriceImporter>();

                var importTask = importer.Import(_CancellationToken);
                importTask.Wait();
            }
        }

        private void ImportTradingDays()
        {
            if (_CancellationToken.IsCancellationRequested) 
                return;

            using var serviceScope = _ServiceScopeFactory.CreateScope();
            {
                var importer = serviceScope.ServiceProvider.GetRequiredService<TradingDayImporter>();

                var importTask = importer.Import(_CancellationToken);
                importTask.Wait();
            }
        }
    }
}
