using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;


namespace Booth.PortfolioManager.Domain.Transactions
{
    class CostBaseAdjustmentHandler
    {
        private IHoldingCollection _Holdings;

        public CostBaseAdjustmentHandler(IHoldingCollection holdings)
        {
            _Holdings = holdings;
        }

        public void ApplyTransaction(IPortfolioTransaction transaction)
        {
            var costBaseAdjustment = transaction as CostBaseAdjustment;
            if (costBaseAdjustment == null)
                throw new ArgumentException("Expected transaction to be an CostBaseAdjustment");

            var holding = _Holdings[costBaseAdjustment.Stock.Id];
            if ((holding == null) || (!holding.IsEffectiveAt(costBaseAdjustment.Date)))
                throw new NoParcelsForTransaction(costBaseAdjustment, "No parcels found for transaction");

            // Adjust cost base of parcels
            foreach (var parcel in holding[costBaseAdjustment.Date])
            {
                var costBaseReduction = (parcel.Properties[costBaseAdjustment.Date].CostBase * (1 - costBaseAdjustment.Percentage)).ToCurrency(RoundingRule.Round);
                parcel.Change(costBaseAdjustment.Date, 0, 0.00m, costBaseReduction, transaction);
            } 
        }
    }
}
