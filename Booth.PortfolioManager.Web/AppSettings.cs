using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

using Booth.PortfolioManager.Web.Authentication;

namespace Booth.PortfolioManager.Web
{
    class AppSettings
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
        public bool AllowDebugUserAcccess { get; set; }

        public JwtTokenConfiguration JwtTokenConfiguration { get; set; }
    }

    class JwtTokenConfiguration
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Key { get; set; }
        public string KeyFile { get; set; }
    }
}
