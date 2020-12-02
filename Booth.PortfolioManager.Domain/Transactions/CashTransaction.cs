using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public enum BankAccountTransactionType { Deposit, Withdrawl, Transfer, Fee, Interest }

    public class CashTransaction : PortfolioTransaction
    {
        public BankAccountTransactionType CashTransactionType { get; set; }
        public decimal Amount { get; set; }
        public override string Description
        {
            get
            {
                switch (CashTransactionType)
                {
                    case BankAccountTransactionType.Deposit:
                        return "Deposit " + MathUtils.FormatCurrency(Amount, true, true);
                    case BankAccountTransactionType.Fee:
                        return "Fee " + MathUtils.FormatCurrency(Amount, true, true);
                    case BankAccountTransactionType.Interest:
                        return "Interest " + MathUtils.FormatCurrency(Amount, true, true);
                    case BankAccountTransactionType.Transfer:
                        return "Transfer " + MathUtils.FormatCurrency(Amount, true, true);
                    case BankAccountTransactionType.Withdrawl:
                        return "Withdrawl " + MathUtils.FormatCurrency(Amount, true, true);
                }

                return "";
            }
        }
    }
}
