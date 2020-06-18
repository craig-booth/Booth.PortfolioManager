using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Web.Utilities
{
    interface IStockQuery
    {
        Stock Get(Guid id);
        Stock Get(string asxCode, Date date);
        IEnumerable<Stock> All();
        IEnumerable<Stock> All(Date date);
        IEnumerable<Stock> All(DateRange dateRange);
        IEnumerable<Stock> Find(Func<StockProperties, bool> predicate);
        IEnumerable<Stock> Find(Date date, Func<StockProperties, bool> predicate);
        IEnumerable<Stock> Find(DateRange dateRange, Func<StockProperties, bool> predicate);
    }

    class StockQuery : IStockQuery
    {
        private IEntityCache<Stock> _Cache;

        public StockQuery(IEntityCache<Stock> cache)
        {
            _Cache = cache;
        }

        public Stock Get(Guid id)
        {
            return _Cache.Get(id);
        }

        public Stock Get(string asxCode, Date date)
        {
            return _Cache.All().FirstOrDefault(x => x.IsEffectiveAt(date) && x.Properties.Matches(date, y => y.AsxCode == asxCode));
        }

        public IEnumerable<Stock> All()
        {
            return _Cache.All();
        }

        public IEnumerable<Stock> All(Date date)
        {
            return _Cache.All().Where(x => x.IsEffectiveAt(date));
        }

        public IEnumerable<Stock> All(DateRange dateRange)
        {
            return _Cache.All().Where(x => x.IsEffectiveDuring(dateRange));
        }

        public IEnumerable<Stock> Find(Func<StockProperties, bool> predicate)
        {
            return All().Where(x => x.Properties.Matches(predicate));
        }

        public IEnumerable<Stock> Find(Date date, Func<StockProperties, bool> predicate)
        {
            return All(date).Where(x => x.Properties.Matches(date, predicate));
        }

        public IEnumerable<Stock> Find(DateRange dateRange, Func<StockProperties, bool> predicate)
        {
            return All(dateRange).Where(x => x.Properties.Matches(dateRange, predicate));
        }
    }
}
