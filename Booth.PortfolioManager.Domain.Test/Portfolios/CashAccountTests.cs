using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;


namespace Booth.PortfolioManager.Domain.Test.Portfolios
{
    class CashAccountTests
    { 
        [TestCase]
        public void BalanceWithNoTransactions()
        {
            var account = new CashAccount();

            Assert.That(account.Balance, Is.EqualTo(0));
        }

        [TestCase]
        public void Balance()
        {
            var account = new CashAccount();
            account.Deposit(new Date(2000, 01, 01), 100.00m, "");
            account.Deposit(new Date(2000, 02, 01), 200.00m, "");
            account.Withdraw(new Date(2000, 03, 01), 50.00m, "");

            Assert.That(account.Balance, Is.EqualTo(250.00m));
        }

        [TestCase]
        public void BalanceAtSpecificDateWithNoTransactions()
        {
            var account = new CashAccount();
            account.Deposit(new Date(2000, 01, 01), 100.00m, "");
            account.Deposit(new Date(2000, 02, 01), 200.00m, "");
            account.Withdraw(new Date(2000, 03, 01), 50.00m, "");

            Assert.That(account.Balance(new Date(2000, 02, 15)), Is.EqualTo(300.00m));
        }

        [TestCase]
        public void BalanceBeforeFirstTransaction()
        {
            var account = new CashAccount();
            account.Deposit(new Date(2000, 01, 01), 100.00m, "");
            account.Deposit(new Date(2000, 02, 01), 200.00m, "");
            account.Withdraw(new Date(2000, 03, 01), 50.00m, "");

            Assert.That(account.Balance(new Date(1999, 01, 01)), Is.EqualTo(0.00m));
        }

        [TestCase]
        public void BalanceAfterLastTransaction()
        {
            var account = new CashAccount();
            account.Deposit(new Date(2000, 01, 01), 100.00m, "");
            account.Deposit(new Date(2000, 02, 01), 200.00m, "");
            account.Withdraw(new Date(2000, 03, 01), 50.00m, "");

            Assert.That(account.Balance(new Date(2002, 01, 01)), Is.EqualTo(250.00m));
        }
        [TestCase]
        public void BalanceMultipleTransactionsOnThatDate()
        {
            var account = new CashAccount();
            account.Deposit(new Date(2000, 01, 01), 100.00m, "");
            account.Deposit(new Date(2000, 02, 01), 200.00m, "");
            account.Deposit(new Date(2000, 02, 01), 60.00m, "");
            account.Withdraw(new Date(2000, 03, 01), 50.00m, "");

            Assert.That(account.Balance(new Date(2000, 02, 01)), Is.EqualTo(360.00m));
        }

        [TestCase]
        public void BalanceNoTransactionsOnThatDate()
        {
            var account = new CashAccount();
            account.Deposit(new Date(2000, 01, 01), 100.00m, "");
            account.Deposit(new Date(2000, 02, 01), 200.00m, "");
            account.Deposit(new Date(2000, 02, 01), 60.00m, "");
            account.Withdraw(new Date(2000, 03, 01), 50.00m, "");

            Assert.That(account.Balance(new Date(2000, 02, 15)), Is.EqualTo(360.00m));
        }

        [TestCase]
        public void EffectiveBalancesNoTransactions()
        {
            var account = new CashAccount();

            var result = account.EffectiveBalances(new DateRange(new Date(2000, 01, 01), new Date(2010, 01, 01))).ToArray();

            var expectedResult = new CashAccountEffectiveBalance[]
            {
                new CashAccountEffectiveBalance(new Date(2000, 01, 01), new Date(2010, 01, 01), 0.00m)
            };

            Assert.That(result, Is.EqualTo(expectedResult).Using(new EffectiveBalanceComparer()));
        }

        [TestCase]
        public void EffectiveBalancesMatchingStartAndEndDates()
        {
            var account = new CashAccount();
            account.Deposit(new Date(2000, 01, 01), 100.00m, "");
            account.Deposit(new Date(2000, 02, 01), 200.00m, "");
            account.Deposit(new Date(2000, 02, 01), 60.00m, "");
            account.Withdraw(new Date(2000, 03, 01), 50.00m, "");

            var result = account.EffectiveBalances(new DateRange(new Date(2000, 01, 01), new Date(2000, 03, 01))).ToArray();

            var expectedResult = new CashAccountEffectiveBalance[]
            {
                new CashAccountEffectiveBalance(new Date(2000, 01, 01), new Date(2000, 01, 31), 100.00m),
                new CashAccountEffectiveBalance(new Date(2000, 02, 01), new Date(2000, 02, 29), 360.00m),
                new CashAccountEffectiveBalance(new Date(2000, 03, 01), new Date(2000, 03, 01), 310.00m)
            };

            Assert.That(result, Is.EqualTo(expectedResult).Using(new EffectiveBalanceComparer()));
        }

