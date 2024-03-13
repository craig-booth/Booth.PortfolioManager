using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Booth.PortfolioManager.Web.Authentication
{
    public class AnonymousAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {

        public AnonymousAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, "DebugUser"),
                new Claim(ClaimTypes.Name, "DebugUser"),
                new Claim(ClaimTypes.Role, Role.Administrator)
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
