using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Models.Transaction
{
    public class ReturnOfCapital : Transaction
    {
        public override TransactionType Type => TransactionType.ReturnOfCapital;
        public Date RecordDate { get; set; }
        public decimal Amount { get; set; }
        public bool CreateCashTransaction { get; set; }
    } 
}
