using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.EventStore;
using Booth.PortfolioManager.Domain.TradingCalanders;

namespace Booth.PortfolioManager.Web.Services
{
    interface ITradingCalanderService
    {
        ITradingCalander TradingCalander { get; }
        void SetNonTradingDays(int year, IEnumerable<NonTradingDay> nonTradingDays);
    }

    class TradingCalanderService : ITradingCalanderService
    {
        private TradingCalander _TradingCalander;
        private IRepository<TradingCalander> _Repository;

        public ITradingCalander TradingCalander => _TradingCalander;

        public TradingCalanderService(IRepository<TradingCalander> repository, Guid calanderId)
        {
            _Repository = repository;
            _TradingCalander = _Repository.Get(calanderId);
        }

        public void SetNonTradingDays(int year, IEnumerable<NonTradingDay> nonTradingDays)
        {
            _TradingCalander.SetNonTradingDays(year, nonTradingDays);
            _Repository.Update(_TradingCalander);
        }
    }
}
