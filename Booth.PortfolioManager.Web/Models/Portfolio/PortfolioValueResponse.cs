using System;
using System.Collections.Generic;
using System.Text;

using Booth.PortfolioManager.Web.Models.Stock;

namespace Booth.PortfolioManager.Web.Models.Portfolio
{
    public enum ValueFrequency { Day, Week, Month };

    public class PortfolioValueResponse
    {
        public List<ClosingPrice> Values { get; } = new List<ClosingPrice>();   
    }
}
