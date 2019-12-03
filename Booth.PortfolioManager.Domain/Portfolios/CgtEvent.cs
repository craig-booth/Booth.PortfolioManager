using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

using Booth.PortfolioManager.Domain.Utils;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Portfolios
{
    public enum CGTMethod { Other, Discount, Indexation }

    public class CgtEvent : ITransaction
    {
        public Guid Id { get; set; }
        public Date Date { get; set; }
        public Stock Stock { get; set; }
        public int Units { get; set; }
       
        public decimal CostBase { get; set; }
        public decimal AmountReceived { get; set; }
        public decimal CapitalGain { get; set; }
        public CGTMethod CgtMethod { get; set; }
    }
}
