﻿using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Transactions
{
    public enum BankAccountTransactionType { Deposit, Withdrawl, Transfer, Fee, Interest }

    public class CashTransaction : IPortfolioTransaction
    {
        public Guid Id { get; set; }
        public Date Date { get; set; }
        public IReadOnlyStock Stock { get; set; }
        public string Comment { get; set; }
        public BankAccountTransactionType CashTransactionType { get; set; }
        public decimal Amount { get; set; }
        public string Description
        {
            get
            {
                switch (CashTransactionType)
                {
                    case BankAccountTransactionType.Deposit:
                        return String.Format("Deposit {0:c}", Amount);
                    case BankAccountTransactionType.Fee:
                        return String.Format("Fee {0:c}", Amount);
                    case BankAccountTransactionType.Interest:
                        return String.Format("Interest {0:c}", Amount);
                    case BankAccountTransactionType.Transfer:
                        return String.Format("Transfer {0:c}", Amount);
                    case BankAccountTransactionType.Withdrawl:
                        return String.Format("Withdrawl {0:c}", Amount);
                }

                return "";
            }
        }
    }
}
