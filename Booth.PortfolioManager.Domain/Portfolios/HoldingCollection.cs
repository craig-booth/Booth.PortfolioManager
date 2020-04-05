using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Portfolios
{

    public interface IReadOnlyHoldingCollection
    {
        IReadOnlyHolding Get(Guid stockId);
        IEnumerable<IReadOnlyHolding> All();
        IEnumerable<IReadOnlyHolding> All(Date date);
        IEnumerable<IReadOnlyHolding> All(DateRange dateRange);
        IEnumerable<IReadOnlyHolding> Find(Date date, Func<HoldingProperties, bool> predicate);
        IEnumerable<IReadOnlyHolding> Find(DateRange dateRange, Func<HoldingProperties, bool> predicate);
    }

    public interface IHoldingCollection : IReadOnlyHoldingCollection
    {
        IHolding this[Guid stockId] { get; }

        IHolding Add(Stock stock, Date fromDate);
    }

    public class HoldingCollection : IHoldingCollection, IReadOnlyHoldingCollection
    {
        private Dictionary<Guid, Holding> _Holdings = new Dictionary<Guid, Holding>();

        public IHolding this[Guid stockId]
        {
            get
            {
                if (_Holdings.ContainsKey(stockId))
                    return _Holdings[stockId];
                else
                    return null;
            }
        }

        public IEnumerable<IReadOnlyHolding> All()
        {
            return _Holdings.Values;
        }

        public IEnumerable<IReadOnlyHolding> All(Date date)
        {
            return _Holdings.Values.Where(x => x.IsEffectiveAt(date));
        }

        public IEnumerable<IReadOnlyHolding> All(DateRange dateRange)
        {
            return _Holdings.Values.Where(x => x.IsEffectiveDuring(dateRange));
        }

        public IEnumerable<IReadOnlyHolding> Find(Date date, Func<HoldingProperties, bool> predicate)
        {
            return _Holdings.Values.Where(x => x.IsEffectiveAt(date) && x.Properties.Matches(predicate));
        }

        public IEnumerable<IReadOnlyHolding> Find(DateRange dateRange, Func<HoldingProperties, bool> predicate)
        {
            return _Holdings.Values.Where(x => x.IsEffectiveDuring(dateRange) && x.Properties.Matches(predicate));
        }

        public IReadOnlyHolding Get(Guid stockId)
        {
            if (_Holdings.ContainsKey(stockId))
                return _Holdings[stockId];
            else
                return null;
        }

        public IHolding Add(Stock stock, Date fromDate)
        {
            var holding = new Holding(stock, fromDate);
            _Holdings.Add(stock.Id, holding);

            return holding;
        }
    }
}
