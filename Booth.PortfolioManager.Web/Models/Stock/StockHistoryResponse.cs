using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Models.Stock
{
    public class StockHistoryResponse 
    {
        public Guid Id { get; set; }
        public string AsxCode { get; set; }
        public string Name { get; set; }
        public Date ListingDate { get; set; }
        public Date DelistedDate { get; set; }
        public List<HistoricProperties> History { get; set; } = new List<HistoricProperties>();
        public List<HistoricDividendRules> DividendRules { get; set; } = new List<HistoricDividendRules>();

        public void AddHistory(Date fromDate, Date toDate, string asxCode, string name, AssetCategory category)
        {
            var history = new HistoricProperties()
            {
                FromDate = fromDate,
                ToDate = toDate,
                AsxCode = asxCode,
                Name = name,
                Category = category
            };
            History.Add(history);
        }

        public void AddDividendRules(Date fromDate, Date toDate, decimal companyTaxRate, RoundingRule roundingRule, bool drpActive, DrpMethod drpMethod)
        {
            var history = new HistoricDividendRules()
            {
                FromDate = fromDate,
                ToDate = toDate,
                CompanyTaxRate = companyTaxRate,
                RoundingRule = roundingRule,
                DrpActive = drpActive,
                DrpMethod = drpMethod
            };
            DividendRules.Add(history);
        }

        public class HistoricProperties
        {
            public Date FromDate { get; set; }
            public Date ToDate { get; set; }
            public string AsxCode { get; set; }
            public string Name { get; set; }
            public AssetCategory Category { get; set; }
        }

        public class HistoricDividendRules
        {
            public Date FromDate { get; set; }
            public Date ToDate { get; set; }
            public decimal CompanyTaxRate { get; set; }
            public RoundingRule RoundingRule { get; set; }
            public bool DrpActive { get; set; }
            public DrpMethod DrpMethod { get; set; }
        }

    }
}
