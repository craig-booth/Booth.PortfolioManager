using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public interface IPortfolioTransactionList : ITransactionList<IPortfolioTransaction>
    {
        IEnumerable<IPortfolioTransaction> ForHolding(Guid stockId);
        IEnumerable<IPortfolioTransaction> ForHolding(Guid stockId, Date date);
        IEnumerable<IPortfolioTransaction> ForHolding(Guid stockId, DateRange dateRange);
    }

    class PortfolioTransactionList : TransactionList<IPortfolioTransaction>, IPortfolioTransactionList
    {

        public new void Add(IPortfolioTransaction transaction)
        {
            base.Add(transaction);
        }

        public IEnumerable<IPortfolioTransaction> ForHolding(Guid stockId)
        {
            return this.Where(x => (x.Stock != null) && (x.Stock.Id == stockId));
        }

        public IEnumerable<IPortfolioTransaction> ForHolding(Guid stockId, Date date)
        {
            return ForDate(date).Where(x => (x.Stock != null) && (x.Stock.Id == stockId));
        }

        public IEnumerable<IPortfolioTransaction> ForHolding(Guid stockId, DateRange dateRange)
        {
            return InDateRange(dateRange).Where(x => (x.Stock != null) && (x.Stock.Id == stockId));
        }
    }
}
