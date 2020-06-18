using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.Stocks.Events
{
    public class ChangeDividendRulesEvent : Event
    {
        public Date ChangeDate { get; set; }
        public decimal CompanyTaxRate { get; set; }
        public RoundingRule DividendRoundingRule { get; set; }
        public bool DrpActive { get; set; }
        public DrpMethod DrpMethod { get; set; }

        public ChangeDividendRulesEvent(Guid entityId, int version, Date changeDate, decimal companyTaxRate, RoundingRule dividendRoundingRule, bool drpActive, DrpMethod drpMethod)
            :base(entityId, version)
        {
            ChangeDate = changeDate;
            CompanyTaxRate = companyTaxRate;
            DividendRoundingRule = dividendRoundingRule;
            DrpActive = drpActive;
            DrpMethod = drpMethod;
        }
    }
}
