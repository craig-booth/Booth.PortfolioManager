using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Stocks
{

    public interface ICorporateActionList : ITransactionList<CorporateAction>
    {
        void Add(CorporateAction action);
        void Update(CorporateAction action);
        void Remove(Guid id);
    }

    public class CorporateActionList : TransactionList<CorporateAction>, ICorporateActionList
    {
        private Stock _Stock;
        public IReadOnlyStock Stock { get { return _Stock; } }


        internal CorporateActionList(Stock stock)
        {
            _Stock = stock;
        }

        public new void Add(CorporateAction action)
        {
            if (action.Stock == null)
                action.Stock = _Stock;

            base.Add(action);
        }

        public new void Update(CorporateAction action)
        {
            if (action.Stock == null)
                action.Stock = _Stock;
            else if (base[action.Id].Stock != action.Stock)
                throw new StockChangedException("Cannot change the stock of a corporate action");

            base.Update(action);
        }

        public new void Remove(Guid id)
        {
            base.Remove(id);
        }

    }

}
