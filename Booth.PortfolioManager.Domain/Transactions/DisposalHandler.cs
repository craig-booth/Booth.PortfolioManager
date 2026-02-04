using System;
using System.Collections.Generic;
using System.Text;

using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Transactions
{
    class DisposalHandler : ITransactionHandler
    {
        public bool CanCreateHolding => false;

        public void Apply(IPortfolioTransaction transaction, IHolding holding, ICashAccount cashAccount)
        {
            var disposal = transaction as Disposal;
            if (disposal == null)
                throw new ArgumentException("Expected transaction to be a Disposal");

            if (!holding.IsEffectiveAt(disposal.Date))
                throw new NoSharesOwnedException("No holdings");

            if (holding.Properties[disposal.Date].Units < disposal.Units)
                throw new NotEnoughSharesForDisposal("Not enough shares for disposal");

            // Determine which parcels to sell based on CGT method 
            decimal amountReceived = (disposal.Units * disposal.AveragePrice) - disposal.TransactionCosts;

            var cgtCalculator = new CgtCalculator();
            var parcelsSold = cgtCalculator.Calculate(holding.Parcels(disposal.Date), disposal.Date, disposal.Units, amountReceived, CgtCalculator.GetCgtComparer(disposal.Date, disposal.CgtMethod));

            // Dispose of select parcels 
            foreach (var parcelSold in parcelsSold)
                holding.DisposeOfParcel(parcelSold.Parcel.Id, disposal.Date, parcelSold.UnitsSold, parcelSold.AmountReceived, parcelSold.CapitalGain, parcelSold.CgtMethod, transaction);

            if (disposal.CreateCashTransaction)
            {
                var cost = disposal.Units * disposal.AveragePrice;

                var asxCode = disposal.Stock.Properties[disposal.Date].AsxCode;
                cashAccount.Transfer(disposal.Date, cost, String.Format("Sale of {0}", asxCode));

                if (disposal.TransactionCosts > 0.00m)
                    cashAccount.FeeDeducted(disposal.Date, disposal.TransactionCosts, String.Format("Brokerage for sale of {0}", asxCode));
            }
        }
    }
}
