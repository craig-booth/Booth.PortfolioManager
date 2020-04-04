using System;
using System.Collections.Generic;
using System.Text;

using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public class DisposalHandler : ITransactionHandler
    { 
        private HoldingCollection _Holdings;
        private CashAccount _CashAccount;
        private CgtEventCollection _CgtEvents;

        public DisposalHandler(HoldingCollection holdings, CashAccount cashAccount, CgtEventCollection cgtEvents)
        {
            _Holdings = holdings;
            _CashAccount = cashAccount;
            _CgtEvents = cgtEvents;
        }

        public void ApplyTransaction(Transaction transaction)
        {
            var disposal = transaction as Disposal;

            var holding = _Holdings.Get(disposal.Stock.Id);

            // Determine which parcels to sell based on CGT method 
            decimal amountReceived = (disposal.Units * disposal.AveragePrice) - disposal.TransactionCosts;
            var cgtCalculation = CgtCalculatorOld.CalculateCapitalGain(holding.Parcels(disposal.Date), disposal.Date, disposal.Units, amountReceived, disposal.CGTMethod);

            if (cgtCalculation.UnitsSold == 0)
                throw new NoParcelsForTransaction(disposal, "No parcels found for transaction");
            else if (cgtCalculation.UnitsSold < disposal.Units)
                throw new NotEnoughSharesForDisposal(disposal, "Not enough shares for disposal");
                   
            // Dispose of select parcels 
            if (disposal.Stock is StapledSecurity)
            {
             /*     foreach (ParcelSold parcelSold in cgtCalculation.ParcelsSold)
                  {
                      var childStocks = _StockQuery.GetChildStocks(stock.Id, disposal.TransactionDate);

                      // Apportion amount based on NTA of child stocks
                      var amountsReceived = PortfolioUtils.ApportionAmountOverChildStocks(childStocks, disposal.TransactionDate, parcelSold.AmountReceived, _StockQuery);

                      int i = 0;
                      foreach (var childStock in childStocks)
                      {
                          var childParcels = _PortfolioQuery.GetParcelsForStock(childStock.Id, disposal.TransactionDate, disposal.TransactionDate);

                          var childParcel = childParcels.First(x => x.PurchaseId == parcelSold.Parcel.PurchaseId);
                          DisposeOfParcel(unitOfWork, childParcel, disposal.TransactionDate, parcelSold.UnitsSold, amountsReceived[i].Amount, transaction.Id);

                          i++;
                      }

                  };  */
            } 
            else
            {
                foreach (ParcelSoldOld parcelSold in cgtCalculation.ParcelsSold)
                {
                    holding.DisposeOfParcel(parcelSold.Parcel, disposal.Date, parcelSold.UnitsSold, parcelSold.AmountReceived, transaction);

                    _CgtEvents.Add(disposal.Date, disposal.Stock, parcelSold.UnitsSold, parcelSold.CostBase, parcelSold.AmountReceived, parcelSold.CapitalGain, parcelSold.CgtMethod);
                }
            } 
            
            if (disposal.CreateCashTransaction)
            {
                var cost = disposal.Units * disposal.AveragePrice;

                var asxCode = disposal.Stock.Properties[disposal.Date].ASXCode;
                _CashAccount.Transfer(disposal.Date, cost, String.Format("Sale of {0}", asxCode));

                if (disposal.TransactionCosts > 0.00m)
                    _CashAccount.FeeDeducted(disposal.Date, disposal.TransactionCosts, String.Format("Brokerage for sale of {0}", asxCode));
            }
        }
    }
}
