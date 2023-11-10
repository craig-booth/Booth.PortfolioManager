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
        Task<ServiceResult<RestApi.TradingCalendars.TradingCalendar>> GetAsync(Guid calendarId, int year);
        Task<ServiceResult> UpdateAsync(Guid calendarId, RestApi.TradingCalendars.TradingCalendar tradingCalendar);
        Task<ServiceResult> SetNonTradingDaysAsync(Guid calendarId, int year, IEnumerable<NonTradingDay> nonTradingDays);
    }

    class TradingCalendarService : ITradingCalendarService
    {
        private ITradingCalendarRepository _Repository;

        public TradingCalendarService(ITradingCalendarRepository repository)
        {
            _Repository = repository;        
        }

        public async Task<ServiceResult<RestApi.TradingCalendars.TradingCalendar>> GetAsync(Guid calendarId, int year)
        {
            var tradingCalendar = await _Repository.GetAsync(calendarId);
            if (tradingCalendar == null)
                return ServiceResult<RestApi.TradingCalendars.TradingCalendar>.NotFound();

            var result = new RestApi.TradingCalendars.TradingCalendar();

            result.Year = year;

            foreach (var nonTradingDay in tradingCalendar.NonTradingDays(year))
                result.AddNonTradingDay(nonTradingDay.Date, nonTradingDay.Description);
            
            return ServiceResult<RestApi.TradingCalendars.TradingCalendar>.Ok(result);
        }
        
        public async Task<ServiceResult> UpdateAsync(Guid calendarId, RestApi.TradingCalendars.TradingCalendar tradingCalendar)
        {
            var nonTradingDays = tradingCalendar.NonTradingDays.Select(x => new NonTradingDay(x.Date, x.Description));

            return await SetNonTradingDaysAsync(calendarId, tradingCalendar.Year, nonTradingDays);
        }

        public async Task<ServiceResult> SetNonTradingDaysAsync(Guid calendarId, int year, IEnumerable<NonTradingDay> nonTradingDays)
        {
            var tradingCalendar = await _Repository.GetAsync(calendarId);
            if (tradingCalendar == null)
                return ServiceResult.NotFound();

            tradingCalendar.SetNonTradingDays(year, nonTradingDays);
            await _Repository.UpdateYearAsync(tradingCalendar, year);

            return ServiceResult.Ok();
        }

    }
}
