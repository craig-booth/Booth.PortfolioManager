using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;


namespace Booth.PortfolioManager.Domain.Test.Portfolios
{
    public class CashAccountTests
    { 
        [Fact]
        public void BalanceWithNoTransactions()
        {
            var account = new CashAccount();

            account.Balance().Should().Be(0);
        }

        [Fact]
        public void Balance()
        {
            var account = new CashAccount();
            account.Deposit(new Date(2000, 01, 01), 100.00m, "");
            account.Deposit(new Date(2000, 02, 01), 200.00m, "");
            account.Withdraw(new Date(2000, 03, 01), 50.00m, "");

            account.Balance().Should().Be(250.00m);
        }

        [Fact]
        public void BalanceAtSpecificDateWithNoTransactions()
        {
            var account = new CashAccount();
            account.Deposit(new Date(2000, 01, 01), 100.00m, "");
            account.Deposit(new Date(2000, 02, 01), 200.00m, "");
            account.Withdraw(new Date(2000, 03, 01), 50.00m, "");

            account.Balance(new Date(2000, 02, 15)).Should().Be(300.00m);
        }

        [Fact]
        public void BalanceBeforeFirstTransaction()
        {
            var account = new CashAccount();
            account.Deposit(new Date(2000, 01, 01), 100.00m, "");
            account.Deposit(new Date(2000, 02, 01), 200.00m, "");
            account.Withdraw(new Date(2000, 03, 01), 50.00m, "");

            account.Balance(new Date(1999, 01, 01)).Should().Be(0.00m);
        }

        [Fact]
        public void BalanceAfterLastTransaction()
        {
            var account = new CashAccount();
            account.Deposit(new Date(2000, 01, 01), 100.00m, "");
            account.Deposit(new Date(2000, 02, 01), 200.00m, "");
            account.Withdraw(new Date(2000, 03, 01), 50.00m, "");

            account.Balance(new Date(2002, 01, 01)).Should().Be(250.00m);
        }
        [Fact]
        public void BalanceMultipleTransactionsOnThatDate()
        {
            var account = new CashAccount();
            account.Deposit(new Date(2000, 01, 01), 100.00m, "");
            account.Deposit(new Date(2000, 02, 01), 200.00m, "");
            account.Deposit(new Date(2000, 02, 01), 60.00m, "");
            account.Withdraw(new Date(2000, 03, 01), 50.00m, "");

            account.Balance(new Date(2000, 02, 01)).Should().Be(360.00m);
        }

        [Fact]
        public void BalanceNoTransactionsOnThatDate()
        {
            var account = new CashAccount();
            account.Deposit(new Date(2000, 01, 01), 100.00m, "");
            account.Deposit(new Date(2000, 02, 01), 200.00m, "");
            account.Deposit(new Date(2000, 02, 01), 60.00m, "");
            account.Withdraw(new Date(2000, 03, 01), 50.00m, "");

            account.Balance(new Date(2000, 02, 15)).Should().Be(360.00m);
        }

