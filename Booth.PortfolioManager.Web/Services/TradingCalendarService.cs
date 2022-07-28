using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.Web.Utilities;

namespace Booth.PortfolioManager.Web.Services
{
    public interface ITradingCalendarService
    {
        ServiceResult<RestApi.TradingCalendars.TradingCalendar> Get(Guid calendarId, int year);
        ServiceResult Update(Guid calendarId, RestApi.TradingCalendars.TradingCalendar tradingCalendar);
        ServiceResult SetNonTradingDays(Guid calendarId, int year, IEnumerable<NonTradingDay> nonTradingDays);
    }

    class TradingCalendarService : ITradingCalendarService
    {
        private ITradingCalendarRepository _Repository;
        private IEntityCache<TradingCalendar> _Cache;

        public TradingCalendarService(IEntityCache<TradingCalendar> cache, ITradingCalendarRepository repository)
        {
            _Repository = repository;
            _Cache = cache;          
        }

        public ServiceResult<RestApi.TradingCalendars.TradingCalendar> Get(Guid calendarId, int year)
        {
            var tradingCalendar = _Cache.Get(calendarId);
            if (tradingCalendar == null)
                return ServiceResult<RestApi.TradingCalendars.TradingCalendar>.NotFound();

            var result = new RestApi.TradingCalendars.TradingCalendar();

            result.Year = year;

            foreach (var nonTradingDay in tradingCalendar.NonTradingDays(year))
                result.AddNonTradingDay(nonTradingDay.Date, nonTradingDay.Description);
            
            return ServiceResult<RestApi.TradingCalendars.TradingCalendar>.Ok(result);
        }

        public ServiceResult Update(Guid calendarId, RestApi.TradingCalendars.TradingCalendar tradingCalendar)
        {
            var nonTradingDays = tradingCalendar.NonTradingDays.Select(x => new NonTradingDay(x.Date, x.Description));

            return SetNonTradingDays(calendarId, tradingCalendar.Year, nonTradingDays);
        }

        public ServiceResult SetNonTradingDays(Guid calendarId, int year, IEnumerable<NonTradingDay> nonTradingDays)
        {
            var tradingCalendar = _Cache.Get(calendarId);
            if (tradingCalendar == null)
                return ServiceResult.NotFound();

            tradingCalendar.SetNonTradingDays(year, nonTradingDays);
            _Repository.UpdateYear(tradingCalendar, year);

            return ServiceResult.Ok();
        }

    }
}
