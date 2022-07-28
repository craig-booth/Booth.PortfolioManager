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
            action.Stock = _Stock;
            base.Add(action);
        }     

    }

}
