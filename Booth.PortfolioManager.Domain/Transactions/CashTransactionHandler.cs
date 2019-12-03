using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public class CashTransactionHandler : ITransactionHandler
    {
        private CashAccount _CashAccount;

        public CashTransactionHandler(CashAccount cashAccount)
        {
            _CashAccount = cashAccount;
        }

        public void ApplyTransaction(Transaction transaction)
        {
            var cashTransaction = transaction as CashTransaction;

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

            _CashAccount.AddTransaction(cashTransaction.Date, cashTransaction.Amount, description, cashTransaction.CashTransactionType);
        }
    }
}
