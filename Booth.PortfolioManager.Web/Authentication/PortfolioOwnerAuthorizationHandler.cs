using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Web.Utilities;
using AngleSharp.Html;
using Microsoft.AspNetCore.Http;
using Booth.PortfolioManager.Repository;
using System.Diagnostics.CodeAnalysis;

namespace Booth.PortfolioManager.Web.Authentication
{
    public class PortfolioOwnerRequirement : IAuthorizationRequirement { }

    class PortfolioOwnerAuthorizationHandler : AuthorizationHandler<PortfolioOwnerRequirement>
    {
        private readonly IHttpContextPortfolioAccessor _PortfolioAccessor;

        public PortfolioOwnerAuthorizationHandler(IHttpContextPortfolioAccessor portfolioAccessor)
        {
            _PortfolioAccessor = portfolioAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PortfolioOwnerRequirement requirement)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                var userId = new Guid(userIdClaim.Value);


                var portfolio = _PortfolioAccessor.GetReadOnlyPortfolio();
                return portfolio.ContinueWith(x => { if (IsPortfolioOwner(x.Result, userId)) context.Succeed(requirement); });
            }

            return Task.CompletedTask;
        }

        private bool IsPortfolioOwner(IReadOnlyPortfolio portfolio, Guid userId)
        {
            if (portfolio == null)
                return false;

            return (portfolio.Owner == userId);
        }
    }
}
