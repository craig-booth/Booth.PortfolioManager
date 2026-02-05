using System;
using System.Collections.Generic;
using System.Text;

namespace Booth.PortfolioManager.Web.Models.Portfolio
{
    public class PortfolioPerformanceResponse
    {
        public decimal OpeningBalance { get; set; }
        public decimal Dividends { get; set; }
        public decimal ChangeInMarketValue { get; set; }
        public decimal OutstandingDRPAmount { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal OpeningCashBalance { get; set; }
        public decimal Deposits { get; set; }
        public decimal Withdrawls { get; set; }
        public decimal Interest { get; set; }
        public decimal Fees { get; set; }
        public decimal ClosingCashBalance { get; set; }
        public List<HoldingPerformanceItem> HoldingPerformance { get; set; } = new List<HoldingPerformanceItem>();

        public class HoldingPerformanceItem
        {
            public Stock Stock { get; set; }
            public decimal OpeningBalance { get; set; }
            public decimal Purchases { get; set; }
            public decimal Sales { get; set; }
            public decimal ClosingBalance { get; set; }
            public decimal Dividends { get; set; }
            public decimal CapitalGain { get; set; }
            public decimal DrpCashBalance { get; set; }
            public decimal TotalReturn { get; set; }
            public decimal Irr { get; set; }
        }
    }
}
