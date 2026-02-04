using System;
using System.Collections.Generic;
using System.Text;

namespace Booth.PortfolioManager.Web.Models.Portfolio
{
    public class Holding
    {
        public Stock Stock { get; set; }
        public int Units { get; set; }
        public decimal Value { get; set; }
        public decimal Cost { get; set; }
        public decimal CostBase { get; set; }
    }
}
