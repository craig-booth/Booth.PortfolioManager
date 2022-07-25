using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.Repository;

namespace Booth.PortfolioManager.IntegrationTest.TestFixture
{
    internal class InMemoryTradingCalendarRepository : InMemoryRepository<TradingCalendar>, ITradingCalendarRepository
    {
        public void UpdateYear(TradingCalendar calendar, int year)
        {
            //
        }
    }
}
