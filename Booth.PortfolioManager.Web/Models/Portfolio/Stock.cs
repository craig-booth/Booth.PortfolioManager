using System;
using System.Collections.Generic;
using System.Text;

using Booth.PortfolioManager.Web.Models.Stock;

namespace Booth.PortfolioManager.Web.Models.Portfolio
{
    public class Stock
    {
        public Guid Id { get; set; }
        public string AsxCode { get; set; }
        public string Name { get; set; }
        public AssetCategory Category { get; set; }
    }
}
