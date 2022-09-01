using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using AngleSharp;
using AngleSharp.Dom;


using Booth.Common;
using Booth.PortfolioManager.Domain.TradingCalendars;

namespace Booth.PortfolioManager.DataServices
{
    internal class AsxTradingCalendarParser2015to2020 : IAsxTradingCalendarParser
    {

        public bool CanParse(int year)
        {
            return (year >= 2015) && (year <= 2020);
        }

        public string ResourceUrl(int year)
        {
            return String.Format("http://www.asx.com.au/about/asx-trading-calendar-{0:d}.htm", year);
        }

        public IEnumerable<NonTradingDay> ParseNonTradingDays(string html, int year)
        {
            var context = BrowsingContext.New(Configuration.Default);

            var task = context.OpenAsync(x => x.Content(html));
            task.Wait();
            var document = task.Result;

            var table = document.QuerySelector("table.contenttable");
            if (table != null)
            {
                var rows = table.QuerySelectorAll("tbody > tr");
                foreach (var row in rows)
                {
                    var cells = row.QuerySelectorAll("td").ToList();
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
        private string GetCellValue(IElement cell)
        {
            var sups = cell.QuerySelectorAll("sup").ToArray();
            for (var i = 0; i < sups.Length; i++)
                sups[i].Remove();

            return cell.TextContent.Trim();
        }  

    }
}
