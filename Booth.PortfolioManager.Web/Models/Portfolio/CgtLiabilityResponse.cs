using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Models.Portfolio
{
    public class CgtLiabilityResponse
    {
        public decimal CurrentYearCapitalGainsOther { get; set; }
        public decimal CurrentYearCapitalGainsDiscounted { get; set; }
        public decimal CurrentYearCapitalGainsTotal { get; set; }
        public decimal CurrentYearCapitalLossesOther { get; set; }
        public decimal CurrentYearCapitalLossesDiscounted { get; set; }
        public decimal CurrentYearCapitalLossesTotal { get; set; }
        public decimal GrossCapitalGainOther { get; set; }
        public decimal GrossCapitalGainDiscounted { get; set; }
        public decimal GrossCapitalGainTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal NetCapitalGainOther { get; set; }
        public decimal NetCapitalGainDiscounted { get; set; }
        public decimal NetCapitalGainTotal { get; set; }

        public List<CgtLiabilityEvent> Events { get; set; } = new List<CgtLiabilityEvent>();

        public class CgtLiabilityEvent
        {
            public Stock Stock { get; set; }
            public Date EventDate { get; set; }
            public decimal CostBase { get; set; }
            public decimal AmountReceived { get; set; }
            public decimal CapitalGain { get; set; }
            public CgtMethod Method { get; set; }
        }
    }

}
