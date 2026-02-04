using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Models.Portfolio
{
    public class SimpleUnrealisedGainsResponse
    {
        public List<SimpleUnrealisedGainsItem> UnrealisedGains { get; } = new List<SimpleUnrealisedGainsItem>();
    }

    public class SimpleUnrealisedGainsItem
    {
        public Stock Stock { get; set; }

        public Date AquisitionDate { get; set; }
        public int Units { get; set; }
        public decimal CostBase { get; set; }
        public decimal MarketValue { get; set; }
        public decimal CapitalGain { get; set; }
        public decimal DiscoutedGain { get; set; }
        public CgtMethod DiscountMethod { get; set; }
    }
}
