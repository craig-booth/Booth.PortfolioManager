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
    class CashTransactionTests
    {

        [TestCase]
        public void Deposit()
        {
            var transaction = new CashTransaction()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 01, 01),
                Comment = "Test Deposit",
                CashTransactionType = BankAccountTransactionType.Deposit,
                Amount = 100.00m
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var cashAccount = mockRepository.Create<ICashAccount>();
            cashAccount.Setup(x => x.AddTransaction(new Date(2020, 01, 01), 100.00m, "Test Deposit", BankAccountTransactionType.Deposit)).Verifiable();

            var handler = new CashTransactionHandler(cashAccount.Object);
            handler.ApplyTransaction(transaction);

            mockRepository.Verify();
        }

        [TestCase]
        public void Fee()
        {
            var transaction = new CashTransaction()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 01, 01),
                Comment = "Test Fee",
                CashTransactionType = BankAccountTransactionType.Fee,
                Amount = 20.00m
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var cashAccount = mockRepository.Create<ICashAccount>();
            cashAccount.Setup(x => x.AddTransaction(new Date(2020, 01, 01), 20.00m, "Test Fee", BankAccountTransactionType.Fee)).Verifiable();

            var handler = new CashTransactionHandler(cashAccount.Object);
            handler.ApplyTransaction(transaction);

            mockRepository.Verify();
        }

        [TestCase]
        public void Interest()
        {
            var transaction = new CashTransaction()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 01, 01),
                Comment = "Test Interest",
                CashTransactionType = BankAccountTransactionType.Interest,
                Amount = 20.00m
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var cashAccount = mockRepository.Create<ICashAccount>();
            cashAccount.Setup(x => x.AddTransaction(new Date(2020, 01, 01), 20.00m, "Test Interest", BankAccountTransactionType.Interest)).Verifiable();

            var handler = new CashTransactionHandler(cashAccount.Object);
            handler.ApplyTransaction(transaction);

            mockRepository.Verify();
        }

        [TestCase]
        public void Transfer()
        {
            var transaction = new CashTransaction()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 01, 01),
                Comment = "Test Transfer",
                CashTransactionType = BankAccountTransactionType.Transfer,
                Amount = 120.00m
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var cashAccount = mockRepository.Create<ICashAccount>();
            cashAccount.Setup(x => x.AddTransaction(new Date(2020, 01, 01), 120.00m, "Test Transfer", BankAccountTransactionType.Transfer)).Verifiable();

            var handler = new CashTransactionHandler(cashAccount.Object);
            handler.ApplyTransaction(transaction);

            mockRepository.Verify();
        }

        [TestCase]
        public void Withdrawl()
        {
            var transaction = new CashTransaction()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 01, 01),
                Comment = "Test Withdrawl",
                CashTransactionType = BankAccountTransactionType.Withdrawl,
                Amount = 200.00m
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var cashAccount = mockRepository.Create<ICashAccount>();
            cashAccount.Setup(x => x.AddTransaction(new Date(2020, 01, 01), 200.00m, "Test Withdrawl", BankAccountTransactionType.Withdrawl)).Verifiable();

            var handler = new CashTransactionHandler(cashAccount.Object);
            handler.ApplyTransaction(transaction);

            mockRepository.Verify();
        }

        [TestCase]
        public void MissingCommnet()
        {
            var transaction = new CashTransaction()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 01, 01),
                Comment = "",
                Amount = 240.00m
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new CashTransactionHandler(cashAccount.Object);
            cashAccount.Setup(x => x.AddTransaction(new Date(2020, 01, 01), 240.00m, "Deposit", BankAccountTransactionType.Deposit)).Verifiable();
            cashAccount.Setup(x => x.AddTransaction(new Date(2020, 01, 01), 240.00m, "Fee", BankAccountTransactionType.Fee)).Verifiable();
            cashAccount.Setup(x => x.AddTransaction(new Date(2020, 01, 01), 240.00m, "Interest", BankAccountTransactionType.Interest)).Verifiable();
            cashAccount.Setup(x => x.AddTransaction(new Date(2020, 01, 01), 240.00m, "Transfer", BankAccountTransactionType.Transfer)).Verifiable();
            cashAccount.Setup(x => x.AddTransaction(new Date(2020, 01, 01), 240.00m, "Withdrawl", BankAccountTransactionType.Withdrawl)).Verifiable();

            transaction.CashTransactionType = BankAccountTransactionType.Deposit;
            handler.ApplyTransaction(transaction);

            transaction.CashTransactionType = BankAccountTransactionType.Fee;
            handler.ApplyTransaction(transaction);

            transaction.CashTransactionType = BankAccountTransactionType.Interest;
            handler.ApplyTransaction(transaction);

            transaction.CashTransactionType = BankAccountTransactionType.Transfer;
            handler.ApplyTransaction(transaction);

            transaction.CashTransactionType = BankAccountTransactionType.Withdrawl;
            handler.ApplyTransaction(transaction);


            mockRepository.Verify();
        }

        [TestCase]
        public void IncorrectTransactionType()
        {
            var transaction = new Aquisition()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 01, 01),
                Comment = ""
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new CashTransactionHandler(cashAccount.Object);

            Assert.That(() => handler.ApplyTransaction(transaction), Throws.ArgumentException);

            mockRepository.Verify();
        }
    }
}
