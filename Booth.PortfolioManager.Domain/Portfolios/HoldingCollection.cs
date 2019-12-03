using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Portfolios
{

    public interface IHoldingCollection
    {
        Holding Get(Guid stockId);

        IEnumerable<Holding> All();
        IEnumerable<Holding> All(Date date);
        IEnumerable<Holding> All(DateRange dateRange);
        IEnumerable<Holding> Find(Date date, Func<HoldingProperties, bool> predicate);
        IEnumerable<Holding> Find(DateRange dateRange, Func<HoldingProperties, bool> predicate);
    }

    public class HoldingCollection : IHoldingCollection
    {
        private Dictionary<Guid, Holding> _Holdings = new Dictionary<Guid, Holding>();


        public IEnumerable<Holding> All()
        {
            return _Holdings.Values;
        }

        public IEnumerable<Holding> All(Date date)
        {
            return _Holdings.Values.Where(x => x.IsEffectiveAt(date));
        }

        public IEnumerable<Holding> All(DateRange dateRange)
        {
            return _Holdings.Values.Where(x => x.IsEffectiveDuring(dateRange));
        }

        public IEnumerable<Holding> Find(Date date, Func<HoldingProperties, bool> predicate)
        {
            return _Holdings.Values.Where(x => x.IsEffectiveAt(date) && x.Properties.Matches(predicate));
        }

        public IEnumerable<Holding> Find(DateRange dateRange, Func<HoldingProperties, bool> predicate)
        {
            return _Holdings.Values.Where(x => x.IsEffectiveDuring(dateRange) && x.Properties.Matches(predicate));
        }

        public Holding Get(Guid stockId)
        {
            if (_Holdings.ContainsKey(stockId))
                return _Holdings[stockId];
            else
                return null;
        }

        public Holding Add(Stock stock, Date fromDate)
        {
            var holding = new Holding(stock, fromDate);
            _Holdings.Add(stock.Id, holding);

            return holding;
        }
    }
}
