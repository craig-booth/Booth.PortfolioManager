using Booth.PortfolioManager.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Booth.PortfolioManager.Web.Mappers
{
    public static class ServiceResultMappers
    {

        public static ActionResult ToActionResult(this ServiceResult result)
        {
            if (result.Status == ServiceStatus.Ok)
                return new OkResult();
            else if (result.Status == ServiceStatus.NotFound)
                return new NotFoundResult();
            else if (result.Status == ServiceStatus.Error)
                return new BadRequestObjectResult(result.Errors);
            else
                return new BadRequestResult();
        }
    }
}
