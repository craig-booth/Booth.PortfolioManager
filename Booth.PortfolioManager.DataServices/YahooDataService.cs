using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Xml.Linq;

using Booth.Common;
using System.Text.Json.Nodes;
namespace Booth.PortfolioManager.DataServices
{


    public class YahooDataService : IHistoricalStockPriceService
    {
        private HttpClient _HttpClient;

        public YahooDataService(HttpClient httpClient)
        {
            _HttpClient = httpClient;
            _HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
        }

        public async Task<IEnumerable<StockPrice>> GetHistoricalPriceData(string asxCode, DateRange dateRange, CancellationToken cancellationToken)
        {
            var result = new List<StockPrice>();

            try
            {
                var range = GetRange(dateRange.FromDate);

                string url = String.Format($"https://query1.finance.yahoo.com/v8/finance/chart/{asxCode}.AX?interval=1d&range={range}");
                var response = await _HttpClient.GetAsync(url, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    var responseStream = await response.Content.ReadAsStreamAsync();

                    var json = await JsonNode.ParseAsync(responseStream);

                    var results = json["chart"]["result"].AsArray().First();
                    var gmtOffset = (int)results["meta"]["gmtoffset"];
                    var timeStamps = results["timestamp"].AsArray();
                    var closingPrices = results["indicators"]["quote"].AsArray().First()["close"].AsArray();

                    for (var i = 0; i < timeStamps.Count; i++) 
                    {
                        var date = new Date(DateTimeOffset.FromUnixTimeSeconds((int)timeStamps.ElementAt(i) + gmtOffset).Date);
                        var price = Math.Round((decimal)closingPrices.ElementAt(i), 5);

                        if (date.InRange(dateRange))
                            result.Add(new StockPrice(asxCode, date, price));
                    }
                }
            }
            catch
            {
                return result;
            }

            return result;
        }
        private string GetRange(Date startDate)
        {
            var today = Date.Today;

            if (today == startDate) return "1d";
            else if (today.AddDays(-5) <= startDate) return "5d";
            else if (today.AddMonths(-1) <= startDate) return "1mo";
            else if (today.AddMonths(-6) <= startDate) return "6mo";
            else if (today.AddYears(-1) <= startDate) return "1y";
            else if (today.AddYears(-2) <= startDate) return "2y";
            else if (today.AddYears(-5) <= startDate) return "5y";
            else if (today.AddYears(-10) <= startDate) return "10y";
            else return "max";
        }

    }
}
