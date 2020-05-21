using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

using Booth.Common;

namespace Booth.PortfolioManager.DataServices
{
    public class AlphaVantageDataService : ILiveStockPriceService, IHistoricalStockPriceService
    {
        private const string _ApiKey = "KVFE5MLIMDUFO2TX";

        private HttpClient _HttpClient;

        public AlphaVantageDataService(HttpClient httpClient)
        {
            _HttpClient = httpClient;
        }

        public async Task<IEnumerable<StockPrice>> GetHistoricalPriceData(string asxCode, DateRange dateRange, CancellationToken cancellationToken)
        {
            var result = new List<StockPrice>();

            try
            {
                var httpClient = new HttpClient();

                string url = String.Format("https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={0}.AX&apikey={1}", asxCode, _ApiKey);
                var response = await _HttpClient.GetAsync(url, cancellationToken);

                var responseStream = await response.Content.ReadAsStreamAsync();
                using (var json = await JsonDocument.ParseAsync(responseStream))
                    result.AddRange(ParseResponse(json).Where(x => x.Date.InRange(dateRange)).OrderBy(x => x.Date));
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
                string url = String.Format("https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={0}.AX&apikey={1}", asxCode, _ApiKey);
                var response = await _HttpClient.GetAsync(url, cancellationToken);

                var responseStream = await response.Content.ReadAsStreamAsync();

                using (var json = await JsonDocument.ParseAsync(responseStream))
                    return ParseResponse(json).FirstOrDefault();            
            }
            catch
            {
                return null;
            } 
        }

        private IEnumerable<StockPrice> ParseResponse(JsonDocument response)
        {
            var asxCode = response.RootElement.GetProperty("Meta Data").GetProperty("2. Symbol").GetString();
            if (asxCode.EndsWith(".AX"))
                asxCode = asxCode.Remove(asxCode.Length - 3);

            var dailyPrices = response.RootElement.GetProperty("Time Series (Daily)").EnumerateObject();
            foreach (var dailyPrice in dailyPrices)
            {
                var date = Date.Parse(dailyPrice.Name);
                var price = Decimal.Parse(dailyPrice.Value.GetProperty("4. close").GetString());

                yield return new StockPrice(asxCode, date, price);
            } 
        } 
    }
}
