using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.EventStore;
using Booth.PortfolioManager.Domain.TradingCalendars;

namespace Booth.PortfolioManager.Web.Services
{
    interface ITradingCalendarService
    {
        ITradingCalendar TradingCalendar { get; }
        ServiceResult SetNonTradingDays(int year, IEnumerable<NonTradingDay> nonTradingDays);
    }

    class TradingCalendarService : ITradingCalendarService
    {
        private TradingCalendar _TradingCalendar;
        private IRepository<TradingCalendar> _Repository;

        public ITradingCalendar TradingCalendar => _TradingCalendar;

        public TradingCalendarService(IRepository<TradingCalendar> repository, Guid CalendarId)
        {
            _Repository = repository;
            _TradingCalendar = _Repository.Get(CalendarId);
        }

        public ServiceResult SetNonTradingDays(int year, IEnumerable<NonTradingDay> nonTradingDays)
        {
            _TradingCalendar.SetNonTradingDays(year, nonTradingDays);
            _Repository.Update(_TradingCalendar);

            return ServiceResult.Ok();
        }
    }
}
