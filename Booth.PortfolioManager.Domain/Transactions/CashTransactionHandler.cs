using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Transactions
{
    class CashTransactionHandler : ITransactionHandler
    {
        public bool CanCreateHolding => false;

        public void Apply(IPortfolioTransaction transaction, IHolding holding, ICashAccount cashAccount)
        {
            var cashTransaction = transaction as CashTransaction;
            if (cashTransaction == null)
                throw new ArgumentException("Expected transaction to be a CashTransaction");

            var description = "";
            if (cashTransaction.Comment != "")
                description = cashTransaction.Comment;
            else if (cashTransaction.CashTransactionType == BankAccountTransactionType.Deposit)
                description = "Deposit";
            else if (cashTransaction.CashTransactionType == BankAccountTransactionType.Fee)
                description = "Fee";
            else if (cashTransaction.CashTransactionType == BankAccountTransactionType.Interest)
                description = "Interest";
            else if (cashTransaction.CashTransactionType == BankAccountTransactionType.Transfer)
                description = "Transfer";
            else if (cashTransaction.CashTransactionType == BankAccountTransactionType.Withdrawl)
                description = "Withdrawl";

            cashAccount.AddTransaction(cashTransaction.Date, cashTransaction.Amount, description, cashTransaction.CashTransactionType);
        }
    }
}
