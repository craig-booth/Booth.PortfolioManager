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

        public TradingCalendarService(IRepository<TradingCalendar> repository, Guid calendarId)
        {
            _Repository = repository;

            try
            {
                _TradingCalendar = _Repository.Get(calendarId);
            }
            catch
            {
                _TradingCalendar = new TradingCalendar(calendarId);
            }
            
        }

        public ServiceResult SetNonTradingDays(int year, IEnumerable<NonTradingDay> nonTradingDays)
        {
            _TradingCalendar.SetNonTradingDays(year, nonTradingDays);
            _Repository.Update(_TradingCalendar);

            return ServiceResult.Ok();
        }
    }
}
