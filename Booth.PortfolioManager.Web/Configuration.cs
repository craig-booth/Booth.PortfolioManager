using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;


namespace Booth.PortfolioManager.Web
{
    class Configuration
    {
        public int Port { get; set; }
        public string EventStore { get; set; }
        public string Database { get; set; }
        public bool EnableAuthentication { get; set; }

        public JwtTokenConfiguration JwtTokenConfiguration { get; set; }
    }
}
