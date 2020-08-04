using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Web.Authentication
{
    public class PortfolioOwnerRequirement : IAuthorizationRequirement { }

    public class PortfolioOwnerAuthorizationHandler : AuthorizationHandler<PortfolioOwnerRequirement, Portfolio>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                        PortfolioOwnerRequirement requirement,
                                                        Portfolio resource)
        {

            if (context.User.Identity.IsAuthenticated)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

                var userId = new Guid(userIdClaim.Value);
                if (resource.Owner == userId)
                    context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
