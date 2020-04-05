using System;
using System.Collections.Generic;
using System.Text;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public class AquisitionHandler : ITransactionHandler
    {
        private IHoldingCollection _Holdings;
        private ICashAccount _CashAccount;

        public AquisitionHandler(IHoldingCollection holdings, ICashAccount cashAccount)
        {
            _Holdings = holdings;
            _CashAccount = cashAccount;
        }

        public void ApplyTransaction(IPortfolioTransaction transaction)
        {
            var aquisition = transaction as Aquisition;
            if (aquisition == null)
                throw new ArgumentException("Expected transaction to be an Aquisition");

            var holding = _Holdings[aquisition.Stock.Id];
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
