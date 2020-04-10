using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Portfolios
{

    public interface ICgtEventCollection : ITransactionList<CgtEvent>
    {
        void Add(Date date, IReadOnlyStock stock, int units, decimal costBase, decimal amountReceived, decimal capitalGain, CgtMethod cgtMethod);
    }

    public class CgtEventCollection : TransactionList<CgtEvent>, ICgtEventCollection
    {
        public void Add(Date date, IReadOnlyStock stock, int units, decimal costBase, decimal amountReceived, decimal capitalGain, CgtMethod cgtMethod)
        {
            Add(new CgtEvent()
            {
                Id = Guid.NewGuid(),
                Date = date,
                Stock = stock,
                Units = units,
                CostBase = costBase,
                AmountReceived = amountReceived,
                CapitalGain = capitalGain,
                CgtMethod = cgtMethod
            });
        }
    }
}
