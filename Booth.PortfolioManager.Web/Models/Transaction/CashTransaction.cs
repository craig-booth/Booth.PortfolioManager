using System;
using System.Collections.Generic;
using System.Text;


namespace Booth.PortfolioManager.Web.Models.Transaction
{
    public enum CashTransactionType { Deposit, Withdrawl, Transfer, Fee, Interest }
    public class CashTransaction : Transaction
    {
        public override TransactionType Type => TransactionType.CashTransaction;
        public CashTransactionType CashTransactionType { get; set; }
        public decimal Amount { get; set; }
    } 
}
