using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Models.Stock
{
    public class CreateStockCommand
    {
        public Guid Id { get; set; }
        public Date ListingDate { get; set; }
        public string AsxCode { get; set; }
        public string Name { get; set; }
        public bool Trust { get; set; }
        public AssetCategory Category { get; set; }

    }

}