        [TestCase]
        public void EffectiveBalancesStartAndEndDatesNotMatching()
        {
            var account = new CashAccount();
            account.Deposit(new Date(2000, 01, 01), 100.00m, "");
            account.Deposit(new Date(2000, 02, 01), 200.00m, "");
            account.Deposit(new Date(2000, 02, 01), 60.00m, "");
            account.Withdraw(new Date(2000, 03, 01), 50.00m, "");

            var result = account.EffectiveBalances(new DateRange(new Date(2000, 01, 15), new Date(2000, 02, 15))).ToArray();

            var expectedResult = new CashAccountEffectiveBalance[]
            {
                new CashAccountEffectiveBalance(new Date(2000, 01, 15), new Date(2000, 01, 31), 100.00m),
                new CashAccountEffectiveBalance(new Date(2000, 02, 01), new Date(2000, 02, 15), 360.00m),
            };

            Assert.That(result, Is.EqualTo(expectedResult).Using(new EffectiveBalanceComparer()));
        }

        [TestCase]
        public void Deposit()
        {
            var account = new CashAccount();

            account.Deposit(new Date(2000, 01, 01), 100.00m, "test");

            var result = account.Transactions.ToArray();

            var expectedResult = new CashAccountTransaction[]
            {
                new CashAccountTransaction(Guid.Empty, new Date(2000, 01, 01), "test", 100.00m, BankAccountTransactionType.Deposit, 100.00m)
            };

            Assert.That(result, Is.EqualTo(expectedResult).Using(new CashTransactionComparer()));
        }

        [TestCase]
        public void Withdrawl()
        {
            var account = new CashAccount();

            account.Withdraw(new Date(2000, 01, 01), 100.00m, "test");

            var result = account.Transactions.ToArray();

            var expectedResult = new CashAccountTransaction[]
            {
                new CashAccountTransaction(Guid.Empty, new Date(2000, 01, 01), "test", -100.00m, BankAccountTransactionType.Withdrawl, -100.00m)
            };

            Assert.That(result, Is.EqualTo(expectedResult).Using(new CashTransactionComparer()));
        }

        [TestCase]
        public void Transfer()
        {
            var account = new CashAccount();

            account.Transfer(new Date(2000, 01, 01), 100.00m, "test");

            var result = account.Transactions.ToArray();

            var expectedResult = new CashAccountTransaction[]
            {
                new CashAccountTransaction(Guid.Empty, new Date(2000, 01, 01), "test", 100.00m, BankAccountTransactionType.Transfer, 100.00m)
            };

            Assert.That(result, Is.EqualTo(expectedResult).Using(new CashTransactionComparer()));
        }

        [TestCase]
        public void Fee()
        {
            var account = new CashAccount();

            account.FeeDeducted(new Date(2000, 01, 01), 100.00m, "test");

            var result = account.Transactions.ToArray();

            var expectedResult = new CashAccountTransaction[]
            {
                new CashAccountTransaction(Guid.Empty, new Date(2000, 01, 01), "test", -100.00m, BankAccountTransactionType.Fee, -100.00m)
            };

            Assert.That(result, Is.EqualTo(expectedResult).Using(new CashTransactionComparer()));
        }

        [TestCase]
        public void Interest()
        {
            var account = new CashAccount();

            account.InterestPaid(new Date(2000, 01, 01), 100.00m, "test");

            var result = account.Transactions.ToArray();

            var expectedResult = new CashAccountTransaction[]
            {
                new CashAccountTransaction(Guid.Empty, new Date(2000, 01, 01), "test", 100.00m, BankAccountTransactionType.Interest, 100.00m)
            };

            Assert.That(result, Is.EqualTo(expectedResult).Using(new CashTransactionComparer()));
        }

        [TestCase]
        public void AddTransactionAtStart()
        {
            var account = new CashAccount();
            account.AddTransaction(new Date(2001, 01, 01), 100.00m, "test1", BankAccountTransactionType.Interest);

            account.AddTransaction(new Date(2000, 01, 01), 200.00m, "test2", BankAccountTransactionType.Deposit);

            var result = account.Transactions.ToArray();

            var expectedResult = new CashAccountTransaction[]
            {
                new CashAccountTransaction(Guid.Empty, new Date(2000, 01, 01), "test2", 200.00m, BankAccountTransactionType.Deposit, 200.00m),
                new CashAccountTransaction(Guid.Empty, new Date(2001, 01, 01), "test1", 100.00m, BankAccountTransactionType.Interest, 300.00m)
            };

            Assert.That(result, Is.EqualTo(expectedResult).Using(new CashTransactionComparer()));
        }

