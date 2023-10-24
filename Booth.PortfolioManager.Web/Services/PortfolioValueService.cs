using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.RestApi.Portfolios;
using Booth.PortfolioManager.RestApi.Stocks;
using Booth.PortfolioManager.Web.Utilities;

namespace Booth.PortfolioManager.Web.Services
{

    public interface IPortfolioValueService
    {
        Task<ServiceResult<PortfolioValueResponse>> GetValue(DateRange dateRange, ValueFrequency frequency);
        Task<ServiceResult<PortfolioValueResponse>> GetValue(Guid stockId, DateRange dateRange, ValueFrequency frequency);
    }

    public class PortfolioValueService : IPortfolioValueService
    {
        private readonly IReadOnlyPortfolio _Portfolio;
        private readonly ITradingCalendarRepository _TradingCalendarRepository;

        public PortfolioValueService(IReadOnlyPortfolio portfolio, ITradingCalendarRepository tradingCalendarRepository)
        {
            _Portfolio = portfolio;
            _TradingCalendarRepository = tradingCalendarRepository;
        }
 
        public async Task<ServiceResult<PortfolioValueResponse>> GetValue(DateRange dateRange, ValueFrequency frequency)
        {
            if (_Portfolio == null)
                return ServiceResult<PortfolioValueResponse>.NotFound();

            var response = new PortfolioValueResponse();

            var tradingCalendar = await _TradingCalendarRepository.GetAsync(TradingCalendarIds.ASX);
            var dates = GetDates(tradingCalendar, dateRange, frequency);

            var holdings = _Portfolio.Holdings.All(dateRange);

            var closingBalances = _Portfolio.CashAccount.EffectiveBalances(dateRange);
            var closingBalanceEnumerator = closingBalances.GetEnumerator();
            closingBalanceEnumerator.MoveNext();

            foreach (var date in dates)
            {
                var amount = 0.00m;

                // Add holding values
                foreach (var holding in holdings)
                    amount += holding.Value(date);

                // Add cash account balances
                if (date > closingBalanceEnumerator.Current.EffectivePeriod.ToDate)
                    closingBalanceEnumerator.MoveNext();
                amount += closingBalanceEnumerator.Current.Balance;

                var value = new ClosingPrice()
                {
                    Date = date,
                    Price = amount
                };

                response.Values.Add(value);
            }

            return ServiceResult<PortfolioValueResponse>.Ok(response);
        }

        public async Task<ServiceResult<PortfolioValueResponse>> GetValue(Guid stockId, DateRange dateRange, ValueFrequency frequency)
        {
            if (_Portfolio == null)
                return ServiceResult<PortfolioValueResponse>.NotFound();

            var holding = _Portfolio.Holdings[stockId];
            if (holding == null)
                return ServiceResult<PortfolioValueResponse>.NotFound();

            var response = new PortfolioValueResponse();

            var tradingCalendar = await _TradingCalendarRepository.GetAsync(TradingCalendarIds.ASX);
            var dates = GetDates(tradingCalendar, dateRange, frequency);

            foreach (var date in dates)
            {
                var value = new ClosingPrice()
                {
                    Date = date,
                    Price = holding.Value(date)
                };

                response.Values.Add(value);
            }

            return ServiceResult<PortfolioValueResponse>.Ok(response);
        } 

        private IEnumerable<Date> GetDates(ITradingCalendar tradingCalendar, DateRange dateRange, ValueFrequency frequency)
        {
            var firstRequestedDate = tradingCalendar.NextTradingDay(dateRange.FromDate);
            var lastRequestedDate = tradingCalendar.PreviousTradingDay(dateRange.ToDate);

            IEnumerable<Date> dates;
            if (frequency == ValueFrequency.Day)
                dates= tradingCalendar.TradingDays(dateRange);
            else if (frequency == ValueFrequency.Week)
                dates = DateUtils.WeekEndingDays(dateRange);
            else if (frequency == ValueFrequency.Month)
                dates = DateUtils.MonthEndingDays(dateRange);
            else
                dates = new Date[] { firstRequestedDate, lastRequestedDate };


            if (firstRequestedDate < dates.First())
                yield return firstRequestedDate;

            Date lastDate = lastRequestedDate;
            foreach (var date in dates)
            {     
                yield return date;
                lastDate = date;
            }

            if (lastDate < lastRequestedDate)
                yield return lastRequestedDate;
        }

    } 

}
