using System;
using System.Linq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public class IncomeReceivedHandler : ITransactionHandler
    {
        private IHoldingCollection _Holdings;
        private ICashAccount _CashAccount;

        public IncomeReceivedHandler(IHoldingCollection holdings, ICashAccount cashAccount)
        {
            _Holdings = holdings;
            _CashAccount = cashAccount;
        }

        public void ApplyTransaction(IPortfolioTransaction transaction)
        {
            var incomeReceived = transaction as IncomeReceived;
            if (incomeReceived == null)
                throw new ArgumentException("Expected transaction to be an IncomeReceived");

            var holding = _Holdings[incomeReceived.Stock.Id];
            if ((holding == null) || (!holding.IsEffectiveAt(incomeReceived.RecordDate)))
                throw new NoParcelsForTransaction(incomeReceived, "No parcels found for transaction");
           
            // Handle any tax deferred amount recieved 
            if (incomeReceived.TaxDeferred > 0)
            {
                var parcels = holding[incomeReceived.RecordDate].ToList();

                // Apportion amount between parcels 
                var apportionedAmounts = parcels.Select(x => new ApportionedCurrencyValue() { Units = x.Properties[incomeReceived.RecordDate].Units }).ToArray();
                MathUtils.ApportionAmount(incomeReceived.TaxDeferred, apportionedAmounts); 

                // Reduce cost base of parcels 
                var i = 0;
                foreach (var parcel in parcels)
                    parcel.Change(incomeReceived.RecordDate, 0, 0.00m, apportionedAmounts[i++].Amount, transaction); 
            }

            if (incomeReceived.CreateCashTransaction)
            {
                var asxCode = incomeReceived.Stock.Properties[incomeReceived.RecordDate].ASXCode;
                _CashAccount.Transfer(incomeReceived.Date, incomeReceived.CashIncome, String.Format("Distribution for {0}", asxCode));
            }

            var drpCashBalance = holding.DrpAccount.Balance(incomeReceived.Date);

            var drpAccountCredit = incomeReceived.DRPCashBalance - drpCashBalance;
            if (drpAccountCredit != 0.00m)
                holding.AddDrpAccountAmount(incomeReceived.Date, drpAccountCredit);
        }
    }
}
