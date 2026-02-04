using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Models.Stock
{
    public class ChangeStockCommand
    {
        public Guid Id { get; set; }
        public Date ChangeDate { get; set; }
        public string AsxCode { get; set; }
        public string Name { get; set; }
        public AssetCategory Category { get; set; }
    }
}
