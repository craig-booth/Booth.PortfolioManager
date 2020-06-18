using System;
using System.Collections.Generic;
using System.Text;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Transactions
{
    class UnitCountAdjustmentHandler : ITransactionHandler
    {

        public void Apply(IPortfolioTransaction transaction, IHolding holding, ICashAccount cashAccount)
        {
            var unitCountAdjustment = transaction as UnitCountAdjustment;
            if (unitCountAdjustment == null)
                throw new ArgumentException("Expected transaction to be an UnitCountAdjustment");

            if (!holding.IsEffectiveAt(unitCountAdjustment.Date))
                throw new NoSharesOwnedException("No holdings");

            // Adjust unit count of parcels
            var ratio = (decimal)unitCountAdjustment.NewUnits / (decimal)unitCountAdjustment.OriginalUnits;
            foreach (var parcel in holding.Parcels(unitCountAdjustment.Date))
            {
                var units = (int)Math.Ceiling(parcel.Properties[unitCountAdjustment.Date].Units * ratio);
                parcel.Change(unitCountAdjustment.Date, units, 0.00m, 0.00m, transaction);
            }
        }
    }
}
