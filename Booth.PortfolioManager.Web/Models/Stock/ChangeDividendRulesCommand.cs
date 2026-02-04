using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Models.Stock
{
    public class ChangeDividendRulesCommand
    {
        public Guid Id { get; set; }
        public Date ChangeDate { get; set; }
        public decimal CompanyTaxRate { get; set; }
        public RoundingRule DividendRoundingRule { get; set; }
        public bool DrpActive { get; set; }
        public DrpMethod DrpMethod { get; set; } 
    }
}
