using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Xml.Linq;

using Booth.Common;


namespace Booth.PortfolioManager.DataServices
{
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
                string url = "https://www.asx.com.au/asx/1/share/" + asxCode;
                var response = await _HttpClient.GetAsync(url, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return null;

                var responseStream = await response.Content.ReadAsStreamAsync();

                using (var json = await JsonDocument.ParseAsync(responseStream))
                {
                    var price = json.RootElement.GetProperty("last_price").GetDecimal();
                    var date = Date.Parse(json.RootElement.GetProperty("last_trade_date").GetString().Substring(0, 10));

                    return new StockPrice(asxCode, date, price);
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<NonTradingDay>> GetNonTradingDays(int year, CancellationToken cancellationToken)
        {
            var url = String.Format("http://www.asx.com.au/about/asx-trading-calendar-{0:d}.htm", year);
            var response = await _HttpClient.GetAsync(url, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return null;

            var text = await response.Content.ReadAsStringAsync();


            return ParseNonTradingDaysResponse(text, year).ToList();
        }


        private IEnumerable<NonTradingDay> ParseNonTradingDaysResponse(string html, int year)
        {
            // Find start of data
            var start = html.IndexOf("<!-- start content -->");
            var end = -1;
            if (start >= 0)
            {
                start = html.IndexOf("<tbody>", start);
                end = html.IndexOf("</tbody>", start);
            }

            if ((start >= 0) && (end >= 0))
            {
                var data = html.Substring(start, end - start + 8);

                data = data.Replace("&nbsp;", " ");

                var table = XElement.Parse(data);

                foreach (var tableRow in table.Descendants("tr"))
                {
                    var cells = tableRow.Descendants("td").ToList();
                    if (cells.Count >= 4)
                    {
                        int cellNumber = 3;
                        if (cells.Count == 6)
                            cellNumber = 2;

                        if (GetCellValue(cells[cellNumber]) == "CLOSED")
                        {
                            var description = GetCellValue(cells[0]);

                            var dateText = GetCellValue(cells[1]) + " " + year;
                            if (Date.TryParse(dateText, out var date))
                                yield return new NonTradingDay(date, description);
                        }
                    }
                }
            }
        }

        private string GetCellValue(XElement cell)
        {
            var sups = cell.Descendants("sup").ToArray();
            for (var i = 0; i < sups.Length; i++)
                sups[i].Remove();

            return cell.Value.Trim();
        }
    }
}
