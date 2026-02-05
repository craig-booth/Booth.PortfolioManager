using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using Booth.Common;
using Booth.PortfolioManager.Web.Models.Transaction;

namespace Booth.PortfolioManager.Web.Models.Portfolio
{
    public class CashAccountTransactionsResponse
    {
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }

        public List<Transaction> Transactions { get; set; } = new List<Transaction>();

        public void AddTransaction(Date date, CashTransactionType type, string description, decimal amount, decimal balance)
        {
            var transaction = new Transaction()
            {
                Date = date,
                Type = type,
                Description = description,
                Amount = amount,
                Balance = balance
            };
            Transactions.Add(transaction);
        }

        public class Transaction
        {
            public Date Date { get; set; }
            public CashTransactionType Type { get; set; }
            public string Description { get; set; }
            public decimal Amount { get; set; }
            public decimal Balance { get; set; }
        }
    }
}
