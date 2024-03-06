using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

using AngleSharp;
using AngleSharp.Dom;

using Booth.Common;
using Booth.PortfolioManager.Domain.TradingCalendars;


namespace Booth.PortfolioManager.DataServices
{
    internal class AsxTradingCalendarParser2021to2023 : IAsxTradingCalendarParser
    {
        public bool CanParse(int year)
        {
            return ((year >= 2021) && (year <= 2023));            
        }

        public string ResourceUrl(int year)
        {
            return "https://www2.asx.com.au/markets/market-resources/trading-hours-calendar/cash-market-trading-hours/trading-calendar";
        }

        public IEnumerable<NonTradingDay> ParseNonTradingDays(string html, int year)
        {

            var context = BrowsingContext.New(Configuration.Default);

            var task = context.OpenAsync(x => x.Content(html));
            task.Wait();
            var document = task.Result;

            // Locate the tabs for each year
            var tabHeaders = document.QuerySelectorAll("h5[role=tab]");
            var tabs = document.QuerySelectorAll("div[role=tabpanel]");
            
            for (var i = 0; i < tabHeaders.Count(); i++)
            {
                if (tabHeaders[i].TextContent == year.ToString())
                {
                    var rows = tabs[i].QuerySelectorAll("table > tbody > tr");
                    foreach (var row in rows)
                    {
                        var cells = row.QuerySelectorAll("td").ToList();
                        if (cells.Count >= 4)
                        {
                            if (GetCellValue(cells[2]) == "CLOSED")
                            {
                                var description = GetCellValue(cells[0]);

                                var dateText = GetCellValue(cells[1]) + " " + year;
                                if (Date.TryParse(dateText, out var date))
                                    yield return new NonTradingDay(date, description);
                            }
                        }
                    }

                    break;
                }
            }
        }

        private string GetCellValue(IElement cell)
        {
            var sups = cell.QuerySelectorAll("sup").ToArray();
            for (var i = 0; i < sups.Length; i++)
                sups[i].Remove();

            var bolds = cell.QuerySelectorAll("b").ToArray();
            for (var i = 0; i < bolds.Length; i++)
            {
                if ((bolds[i].TextContent.StartsWith("[")) && (bolds[i].TextContent.EndsWith("]")))
                    bolds[i].Remove();
            }

            return CleanupText(cell.TextContent);
        }

        private string CleanupText(string text)
        {
            if (text == null) 
                return null; 

            var stringBuilder = new StringBuilder();

            var lastCharIsWhiteSpace = false;
            foreach (var c in text)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (!lastCharIsWhiteSpace)
                    {
                        stringBuilder.Append(' ');
                        lastCharIsWhiteSpace = true;
                    }
                }
                else
                {
                    if (c == '\u2019')
                        stringBuilder.Append('\'');
                    else
                        stringBuilder.Append(c);
                    lastCharIsWhiteSpace = false;
                }
            }

            return stringBuilder.ToString().TrimEnd();
        }
    }
}
