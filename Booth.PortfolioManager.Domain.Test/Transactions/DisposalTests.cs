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
    class DisposalTests
    {

        [TestCase]
        public void NeedToWorkOutHowToHandleCGTCalculation()
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
            var cgtEvents = new CgtEventCollection();

            var handler = new DisposalHandler(holdings, cashAccount, cgtEvents);

            Assert.That(() => handler.ApplyTransaction(transaction), Throws.ArgumentException);
        }

    }
}
