using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Models.Transaction
{
    public class IncomeReceived : Transaction
    {
        public override TransactionType Type => TransactionType.IncomeReceived;
        public Date RecordDate { get; set; }
        public decimal FrankedAmount { get; set; }
        public decimal UnfrankedAmount { get; set; }
        public decimal FrankingCredits { get; set; }
        public decimal Interest { get; set; }
        public decimal TaxDeferred { get; set; }
        public bool CreateCashTransaction { get; set; }
        public decimal DrpCashBalance { get; set; }
    } 
}
 