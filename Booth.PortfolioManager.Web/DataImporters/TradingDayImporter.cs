using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Booth.EventStore;
using Booth.PortfolioManager.Domain.TradingCalanders;
using Booth.PortfolioManager.DataServices;
using Booth.PortfolioManager.Web.Services;

namespace Booth.PortfolioManager.Web.DataImporters
{
    class TradingDayImporter
    {
        private readonly ITradingCalanderService _TradingCalanderService;
        private readonly ITradingDayService _DataService;
        private readonly ILogger _Logger;

        public TradingDayImporter(ITradingCalanderService tradingCalanderService, ITradingDayService dataService, ILogger<TradingDayImporter> logger)
        {
            _TradingCalanderService = tradingCalanderService;
            _DataService = dataService;
            _Logger = logger;
        }

        public async Task Import(CancellationToken cancellationToken)
        {
            int year = DateTime.Today.Year;

            var nonTradingDays = await _DataService.GetNonTradingDays(year, cancellationToken);

            if (nonTradingDays.Any())
            {
                _Logger?.LogInformation("Adding {0} non-trading days for {1}", nonTradingDays.Count(), year);
                _TradingCalanderService.SetNonTradingDays(year, nonTradingDays);
            }           
        }
    }
}
