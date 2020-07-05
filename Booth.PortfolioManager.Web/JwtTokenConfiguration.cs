using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Booth.PortfolioManager.Web
{

    public interface IJwtTokenConfiguration
    {
        string Issuer { get; }
        string Audience { get; }
        public SymmetricSecurityKey GetKey();
    }

    class JwtTokenConfiguration : IJwtTokenConfiguration
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string KeyFile { get; set; }

        private SymmetricSecurityKey _Key = null;
        public SymmetricSecurityKey GetKey()
        {
            if (_Key == null)
            {
                var base64Key = System.IO.File.ReadAllText(KeyFile);
                var key = Convert.FromBase64String(base64Key);
                _Key = new SymmetricSecurityKey(key);
            }

            return _Key;
        }
    }
}
