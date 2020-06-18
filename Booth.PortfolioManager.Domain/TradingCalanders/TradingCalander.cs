using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;
using Booth.EventStore;

using Booth.PortfolioManager.Domain.TradingCalanders.Events;

namespace Booth.PortfolioManager.Domain.TradingCalanders
{
    public static class TradingCalanderIds
    {
        public static Guid ASX => new Guid("712E464B-1CE6-4B21-8FB2-D679DFFE3EE3");
    }

    public interface ITradingCalander
    {
        IEnumerable<NonTradingDay> NonTradingDays(int year);
        bool IsTradingDay(Date date);
        IEnumerable<Date> TradingDays(DateRange range);

        void SetNonTradingDays(int year, IEnumerable<NonTradingDay> nonTradingDays);
    }

    public class TradingCalander : TrackedEntity, ITradingCalander
    {
        private List<NonTradingDay> _NonTradingDays = new List<NonTradingDay>();

        public TradingCalander(Guid id)
            : base(id)
        {
        }

        public void SetNonTradingDays(int year, IEnumerable<NonTradingDay> nonTradingDays)
        {
            // Check that each day is in the correct year
            var invalidDate = nonTradingDays.FirstOrDefault(x => x.Date.Year != year);
            if (invalidDate != null)
                throw new ArgumentException(String.Format("Date {0} is not in calander year {1}", invalidDate, year));

            var @event = new NonTradingDaysSetEvent(Id, Version, year, nonTradingDays);
            Apply(@event);

            PublishEvent(@event);
        }
        public void Apply(NonTradingDaysSetEvent @event)
        {
            Version++;

            // Remove any existing non trading days for the year
            _NonTradingDays.RemoveAll(x => x.Date.Year == @event.Year);

            foreach (var nonTradingDay in @event.NonTradingDays)
            {
                var newNonTradingDay = new NonTradingDay(nonTradingDay.Date, nonTradingDay.Description);
                var index = _NonTradingDays.BinarySearch(newNonTradingDay);
                if (index < 0)
                    _NonTradingDays.Insert(~index, newNonTradingDay);
            }
        }

        public IEnumerable<NonTradingDay> NonTradingDays(int year)
        {
            return _NonTradingDays.Where(x => x.Date.Year == year);
        }

        public bool IsTradingDay(Date date)
        {
            return (_NonTradingDays.BinarySearch(new NonTradingDay(date, "")) < 0);
        }

        public IEnumerable<Date> TradingDays(DateRange range)
        {
            return DateUtils.Days(range.FromDate, range.ToDate).Where(x => x.WeekDay() && IsTradingDay(x));
        }

    }

    public class NonTradingDay : IComparable<NonTradingDay>
    {
        public Date Date { get; set; }
        public string Desciption { get; set; }

        public NonTradingDay(Date date, string description)
        {
            Date = date;
            Desciption = description;
        }

        public int CompareTo(NonTradingDay other)
        {
            return Date.CompareTo(other.Date);
        }
    }
}
