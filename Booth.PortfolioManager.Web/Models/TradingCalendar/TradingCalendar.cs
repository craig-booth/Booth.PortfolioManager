using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Models.TradingCalendar
{
    public class TradingCalendar
    {
        public int Year { get; set; }
        public List<NonTradingDay> NonTradingDays { get; } = new List<NonTradingDay>();

        public void AddNonTradingDay(Date date, string description)
        {
            var nonTradingDay = new NonTradingDay()
            {
                Date = date,
                Description = description
            };
            NonTradingDays.Add(nonTradingDay);
        }

        public class NonTradingDay
        {
            public Date Date { get; set; }
            public string Description { get; set; }
        }
    }
}
