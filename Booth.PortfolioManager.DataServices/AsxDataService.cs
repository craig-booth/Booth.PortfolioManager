using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Xml.Linq;

using Booth.Common;
using Booth.PortfolioManager.Domain.TradingCalendars;

namespace Booth.PortfolioManager.DataServices
{
    public interface IAsxTradingCalendarParser
    {
        bool CanParse(int year);
        string ResourceUrl(int year);
        IEnumerable<NonTradingDay> ParseNonTradingDays(string html, int year);        
    }



    public class AsxDataService : ITradingDayService, ILiveStockPriceService, IHistoricalStockPriceService
    {
        private HttpClient _HttpClient;

        public AsxDataService(HttpClient httpClient)
        {
            _HttpClient = httpClient;
        }

        public async Task<IEnumerable<StockPrice>> GetHistoricalPriceData(string asxCode, DateRange dateRange, CancellationToken cancellationToken)
        {
            var result = new List<StockPrice>();

            try
            {
                string url = String.Format("https://www.asx.com.au/asx/1/share/{0}/prices?interval=daily&count={1}", asxCode, Date.Today.Subtract(dateRange.FromDate).Days);
                var response = await _HttpClient.GetAsync(url, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    var responseStream = await response.Content.ReadAsStreamAsync();

                    using (var json = await JsonDocument.ParseAsync(responseStream))
                    {
                        foreach (var dailyPrice in json.RootElement.GetProperty("data").EnumerateArray())
                        {
                            var price = dailyPrice.GetProperty("close_price").GetDecimal();
                            var date = Date.Parse(dailyPrice.GetProperty("close_date").GetString().Substring(0, 10));

                            if (date.InRange(dateRange))
                                result.Add(new StockPrice(asxCode, date, price));
                        }
                    }
                }
            }
            catch
            {
                return result;
            }

            return result;
        }

        public Task<IEnumerable<StockPrice>> GetMultiplePrices(IEnumerable<string> asxCodes, CancellationToken cancellationToken)
        {
            var stockPrices = new List<StockPrice>();

            var tasks = asxCodes.Select(x => GetSinglePrice(x, cancellationToken)).ToArray();

            Task.WaitAll(tasks, cancellationToken);

            return Task<IEnumerable<StockPrice>>.FromResult(tasks.Where(x => x.Result != null).Select(x => x.Result));
        }

        public async Task<StockPrice> GetSinglePrice(string asxCode, CancellationToken cancellationToken)
        {
            try
            {
                string url = $"https://asx.api.markitdigital.com/asx-research/1.0/companies/{asxCode}/header";
                var response = await _HttpClient.GetAsync(url, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return null;

                var responseStream = await response.Content.ReadAsStreamAsync();

                using (var json = await JsonDocument.ParseAsync(responseStream))
                {
                    var price = json.RootElement.GetProperty("data").GetProperty("priceLast").GetDecimal();
                    var date = Date.Today;

                    return new StockPrice(asxCode, date, Math.Round(price, 5));
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<NonTradingDay>> GetNonTradingDays(int year, CancellationToken cancellationToken)
        {
            var parser = GetParser(year);
            if (parser == null)
                return null;

            var url = parser.ResourceUrl(year);
            var response = await _HttpClient.GetAsync(url, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return null;

            var text = await response.Content.ReadAsStringAsync();

            return parser.ParseNonTradingDays(text, year).ToList();
        }

        private IAsxTradingCalendarParser GetParser(int year)
        {
            var parserTypes = TypeUtils.GetSubclassesOf(typeof(IAsxTradingCalendarParser), true);

            foreach (var parserType in parserTypes)
            {
                var parser = (IAsxTradingCalendarParser)Activator.CreateInstance(parserType);

                if (parser.CanParse(year))
                    return parser;
            }

            return null;
        }

    }
}
