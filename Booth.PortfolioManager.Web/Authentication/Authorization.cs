using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Booth.PortfolioManager.Web.Authentication
{
    static class Role
    {
        public const string Administrator = "Administrator";
    }

    static class Policy
    {
        public const string CanMantainStocks = "CanMaintainStocks";
        public const string IsPortfolioOwner = "CanAccessPortfolio";
        public const string CanCreatePortfolio = "CanCreatePortfolio";
    }
}
