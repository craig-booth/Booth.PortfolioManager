using System;
using System.Collections.Generic;
using System.Text;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public class AquisitionHandler : ITransactionHandler
    {
        private HoldingCollection _Holdings;
        private CashAccount _CashAccount;

        public AquisitionHandler(HoldingCollection holdings, CashAccount cashAccount)
        {
            _Holdings = holdings;
            _CashAccount = cashAccount;
        }

        public void ApplyTransaction(Transaction transaction)
        {
            var aquisition = transaction as Aquisition;

            var holding = _Holdings.Get(aquisition.Stock.Id);
            if (holding == null)
            {
                holding = _Holdings.Add(aquisition.Stock, aquisition.Date);
            }

            decimal cost = aquisition.Units * aquisition.AveragePrice;
            decimal amountPaid = cost + aquisition.TransactionCosts;
            decimal costBase = amountPaid;

            holding.AddParcel(aquisition.Date, aquisition.Date, aquisition.Units, amountPaid, costBase, transaction);

            if (aquisition.CreateCashTransaction)
            {
                var asxCode = aquisition.Stock.Properties[aquisition.Date].ASXCode;
                _CashAccount.Transfer(aquisition.Date, -cost, String.Format("Purchase of {0}", asxCode));

                if (aquisition.TransactionCosts > 0.00m)
                    _CashAccount.FeeDeducted(aquisition.Date, aquisition.TransactionCosts, String.Format("Brokerage for purchase of {0}", asxCode));
            }
        }


    }
}
