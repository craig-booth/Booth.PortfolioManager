using System;
using System.Collections.Generic;
using System.Text;

namespace Booth.PortfolioManager.Web.Models.Portfolio
{
    public class CreatePortfolioCommand
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
