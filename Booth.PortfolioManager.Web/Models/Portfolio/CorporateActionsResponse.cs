using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Models.Portfolio
{
    public class CorporateActionsResponse
    {
        public List<CorporateActionItem> CorporateActions { get; } = new List<CorporateActionItem>();

        public class CorporateActionItem
        {
            public Guid Id { get; set; }
            public Date ActionDate { get; set; }
            public Stock Stock { get; set; }
            public string Description { get; set; }
        }
    }

}
