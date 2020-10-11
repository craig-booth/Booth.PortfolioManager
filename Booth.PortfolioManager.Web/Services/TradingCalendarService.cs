using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.EventStore;
using Booth.PortfolioManager.Domain.TradingCalendars;

namespace Booth.PortfolioManager.Web.Services
{
    public interface ITradingCalendarService
    {
        ITradingCalendar TradingCalendar { get; }

        ServiceResult<RestApi.TradingCalendars.TradingCalendar> Get(int year);
        ServiceResult Update(RestApi.TradingCalendars.TradingCalendar tradingCalendar);
        void SetNonTradingDays(int year, IEnumerable<NonTradingDay> nonTradingDays);
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

        public ServiceResult<RestApi.TradingCalendars.TradingCalendar> Get(int year)
        {
            var result = new RestApi.TradingCalendars.TradingCalendar();

            result.Year = year;

            foreach (var nonTradingDay in _TradingCalendar.NonTradingDays(year))
                result.AddNonTradingDay(nonTradingDay.Date, nonTradingDay.Desciption);
            
            return ServiceResult<RestApi.TradingCalendars.TradingCalendar>.Ok(result);
        }

        public ServiceResult Update(RestApi.TradingCalendars.TradingCalendar tradingCalendar)
        {
            var nonTradingDays = tradingCalendar.NonTradingDays.Select(x => new NonTradingDay(x.Date, x.Description));

            SetNonTradingDays(tradingCalendar.Year, nonTradingDays);

            return ServiceResult.Ok();
        }

        public void SetNonTradingDays(int year, IEnumerable<NonTradingDay> nonTradingDays)
        {
            _TradingCalendar.SetNonTradingDays(year, nonTradingDays);
            _Repository.Update(_TradingCalendar);
        }
    }
}
