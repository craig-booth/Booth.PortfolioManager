using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Booth.PortfolioManager.Web.Authentication
{

    public interface IJwtTokenConfigurationProvider
    {
        string Issuer { get; }
        string Audience { get; }
        SymmetricSecurityKey Key { get; }
    }

    class JwtTokenConfigurationProvider : IJwtTokenConfigurationProvider
    {
        public string Issuer { get; }
        public string Audience { get; }
        public SymmetricSecurityKey Key  { get; }

        public JwtTokenConfigurationProvider(string issuer, string audience, SymmetricSecurityKey key) 
        {
            Issuer = issuer;
            Audience = audience;
            Key = key;
        }

        public JwtTokenConfigurationProvider(string issuer, string audience, string keyFile)
        {
            Issuer = issuer;
            Audience = audience;

            byte[] key;
            if (File.Exists(keyFile))
            {
                var base64Key = File.ReadAllText(keyFile);
                key = Convert.FromBase64String(base64Key);
            }
            else
            {
                key = new HMACSHA256().Key;
                var base64Key = Convert.ToBase64String(key);
                File.WriteAllText(keyFile, base64Key);
            }

            Key = new SymmetricSecurityKey(key);

        }
    }
}
