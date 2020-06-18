using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;

using Booth.Common;
using Booth.PortfolioManager.DataServices;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Utilities;

namespace Booth.PortfolioManager.Web.DataImporters
{
    class LivePriceImporter
    {
        private readonly ILiveStockPriceService _DataService;
        private readonly IStockQuery _StockQuery;
        private IStockService _StockService;
        private readonly ILogger _Logger;

        public LivePriceImporter(IStockQuery stockQuery, IStockService stockService, ILiveStockPriceService dataService, ILogger<LivePriceImporter> logger)
        {
            _StockQuery = stockQuery;
            _StockService = stockService;
            _DataService = dataService;
            _Logger = logger;
        }

        public async Task Import(CancellationToken cancellationToken)
        {
            var asxCodes = _StockQuery.All(Date.Today).Select(x => x.Properties[Date.Today].AsxCode);
                
            var stockQuotes = await _DataService.GetMultiplePrices(asxCodes, cancellationToken);

            foreach (var stockQuote in stockQuotes)
            {
                if (stockQuote.Date == Date.Today)
                {
                    var stock = _StockQuery.Get(stockQuote.AsxCode, stockQuote.Date);
                    if (stock != null)
                    {
                        _Logger?.LogInformation("Updating current price foe {0}: {1}", stockQuote.AsxCode, stockQuote.Price);

                        _StockService.UpdateCurrentPrice(stock.Id, stockQuote.Price);
                    }
                        
                }
            }      
        }
    }
}
