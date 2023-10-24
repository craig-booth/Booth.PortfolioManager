using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.RestApi.Portfolios;
using Booth.PortfolioManager.Web.Authentication;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Mappers;


namespace Booth.PortfolioManager.Web.Controllers
{
    [Route("api/portfolio")]
    [Authorize(Policy.CanCreatePortfolio)]
    [ApiController]
    public class PortfolioCreateController : ControllerBase
    {

        // POST :
        [Authorize(Policy.CanCreatePortfolio)]
        [HttpPost]
        public ActionResult CreatePortfolio([FromServices] IPortfolioService service, [FromBody] CreatePortfolioCommand command)
        {
            // Retreive the authenticated user
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var userId = new Guid(userIdClaim.Value);

            var result = service.CreatePortfolio(command.Id, command.Name, userId);

            return result.ToActionResult();
        }
    }
}
