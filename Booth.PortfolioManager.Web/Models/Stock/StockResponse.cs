using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Models.Stock
{
    public enum DrpMethod { Round, RoundDown, RoundUp, RetainCashBalance }
    public class StockResponse
    {
        public Guid Id { get; set; }
        public string AsxCode { get; set; }
        public string Name { get; set; }

        public AssetCategory Category { get; set; }
        public bool Trust { get; set; }

        public Date ListingDate { get; set; }
        public Date DelistedDate { get; set; }
        public decimal LastPrice { get; set; }
        public decimal CompanyTaxRate { get; set; }
        public RoundingRule DividendRoundingRule { get; set; }
        public bool DrpActive { get; set; }
        public DrpMethod DrpMethod { get; set; }
    }
}
