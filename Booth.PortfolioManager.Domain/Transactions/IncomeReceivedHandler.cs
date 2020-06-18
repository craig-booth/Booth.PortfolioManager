using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Transactions
{
    class IncomeReceivedHandler : ITransactionHandler
    {
        public void Apply(IPortfolioTransaction transaction, IHolding holding, ICashAccount cashAccount)
        {
            var incomeReceived = transaction as IncomeReceived;
            if (incomeReceived == null)
                throw new ArgumentException("Expected transaction to be an IncomeReceived");

            if (!holding.IsEffectiveAt(incomeReceived.RecordDate))
                throw new NoSharesOwnedException("No holdings");

            // Handle any tax deferred amount recieved 
            if (incomeReceived.TaxDeferred > 0)
            {
                var parcels = holding.Parcels(incomeReceived.RecordDate);

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
                var asxCode = incomeReceived.Stock.Properties[incomeReceived.RecordDate].AsxCode;
                cashAccount.Transfer(incomeReceived.Date, incomeReceived.CashIncome, String.Format("Distribution for {0}", asxCode));
            }

            var drpCashBalance = holding.DrpAccount.Balance(incomeReceived.Date);

            var drpAccountCredit = incomeReceived.DrpCashBalance - drpCashBalance;
            if (drpAccountCredit != 0.00m)
                holding.AddDrpAccountAmount(incomeReceived.Date, drpAccountCredit);
        }
    }
}
