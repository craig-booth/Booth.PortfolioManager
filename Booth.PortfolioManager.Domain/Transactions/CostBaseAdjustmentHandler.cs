using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;


namespace Booth.PortfolioManager.Domain.Transactions
{
    class CostBaseAdjustmentHandler : ITransactionHandler
    {
        public bool CanCreateHolding => false;

        public void Apply(IPortfolioTransaction transaction, IHolding holding, ICashAccount cashAccount)
        {
            var costBaseAdjustment = transaction as CostBaseAdjustment;
            if (costBaseAdjustment == null)
                throw new ArgumentException("Expected transaction to be an CostBaseAdjustment");

            if (!holding.IsEffectiveAt(costBaseAdjustment.Date))
                throw new NoSharesOwnedException("No holdings");

            // Adjust cost base of parcels
            foreach (var parcel in holding.Parcels(costBaseAdjustment.Date))
            {
                var costBaseReduction = (parcel.Properties[costBaseAdjustment.Date].CostBase * (1 - costBaseAdjustment.Percentage)).ToCurrency(RoundingRule.Round);
                holding.ReduceParcelCostBase(parcel.Id, costBaseAdjustment.Date, costBaseReduction, transaction);
            }
        }
    }
}
