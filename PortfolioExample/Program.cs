using System;
using System.Linq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;

namespace PortfolioExample
{
    class Program
    {
        static void Main(string[] args)
        {

            

         /*   var portfolioFactory = new PortfolioFactory(new ExampleStockResolver());

            var portfolio = portfolioFactory.CreatePortfolio(Guid.NewGuid(), "test", Guid.NewGuid());

            //Transactions
            portfolio.AddOpeningBalance();     // Change references to Stock to be Guid
            portfolio.AdjustUnitCount();       // Change Transacton handlers to take holding and cash account as parameters
            portfolio.AquireShares();          // CGT events created by parcel
            portfolio.DisposeOfShares();       // This should allow handlers to be injected 
            portfolio.IncomeReceived();        // Change all handlers to have stock id, date as first parameters
            portfolio.MakeCashTransaction();
            portfolio.ReturnOfCapitalReceived();

            portfolio.ChangeDrpParticipation(); 

            // Cash Account
            var cashAccount = portfolio.CashAccount;
            cashAccount.Balance;
            var transaction = cashAccount.Transactions[0];
            transaction.Balance;

            //Holdings
          //  var holding = portfolio.Holdings[Guid.Empty];  // This would be good
            var holding = portfolio.Holdings.Get(Guid.Empty);  //  instead of this
                                                               // IHolldingCollection should not be needed - rename IReadonlyHolding collection

            // Parcels
            var parcel = holding.Parcels().First();             // Why is Parcels a method
            parcel.Audit;                                       // Should this be ITransactionList<ParcelAudit>
                                        // Is IParcel needed (rename IReadonlyParcel) if holding could dspose by id


            portfolio.CalculateIRR();

            // Move Entity, TrackedEntity, EntityFactory and EventList to Booth.EventStore


            var p1 = portfolio as Portfolio;              // These are needed in service to link to the entity store
            p1.FetchEvents();
            p1.ApplyEvents(); */
        }
    }

    class ExampleStockResolver : IStockResolver
    {
        public IReadOnlyStock GetStock(Guid id)
        {
            return new Stock(id);
        }
    }
}