        [TestCase]
        public void AddTransactionOnSameDayAsExistingOn()
        {
            var account = new CashAccount();
            account.AddTransaction(new Date(2001, 01, 01), 100.00m, "test1", BankAccountTransactionType.Interest);

            account.AddTransaction(new Date(2001, 01, 01), 200.00m, "test2", BankAccountTransactionType.Deposit);

            var result = account.Transactions.ToArray();

            var expectedResult = new CashAccountTransaction[]
            {
                new CashAccountTransaction(Guid.Empty, new Date(2001, 01, 01), "test1", 100.00m, BankAccountTransactionType.Interest, 100.00m),
                new CashAccountTransaction(Guid.Empty, new Date(2001, 01, 01), "test2", 200.00m, BankAccountTransactionType.Deposit, 300.00m)
            };

            Assert.That(result, Is.EqualTo(expectedResult).Using(new CashTransactionComparer()));
        }

        [TestCase]
        public void AddTransactionInMiddle()
        {
            var account = new CashAccount();
            account.AddTransaction(new Date(2000, 01, 01), 100.00m, "test1", BankAccountTransactionType.Interest);
            account.AddTransaction(new Date(2002, 01, 01), 200.00m, "test2", BankAccountTransactionType.Deposit);

            account.AddTransaction(new Date(2001, 01, 01), 50.00m, "test3", BankAccountTransactionType.Withdrawl);

            var result = account.Transactions.ToArray();

            var expectedResult = new CashAccountTransaction[]
            {
                new CashAccountTransaction(Guid.Empty, new Date(2000, 01, 01), "test1", 100.00m, BankAccountTransactionType.Interest, 100.00m),
                new CashAccountTransaction(Guid.Empty, new Date(2001, 01, 01), "test3", -50.00m, BankAccountTransactionType.Deposit, 50.00m),
                new CashAccountTransaction(Guid.Empty, new Date(2002, 01, 01), "test2", 200.00m, BankAccountTransactionType.Deposit, 250.00m)
            };

            Assert.That(result, Is.EqualTo(expectedResult).Using(new CashTransactionComparer()));
        }

        [TestCase]
        public void AddTransactionAtTheEnd()
        {
            var account = new CashAccount();
            account.AddTransaction(new Date(2000, 01, 01), 100.00m, "test1", BankAccountTransactionType.Interest);
            account.AddTransaction(new Date(2001, 01, 01), 200.00m, "test2", BankAccountTransactionType.Deposit);

            account.AddTransaction(new Date(2002, 01, 01), 50.00m, "test3", BankAccountTransactionType.Withdrawl);

            var result = account.Transactions.ToArray();

            var expectedResult = new CashAccountTransaction[]
            {
                new CashAccountTransaction(Guid.Empty, new Date(2000, 01, 01), "test1", 100.00m, BankAccountTransactionType.Interest, 100.00m),
                new CashAccountTransaction(Guid.Empty, new Date(2001, 01, 01), "test2", 200.00m, BankAccountTransactionType.Deposit, 300.00m),
                new CashAccountTransaction(Guid.Empty, new Date(2002, 01, 01), "test3", -50.00m, BankAccountTransactionType.Deposit, 250.00m)
            };

            Assert.That(result, Is.EqualTo(expectedResult).Using(new CashTransactionComparer()));
        }


        private class EffectiveBalanceComparer : IEqualityComparer<CashAccountEffectiveBalance>
        {
            public bool Equals(CashAccountEffectiveBalance x, CashAccountEffectiveBalance y)
            {
                return (x.EffectivePeriod == y.EffectivePeriod) && (x.Balance == y.Balance);
            }

            public int GetHashCode(CashAccountEffectiveBalance obj)
            {
                throw new NotImplementedException();
            }
        }

        private class CashTransactionComparer : IEqualityComparer<CashAccountTransaction>
        {
            public bool Equals(CashAccountTransaction x, CashAccountTransaction y)
            {
                return (x.Date == y.Date) && (x.Description == y.Description) && (x.Amount == y.Amount) && (x.Balance == y.Balance);
            }

            public int GetHashCode(CashAccountTransaction obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}
