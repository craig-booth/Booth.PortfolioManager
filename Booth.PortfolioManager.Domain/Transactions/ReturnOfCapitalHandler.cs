using System;
using System.Collections.Generic;
using System.Text;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Transactions
{
    class ReturnOfCapitalHandler : ITransactionHandler
    {
        public bool CanCreateHolding => false;

        public void Apply(IPortfolioTransaction transaction, IHolding holding, ICashAccount cashAccount)
        {
            var returnOfCapital = transaction as ReturnOfCapital;
            if (returnOfCapital == null)
                throw new ArgumentException("Expected transaction to be an ReturnOfCapital");

            if (!holding.IsEffectiveAt(returnOfCapital.RecordDate))
                throw new NoSharesOwnedException("No holdings");

            // Reduce cost base of parcels 
            decimal totalAmount = 0;
            foreach (var parcel in holding.Parcels(returnOfCapital.RecordDate))
            {
                var costBaseReduction = parcel.Properties[returnOfCapital.RecordDate].Units * returnOfCapital.Amount;
                holding.ReduceParcelCostBase(parcel.Id, returnOfCapital.RecordDate, costBaseReduction, transaction);

                totalAmount += costBaseReduction;
            }

            if (returnOfCapital.CreateCashTransaction)
            {
                var asxCode = returnOfCapital.Stock.Properties[returnOfCapital.RecordDate].AsxCode;
                cashAccount.Transfer(returnOfCapital.Date, totalAmount, String.Format("Return of capital for {0}", asxCode));
            }
        }
    }
}
