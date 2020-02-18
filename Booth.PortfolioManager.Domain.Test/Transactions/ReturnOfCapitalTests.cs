using System;
using System.Linq;

using NUnit.Framework;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain.Test.Transactions
{
    class ReturnOfCapitalTests
    {

        [TestCase]
        public void NoSharesOwned()
        {
            Assert.That(false);
        }

        [TestCase]
        public void SingleParcelOwned()
        {
            Assert.That(false);
        }

        [TestCase]
        public void MultipleParcelsOwned()
        {
            Assert.That(false);
        }


        [TestCase]
        public void NoCashTransaction()
        {
            Assert.That(false);
        }


        [TestCase]
        public void IncorrectTransactionType()
        {
            var transaction = new CashTransaction()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 01, 01),
                Comment = "Test Deposit",
                CashTransactionType = BankAccountTransactionType.Deposit,
                Amount = 100.00m
            };

            var holdings = new HoldingCollection();
            var cashAccount = new CashAccount();

            var handler = new ReturnOfCapitalHandler(holdings, cashAccount);

            Assert.That(() => handler.ApplyTransaction(transaction), Throws.ArgumentException); 
        }
    }
}
