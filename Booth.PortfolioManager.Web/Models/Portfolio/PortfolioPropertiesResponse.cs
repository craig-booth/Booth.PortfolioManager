using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Models.Portfolio
{
    public class PortfolioPropertiesResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Date StartDate { get; set; }
        public Date EndDate { get; set; }

        public List<HoldingProperties> Holdings { get; set; } = new List<HoldingProperties>();
    }

    public class HoldingProperties
    {
        public Stock Stock { get; set; }
        public Date StartDate { get; set; }
        public Date EndDate { get; set; }
        public bool ParticipatingInDrp { get; set; }
    }
}
