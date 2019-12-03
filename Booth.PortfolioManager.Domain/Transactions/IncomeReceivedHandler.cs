using System;
using System.Collections.Generic;
using System.Text;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public class IncomeReceivedHandler : ITransactionHandler
    {
        private HoldingCollection _Holdings;
        private CashAccount _CashAccount;

        public IncomeReceivedHandler(HoldingCollection holdings, CashAccount cashAccount)
        {
            _Holdings = holdings;
            _CashAccount = cashAccount;
        }

        public void ApplyTransaction(Transaction transaction)
        {
            var incomeReceived = transaction as IncomeReceived;

            var holding = _Holdings.Get(incomeReceived.Stock.Id);
            if ((holding == null) || (!holding.IsEffectiveAt(incomeReceived.RecordDate)))
                throw new NoParcelsForTransaction(incomeReceived, "No parcels found for transaction");

            var holdingProperties = holding.Properties[incomeReceived.RecordDate];
            
            // Handle any tax deferred amount recieved 
            if (incomeReceived.TaxDeferred > 0)
            {
                // Apportion amount between parcels 
             /*   ApportionedCurrencyValue[] apportionedAmounts = new ApportionedCurrencyValue[parcels.Count()];
                int i = 0;
                foreach (ShareParcel parcel in parcels)
                    apportionedAmounts[i++].Units = parcel.Units;
                MathUtils.ApportionAmount(incomeReceived.TaxDeferred, apportionedAmounts); */

                // Reduce cost base of parcels 
             /*   i = 0;
                foreach (ShareParcel parcel in parcels)
                {
                    decimal costBaseReduction = apportionedAmounts[i++].Amount;

                    ReduceParcelCostBase(unitOfWork, parcel, incomeReceived.RecordDate, costBaseReduction, transaction.Id);
                } */
            }

            if (incomeReceived.CreateCashTransaction)
            {
                var asxCode = incomeReceived.Stock.Properties[incomeReceived.RecordDate].ASXCode;
                _CashAccount.Transfer(incomeReceived.Date, incomeReceived.CashIncome, String.Format("Distribution for {0}", asxCode));
            }

            var drpCashBalance = holding.DrpAccount.Balance(incomeReceived.Date);

            var drpAccountCredit = incomeReceived.DRPCashBalance - drpCashBalance;
            holding.AddDrpAccountAmount(incomeReceived.Date, drpAccountCredit);
        }
    }
}
