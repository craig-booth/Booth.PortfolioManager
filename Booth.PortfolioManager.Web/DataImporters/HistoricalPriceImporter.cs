using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Booth.Common;
using Booth.PortfolioManager.Domain;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.DataServices;
using Booth.PortfolioManager.Web.Utilities;
using Booth.PortfolioManager.Web.Services;

namespace Booth.PortfolioManager.Web.DataImporters
{
    class HistoricalPriceImporter
    {
        private readonly IHistoricalStockPriceService _DataService;
        private readonly IStockQuery _StockQuery;
        private IStockService _StockService;
        private readonly ITradingCalendar _TradingCalendar;
        private readonly ILogger _Logger;

        public HistoricalPriceImporter(IStockQuery stockQuery, IStockService stockService, ITradingCalendar tradingCalendar, IHistoricalStockPriceService dataService, ILogger<HistoricalPriceImporter> logger)
        {
            _StockQuery = stockQuery;
            _StockService = stockService;
            _TradingCalendar = tradingCalendar;
            _DataService = dataService;
            _Logger = logger;
        }


        public async Task Import(CancellationToken cancellationToken)
        {
            var lastExpectedDate = Date.Today.AddDays(-1);
            while (! _TradingCalendar.IsTradingDay(lastExpectedDate))
                lastExpectedDate = lastExpectedDate.AddDays(-1);

            foreach (var stock in _StockQuery.All())
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var latestDate = stock.DateOfLastestPrice(); 

                if (latestDate < lastExpectedDate)
                {                 
                    var dateRange = new DateRange(latestDate.AddDays(1), lastExpectedDate);
                    var asxCode = stock.Properties.ClosestTo(dateRange.ToDate).AsxCode;

                    _Logger?.LogInformation("Importing closing prices for {0} between {1:d} and {2:d}", asxCode, dateRange.FromDate, dateRange.ToDate);

                    var data = await _DataService.GetHistoricalPriceData(asxCode, dateRange, cancellationToken);
                 
                    var closingPrices = data.Select(x => new Domain.Stocks.StockPrice(x.Date, x.Price)).ToList();
                    _Logger?.LogInformation("{0} closing prices found", closingPrices.Count);
                    if (closingPrices.Count > 0)
                    {
                        _StockService.UpdateClosingPrices(stock.Id, closingPrices);
                    }
                     
                }
            }
        }
    }


}
