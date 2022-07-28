using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

namespace Booth.PortfolioManager.Domain.TradingCalendars
{
    public static class TradingCalendarIds
    {
        public static Guid ASX => new Guid("712E464B-1CE6-4B21-8FB2-D679DFFE3EE3");
    }

    public interface ITradingCalendar
    {
        IEnumerable<int> Years { get; }
        IEnumerable<NonTradingDay> NonTradingDays(int year);
        bool IsTradingDay(Date date);
        Date NextTradingDay(Date date);
        Date PreviousTradingDay(Date date);

        IEnumerable<Date> TradingDays(DateRange range);

        void SetNonTradingDays(int year, IEnumerable<NonTradingDay> nonTradingDays);
    }

    public class TradingCalendar : IEntity, ITradingCalendar
    {
        private List<NonTradingDay> _NonTradingDays = new List<NonTradingDay>();

        public Guid Id { get; }

        public TradingCalendar(Guid id)
        {
            Id = id;
        }

        public void SetNonTradingDays(int year, IEnumerable<NonTradingDay> nonTradingDays)
        {
            // Check that each day is in the correct year
            var invalidDate = nonTradingDays.FirstOrDefault(x => x.Date.Year != year);
            if (invalidDate != null)
                throw new ArgumentException(String.Format("Date {0} is not in calendar year {1}", invalidDate, year));

            // Remove any existing non trading days for the year
            _NonTradingDays.RemoveAll(x => x.Date.Year == year);

            foreach (var nonTradingDay in nonTradingDays)
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

        public IEnumerable<int> Years 
        { 
            get
            {
                return _NonTradingDays.Select(x => x.Date.Year).Distinct();
            }
        }

        public bool IsTradingDay(Date date)
        {
            if (!date.WeekDay())
                return false;
            else
                return (_NonTradingDays.BinarySearch(new NonTradingDay(date, "")) < 0);
        }

        public Date NextTradingDay(Date date)
        {
            var nextTradingDay = date;
            while (!IsTradingDay(nextTradingDay))
                nextTradingDay = nextTradingDay.AddDays(1);

            return nextTradingDay;
        }

        public Date PreviousTradingDay(Date date)
        {
            var previousTradingDay = date;
            while (!IsTradingDay(previousTradingDay))
                previousTradingDay = previousTradingDay.AddDays(-1);

            return previousTradingDay;
        }

        public IEnumerable<Date> TradingDays(DateRange range)
        {
            return DateUtils.Days(range.FromDate, range.ToDate).Where(x => IsTradingDay(x));
        }

    }

    public class NonTradingDay : IComparable<NonTradingDay>
    {
        public Date Date { get; set; }
        public string Description { get ; set; }

        public NonTradingDay(Date date, string description)
        {
            Date = date;
            Description = description;
        }

        public int CompareTo(NonTradingDay other)
        {
            return Date.CompareTo(other.Date);
        }
    }
}
