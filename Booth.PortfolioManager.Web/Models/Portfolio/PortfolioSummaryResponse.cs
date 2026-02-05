using System;
using System.Collections.Generic;
using System.Text;

namespace Booth.PortfolioManager.Web.Models.Portfolio
{
    public class PortfolioSummaryResponse
    {
        public decimal PortfolioValue { get; set; }
        public decimal PortfolioCost { get; set; }
        public decimal? Return1Year { get; set; }
        public decimal? Return3Year { get; set; }
        public decimal? Return5Year { get; set; }
        public decimal? ReturnAll { get; set; }
        public decimal CashBalance { get; set; }
        public List<Holding> Holdings { get; set; } = new List<Holding>();
    }
}
