using System;
using System.Collections.Generic;
using System.Text;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Transactions
{
    class ReturnOfCapitalHandler : ITransactionHandler
    {
        private HoldingCollection _Holdings;
        private CashAccount _CashAccount;

        public ReturnOfCapitalHandler(HoldingCollection holdings, CashAccount cashAccount)
        {
            _Holdings = holdings;
            _CashAccount = cashAccount;
        }

        public void ApplyTransaction(Transaction transaction)
        {
            var returnOfCapital = transaction as ReturnOfCapital;

            var holding = _Holdings.Get(returnOfCapital.Stock.Id);
            if ((holding == null) || (!holding.IsEffectiveAt(returnOfCapital.RecordDate)))
                throw new NoParcelsForTransaction(returnOfCapital, "No parcels found for transaction");

            var holdingProperties = holding.Properties[returnOfCapital.RecordDate];

            /* Reduce cost base of parcels */
            decimal totalAmount = 0;
            foreach (var parcel in holding.Parcels(returnOfCapital.RecordDate))
            {
                var parcelProperties = parcel.Properties[returnOfCapital.RecordDate];

                var costBaseReduction = parcelProperties.Units * returnOfCapital.Amount;

              /*  parcel.
                ReduceParcelCostBase(unitOfWork, parcel, returnOfCapital.RecordDate, costBaseReduction, transaction.Id);
                */

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