        [Fact]
        public void EffectiveBalancesNoTransactions()
        {
            var account = new CashAccount();

            var result = account.EffectiveBalances(new DateRange(new Date(2000, 01, 01), new Date(2010, 01, 01))).ToArray();

            var expectedResult = new CashAccountEffectiveBalance[]
            {
                new CashAccountEffectiveBalance(new Date(2000, 01, 01), new Date(2010, 01, 01), 0.00m)
            };

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
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

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
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

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void Deposit()
        {
            var account = new CashAccount();

            account.Deposit(new Date(2000, 01, 01), 100.00m, "test");

            var result = account.Transactions.ToArray();

            result.Should().BeEquivalentTo(new[]
            {
                new { Date = new Date(2000, 01, 01), Description = "test", Amount = 100.00m, Type = BankAccountTransactionType.Deposit, Balance = 100.00m }
            });
        }

        [Fact]
        public void Withdrawl()
        {
            var account = new CashAccount();

            account.Withdraw(new Date(2000, 01, 01), 100.00m, "test");

            var result = account.Transactions.ToArray();

            result.Should().BeEquivalentTo(new[]
            {
                new { Date = new Date(2000, 01, 01), Description = "test", Amount = -100.00m, Type = BankAccountTransactionType.Withdrawl, Balance = -100.00m }
            });
        }

        [Fact]
        public void Transfer()
        {
            var account = new CashAccount();

            account.Transfer(new Date(2000, 01, 01), 100.00m, "test");

            var result = account.Transactions.ToArray();

            result.Should().BeEquivalentTo(new[]
            {
                new { Date = new Date(2000, 01, 01), Description = "test", Amount = 100.00m, Type = BankAccountTransactionType.Transfer, Balance = 100.00m }
            });
        }

        [Fact]
        public void Fee()
        {
            var account = new CashAccount();

            account.FeeDeducted(new Date(2000, 01, 01), 100.00m, "test");

            var result = account.Transactions.ToArray();

            result.Should().BeEquivalentTo(new[]
            {
                new { Date = new Date(2000, 01, 01), Description = "test", Amount = -100.00m, Type = BankAccountTransactionType.Fee, Balance = -100.00m }
            });
        }

        [Fact]
        public void Interest()
        {
            var account = new CashAccount();

            account.InterestPaid(new Date(2000, 01, 01), 100.00m, "test");

            var result = account.Transactions.ToArray();

            result.Should().BeEquivalentTo(new[]
            {
                new { Date = new Date(2000, 01, 01), Description = "test", Amount = 100.00m, Type = BankAccountTransactionType.Interest, Balance = 100.00m }
            });
        }

        [Fact]
        public void AddTransactionAtStart()
        {
            var account = new CashAccount();
            account.AddTransaction(new Date(2001, 01, 01), 100.00m, "test1", BankAccountTransactionType.Interest);

            account.AddTransaction(new Date(2000, 01, 01), 200.00m, "test2", BankAccountTransactionType.Deposit);

            var result = account.Transactions.ToArray();

            result.Should().BeEquivalentTo(new[]
            {
                new { Date = new Date(2000, 01, 01), Description = "test2", Amount = 200.00m, Type = BankAccountTransactionType.Deposit, Balance = 200.00m },
                new { Date = new Date(2001, 01, 01), Description = "test1", Amount = 100.00m, Type = BankAccountTransactionType.Interest, Balance = 300.00m },
            });
        }

        [Fact]
        public void AddTransactionOnSameDayAsExistingOn()
        {
            var account = new CashAccount();
            account.AddTransaction(new Date(2001, 01, 01), 100.00m, "test1", BankAccountTransactionType.Interest);

            account.AddTransaction(new Date(2001, 01, 01), 200.00m, "test2", BankAccountTransactionType.Deposit);

            var result = account.Transactions.ToArray();

            result.Should().BeEquivalentTo(new[]
            {
                new { Date = new Date(2001, 01, 01), Description = "test1", Amount = 100.00m, Type = BankAccountTransactionType.Interest, Balance = 100.00m },
                new { Date = new Date(2001, 01, 01), Description = "test2", Amount = 200.00m, Type = BankAccountTransactionType.Deposit, Balance = 300.00m },
            });
        }

        [Fact]
        public void AddTransactionInMiddle()
        {
            var account = new CashAccount();
            account.AddTransaction(new Date(2000, 01, 01), 100.00m, "test1", BankAccountTransactionType.Interest);
            account.AddTransaction(new Date(2002, 01, 01), 200.00m, "test2", BankAccountTransactionType.Deposit);

            account.AddTransaction(new Date(2001, 01, 01), 50.00m, "test3", BankAccountTransactionType.Withdrawl);

            var result = account.Transactions.ToArray();

            result.Should().BeEquivalentTo(new[]
            {
                new { Date = new Date(2000, 01, 01), Description = "test1", Amount = 100.00m, Type = BankAccountTransactionType.Interest, Balance = 100.00m },
                new { Date = new Date(2001, 01, 01), Description = "test3", Amount = -50.00m, Type = BankAccountTransactionType.Withdrawl, Balance = 50.00m },
                new { Date = new Date(2002, 01, 01), Description = "test2", Amount = 200.00m, Type = BankAccountTransactionType.Deposit, Balance = 250.00m }
            });
        }

        [Fact]
        public void AddTransactionAtTheEnd()
        {
            var account = new CashAccount();
            account.AddTransaction(new Date(2000, 01, 01), 100.00m, "test1", BankAccountTransactionType.Interest);
            account.AddTransaction(new Date(2001, 01, 01), 200.00m, "test2", BankAccountTransactionType.Deposit);

            account.AddTransaction(new Date(2002, 01, 01), 50.00m, "test3", BankAccountTransactionType.Withdrawl);

            var result = account.Transactions.ToArray();

            result.Should().BeEquivalentTo(new[]
            {
                new { Date = new Date(2000, 01, 01), Description = "test1", Amount = 100.00m, Type = BankAccountTransactionType.Interest, Balance = 100.00m },
                new { Date = new Date(2001, 01, 01), Description = "test2", Amount = 200.00m, Type = BankAccountTransactionType.Deposit, Balance = 300.00m },
                new { Date = new Date(2002, 01, 01), Description = "test3", Amount = -50.00m, Type = BankAccountTransactionType.Withdrawl, Balance = 250.00m }
            });
        }

    }
}
