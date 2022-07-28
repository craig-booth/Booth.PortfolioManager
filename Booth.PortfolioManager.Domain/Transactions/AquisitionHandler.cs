using System;
using System.Collections.Generic;
using System.Text;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Transactions
{
    class AquisitionHandler : ITransactionHandler
    {
        public bool CanCreateHolding => true;

        public void Apply(IPortfolioTransaction transaction, IHolding holding, ICashAccount cashAccount)
        {
            var aquisition = transaction as Aquisition;
            if (aquisition == null)
                throw new ArgumentException("Expected transaction to be an Aquisition");

            if (!aquisition.Stock.IsEffectiveAt(aquisition.Date))
                throw new StockNotActiveException("Stock is not active");

            decimal cost = aquisition.Units * aquisition.AveragePrice;
            decimal amountPaid = cost + aquisition.TransactionCosts;
            decimal costBase = amountPaid;

            holding.AddParcel(aquisition.Date, aquisition.Date, aquisition.Units, amountPaid, costBase, transaction);

            if (aquisition.CreateCashTransaction)
            {
                var asxCode = aquisition.Stock.Properties[aquisition.Date].AsxCode;
                cashAccount.Transfer(aquisition.Date, -cost, String.Format("Purchase of {0}", asxCode));

                if (aquisition.TransactionCosts > 0.00m)
                    cashAccount.FeeDeducted(aquisition.Date, aquisition.TransactionCosts, String.Format("Brokerage for purchase of {0}", asxCode));
            }
        }
    }
}
