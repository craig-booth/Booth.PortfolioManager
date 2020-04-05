using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Portfolios
{
    public interface IReadOnlyCashAccount
    {
        ITransactionList<CashAccount.Transaction> Transactions { get; }

        decimal Balance();
        decimal Balance(Date date);

        IEnumerable<CashAccount.EffectiveBalance> EffectiveBalances(DateRange dateRange);
    }

    public interface ICashAccount :  IReadOnlyCashAccount
    {
        void Deposit(Date date, decimal amount, string description);
        void Withdraw(Date date, decimal amount, string description);
        void Transfer(Date date, decimal amount, string description);
        void FeeDeducted(Date date, decimal amount, string description);
        void InterestPaid(Date date, decimal amount, string description);
        void AddTransaction(Date date, decimal amount, string description, BankAccountTransactionType type);
    }

    public class CashAccount : ICashAccount, IReadOnlyCashAccount
{
        private CashTransactionList _Transactions;
        public ITransactionList<Transaction> Transactions
        {
            get { return _Transactions; }
        }

        public CashAccount()
        {
            _Transactions = new CashTransactionList(this);
        }

        public decimal Balance()
        {
            if (_Transactions.Count > 0)
                return _Transactions[_Transactions.Count - 1].Balance;
            else
                return 0.00m;
        }

        public decimal Balance(Date date)
        {
            {
                if ((_Transactions.Count == 0) || (date < _Transactions.Earliest))
                    return 0.00m;

                var index = _Transactions.IndexOf(date, TransationListPosition.Last);
                if (index < 0)
                    index = ~index - 1;

                return _Transactions[index].Balance;
            }

        }

        public IEnumerable<EffectiveBalance> EffectiveBalances(DateRange dateRange)
        {
            var fromDate = dateRange.FromDate;
            var toDate = Date.MaxValue;
            var balance = Balance(fromDate);

            foreach (var transaction in _Transactions.InDateRange(dateRange))
            {
                if (fromDate == transaction.Date)
                {
                    balance = transaction.Balance;
                }
                else
                {
                    toDate = transaction.Date.AddDays(-1);

                    yield return new EffectiveBalance(fromDate, toDate, balance);

                    fromDate = transaction.Date;
                    toDate = transaction.Date;
                    balance = transaction.Balance;
                }
            }

            yield return new EffectiveBalance(fromDate, dateRange.ToDate, balance);
        } 

        public void Deposit(Date date, decimal amount, string description)
        {
            AddTransaction(date, amount, description, BankAccountTransactionType.Deposit);
        }

        public void Withdraw(Date date, decimal amount, string description)
        {
            AddTransaction(date, -amount, description, BankAccountTransactionType.Deposit);
        }

        public void Transfer(Date date, decimal amount, string description)
        {
            AddTransaction(date, amount, description, BankAccountTransactionType.Transfer);
        }

        public void FeeDeducted(Date date, decimal amount, string description)
        {
            AddTransaction(date, -amount, description, BankAccountTransactionType.Fee);
        }

        public void InterestPaid(Date date, decimal amount, string description)
        {
            AddTransaction(date, amount, description, BankAccountTransactionType.Interest);
        } 
        
        public void AddTransaction(Date date, decimal amount, string description, BankAccountTransactionType type)
        {
            if (((type == BankAccountTransactionType.Withdrawl) || (type == BankAccountTransactionType.Fee)) && (amount > 0.00m))
                _Transactions.Add(date, -amount, description, type);
            else
                _Transactions.Add(date, amount, description, type);
        }

        private class CashTransactionList
            : TransactionList<Transaction>
        {
            private CashAccount _CashAccount;
            public CashTransactionList(CashAccount cashAccount)
            {
                _CashAccount = cashAccount;
            }

            public void Add(Date date, decimal amount, string description, BankAccountTransactionType type)
            {
                if ((Count == 0) || (date >= Latest))
                {
                    var transaction = new Transaction(Guid.NewGuid(), date, description, amount, type, _CashAccount.Balance() + amount);
                    Add(transaction);
                }
                else
                {
                    var transaction = new Transaction(Guid.NewGuid(), date, description, amount, type, _CashAccount.Balance(date) + amount);
                    Add(transaction);

                    // Update balance on subsequent transactions
                    var index = IndexOf(date, TransationListPosition.Last);
                    for (var i = index + 1; i < Count; i++)
                        this[i].Balance += amount;
                }
            }
        }

        public class Transaction : ITransaction
        {
            public Guid Id { get; }
            public Date Date { get; }
            public string Description { get; }
            public decimal Amount { get; }
            public BankAccountTransactionType Type { get; }
            public decimal Balance { get; internal set; }

            public Transaction(Guid id, Date date, string description, decimal amount, BankAccountTransactionType type, decimal balance)
            {
                Id = id;
                Date = date;
                Description = description;
                Amount = amount;
                Type = type;
                Balance = balance;
            }
        }


        public class EffectiveBalance
        {
            public DateRange EffectivePeriod { get; set; }
            public decimal Balance { get; set; }

            public EffectiveBalance(Date fromDate, Date toDate, decimal balance)
            {
                EffectivePeriod = new DateRange(fromDate, toDate);
                Balance = balance;
            }
        }
    }
}