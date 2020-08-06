using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Booth.EventStore;
using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.DataServices;
using Booth.PortfolioManager.Web.Services;

namespace Booth.PortfolioManager.Web.DataImporters
{
    class TradingDayImporter
    {
        private readonly ITradingCalendarService _TradingCalendarService;
        private readonly ITradingDayService _DataService;
        private readonly ILogger _Logger;

        public TradingDayImporter(ITradingCalendarService tradingCalendarService, ITradingDayService dataService, ILogger<TradingDayImporter> logger)
        {
            _TradingCalendarService = tradingCalendarService;
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
                _TradingCalendarService.SetNonTradingDays(year, nonTradingDays);
            }           
        }
    }
}
