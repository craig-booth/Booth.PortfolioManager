using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.TradingCalendars;

namespace Booth.PortfolioManager.DataServices
{
    public interface ILiveStockPriceService
    {
        Task<StockPrice> GetSinglePrice(string asxCode, CancellationToken cancellationToken);
        Task<IEnumerable<StockPrice>> GetMultiplePrices(IEnumerable<string> asxCodes, CancellationToken cancellationToken);
    }

    public interface IHistoricalStockPriceService
    {
        Task<IEnumerable<StockPrice>> GetHistoricalPriceData(string asxCode, DateRange dateRange, CancellationToken cancellationToken);
    }

    public interface ITradingDayService
    {
        Task<IEnumerable<NonTradingDay>> GetNonTradingDays(int year, CancellationToken cancellationToken);
    }

    public class StockPrice
    {
        public string AsxCode { get; set; }
        public Date Date { get; set; }
        public decimal Price { get; set; }
        public StockPrice(string asxCode, Date date, decimal price)
        {
            AsxCode = asxCode;
            Date = date;
            Price = price;
        }
    }
}
