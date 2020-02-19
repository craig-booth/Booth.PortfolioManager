using System;
using System.Collections.Generic;
using System.Text;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Transactions
{
    class ReturnOfCapitalHandler : ITransactionHandler
    {
        private IHoldingCollection _Holdings;
        private ICashAccount _CashAccount;

        public ReturnOfCapitalHandler(IHoldingCollection holdings, ICashAccount cashAccount)
        {
            _Holdings = holdings;
            _CashAccount = cashAccount;
        }

        public void ApplyTransaction(Transaction transaction)
        {
            var returnOfCapital = transaction as ReturnOfCapital;
            if (returnOfCapital == null)
                throw new ArgumentException("Expected transaction to be an ReturnOfCapital");

            var holding = _Holdings[returnOfCapital.Stock.Id];
            if ((holding == null) || (!holding.IsEffectiveAt(returnOfCapital.RecordDate)))
                throw new NoParcelsForTransaction(returnOfCapital, "No parcels found for transaction");

            /* Reduce cost base of parcels */
            decimal totalAmount = 0;
            foreach (var parcel in holding[returnOfCapital.RecordDate])
            {
                var costBaseReduction = parcel.Properties[returnOfCapital.RecordDate].Units * returnOfCapital.Amount;
                parcel.Change(returnOfCapital.RecordDate, 0, 0.00m, costBaseReduction, transaction);

                totalAmount += costBaseReduction;
            } 

            if (returnOfCapital.CreateCashTransaction)
            {
                var asxCode = returnOfCapital.Stock.Properties[returnOfCapital.RecordDate].ASXCode;
                _CashAccount.Transfer(returnOfCapital.Date, totalAmount, String.Format("Return of capital for {0}", asxCode));
            }
        }
    }
}
