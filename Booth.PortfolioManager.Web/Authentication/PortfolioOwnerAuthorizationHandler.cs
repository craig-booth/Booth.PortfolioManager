using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Web.Utilities;

namespace Booth.PortfolioManager.Web.Authentication
{
    public class PortfolioOwnerRequirement : IAuthorizationRequirement { }

    class PortfolioOwnerAuthorizationHandler : AuthorizationHandler<PortfolioOwnerRequirement>
    {
        private readonly IReadOnlyPortfolio _Portfolio;

        public PortfolioOwnerAuthorizationHandler(IHttpContextPortfolioAccessor portfolioAccessor)
        {
            _Portfolio = portfolioAccessor.ReadOnlyPortfolio;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PortfolioOwnerRequirement requirement)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                if (_Portfolio == null)
                    context.Succeed(requirement);
                else
                {
                    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

                    var userId = new Guid(userIdClaim.Value);
                    if (_Portfolio.Owner == userId)
                        context.Succeed(requirement);
                }                   
            }


            return Task.CompletedTask;
        }
    }
}
