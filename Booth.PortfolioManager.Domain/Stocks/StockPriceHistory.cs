using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

namespace Booth.PortfolioManager.Domain.Stocks
{
    public interface IStockPriceHistory
    {
        Date EarliestDate { get; }
        Date LatestDate { get; }
        decimal GetPrice(Date date);
        IEnumerable<StockPrice> GetPrices(DateRange dateRange);
    }

    public class StockPriceHistory : IEntity, IStockPriceHistory
    {
        private List<StockPrice> _Prices { get; } = new List<StockPrice>();

        public Guid Id { get; }

        public StockPriceHistory(Guid id)
        {
            Id = id;
        }

        public Date EarliestDate
        {
            get
            {
                if (_Prices.Count > 0)
                    return _Prices[0].Date;
                else
                    return Date.MinValue;
            }
        }

        public Date LatestDate
        {
            get
            {
                if (_Prices.Count > 0)
                    return _Prices[_Prices.Count -1].Date;
                else
                    return Date.MinValue;
            }
        }

        public decimal GetPrice(Date date)
        {
            var index = _Prices.BinarySearch(new StockPrice(date, 0.00m), new StockPriceComparer());

            if (index >= 0)
                return _Prices[index].Price;
            else if (index == -1)
                return 0.00m;
            else
                return _Prices[~index - 1].Price;
        }

        public IEnumerable<StockPrice> GetPrices(DateRange dateRange)
        {
            var first = _Prices.BinarySearch(new StockPrice(dateRange.FromDate, 0.00m), new StockPriceComparer());
            if (first < 0)
                first = ~first;

            var last = _Prices.BinarySearch(new StockPrice(dateRange.ToDate, 0.00m), new StockPriceComparer());
            if (last < 0)
                last = ~last - 1;

            return _Prices.Skip(first).Take(last - first + 1);
        }

        public void UpdateCurrentPrice(decimal currentPrice)
        {
            UpdatePrice(Date.Today, currentPrice);
        }

        public void UpdateClosingPrice(Date date, decimal closingPrice)
        {         
            UpdatePrice(date, closingPrice);
        }

        public void UpdateClosingPrices(IEnumerable<StockPrice> closingPrices)
        {
            foreach (var closingPrice in closingPrices)
                UpdatePrice(closingPrice.Date, closingPrice.Price);
        }

        private void UpdatePrice(Date date, decimal price)
        {
            var index = _Prices.BinarySearch(new StockPrice(date, 0.00m), new StockPriceComparer());

            if (index >= 0)
                _Prices[index] = new StockPrice(date, price);
            else
                _Prices.Insert(~index, new StockPrice(date, price));
        }

        private class StockPriceComparer : IComparer<StockPrice>
        {
            public int Compare(StockPrice x, StockPrice y)
            {
                return x.Date.CompareTo(y.Date);
            }
        }
    }
}
