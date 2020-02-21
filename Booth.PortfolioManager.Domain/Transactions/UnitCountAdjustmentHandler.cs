using System;
using System.Collections.Generic;
using System.Text;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Transactions
{
    class UnitCountAdjustmentHandler
    {
        private IHoldingCollection _Holdings;

        public UnitCountAdjustmentHandler(IHoldingCollection holdings)
        {
            _Holdings = holdings;
        }

        public void ApplyTransaction(Transaction transaction)
        {
            var unitCountAdjustment = transaction as UnitCountAdjustment;
            if (unitCountAdjustment == null) 
                throw new ArgumentException("Expected transaction to be an UnitCountAdjustment"); 

            var holding = _Holdings[unitCountAdjustment.Stock.Id];
            if ((holding == null) || (!holding.IsEffectiveAt(unitCountAdjustment.Date)))
                throw new NoParcelsForTransaction(unitCountAdjustment, "No parcels found for transaction");

            // Adjust unit count of parcels
            var ratio = (decimal)unitCountAdjustment.NewUnits / (decimal)unitCountAdjustment.OriginalUnits;
            foreach (var parcel in holding[unitCountAdjustment.Date])
            {
                var units = (int)Math.Ceiling(parcel.Properties[unitCountAdjustment.Date].Units * ratio);
                parcel.Change(unitCountAdjustment.Date, units, 0.00m, 0.00m, transaction);
            } 
        }
    }
}
