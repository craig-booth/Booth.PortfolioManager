using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Portfolios
{

    public interface IHoldingCollection
    {
        IReadOnlyHolding this[Guid stockId] { get; }
        IEnumerable<IReadOnlyHolding> All();
        IEnumerable<IReadOnlyHolding> All(Date date);
        IEnumerable<IReadOnlyHolding> All(DateRange dateRange);

        void Clear();
    }

    class HoldingCollection : IHoldingCollection
    {
        private Dictionary<Guid, Holding> _Holdings = new Dictionary<Guid, Holding>();

        IReadOnlyHolding IHoldingCollection.this[Guid stockId]
        {
            get { return this[stockId];  }
        }

        public Holding this[Guid stockId]
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

        public IReadOnlyHolding Get(Guid stockId)
        {
            return this[stockId];
        }

        public Holding Add(IReadOnlyStock stock, Date fromDate)
        {
            var holding = new Holding(stock, fromDate);
            _Holdings.Add(stock.Id, holding);

            return holding;
        }

        public void Clear()
        {
            _Holdings.Clear();
        }
    }
}
