using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Stocks.Events;

namespace Booth.PortfolioManager.Domain.Stocks
{

    public interface IStockPriceHistory
    {
        Guid Id { get; }
        Date EarliestDate { get; }
        Date LatestDate { get; }
        decimal GetPrice(Date date);
        IEnumerable<KeyValuePair<Date, decimal>> GetPrices(DateRange dateRange);
    }

    public class StockPriceHistory : TrackedEntity, IStockPriceHistory
    {
        private SortedList<Date, decimal> _Prices { get; } = new SortedList<Date, decimal>();

        public StockPriceHistory(Guid id)
            : base(id)
        {

        }

        public Date EarliestDate
        {
            get
            {
                if (_Prices.Count > 0)
                    return _Prices.First().Key;
                else
                    return Date.MinValue;
            }
        }

        public Date LatestDate
        {
            get
            {
                if (_Prices.Count > 0)
                    return _Prices.Last().Key;
                else
                    return Date.MinValue;
            }
        }

        public decimal GetPrice(Date date)
        {
            var index = IndexOf(date);

            if (index >= 0)
                return _Prices.Values[index];
            else if (index == -1)
                return 0.00m;
            else
                return _Prices.Values[~index];

        }

        public IEnumerable<KeyValuePair<Date, decimal>> GetPrices(DateRange dateRange)
        {
            var first = IndexOf(dateRange.FromDate);
            if (first == -1)
                first = 0;
            else if (first < 0)
                first = ~first;

            var last = IndexOf(dateRange.ToDate);
            if (last == -1)
                last = 0;
            else if (last < 0)
                last = ~last;

            return _Prices.Skip(first).Take(last - first + 1);
        }

        public void UpdateCurrentPrice(decimal currentPrice)
        {
            UpdatePrice(Date.Today, currentPrice);
        }

        public void UpdateClosingPrice(Date date, decimal closingPrice)
        {
            var @event = new ClosingPricesAddedEvent(Id, Version, new ClosingPricesAddedEvent.ClosingPrice[] { new ClosingPricesAddedEvent.ClosingPrice(date, closingPrice) });
            Apply(@event);

            PublishEvent(@event);
        }

        public void UpdateClosingPrices(IEnumerable<Tuple<Date, decimal>> closingPrices)
        {
            var @event = new ClosingPricesAddedEvent(Id, Version, closingPrices.Select(x => new ClosingPricesAddedEvent.ClosingPrice(x.Item1, x.Item2)));
            Apply(@event);

            PublishEvent(@event);
        }

        private void Apply(ClosingPricesAddedEvent @event)
        {
            Version++;
            foreach (var closingPrice in @event.ClosingPrices)
                UpdatePrice(closingPrice.Date, closingPrice.Price);
        }

        private void UpdatePrice(Date date, decimal price)
        {
            if (_Prices.ContainsKey(date))
                _Prices[date] = price;
            else
                _Prices.Add(date, price);
        }

        private int IndexOf(Date date)
        {
            if (_Prices.Keys.Count == 0)
                return -1;

            int begin = 0;
            int end = _Prices.Keys.Count;
            while (end > begin)
            {
                int index = (begin + end) / 2;
                var el = _Prices.Keys[index];

                var r = el.CompareTo(date);
                if (r == 0)
                    return index;
                else if (r > 0)
                    end = index;
                else
                    begin = index + 1;
            }

            if (end == 0)
                return -1;
            else
                return -end;
        }
    }
}
