using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

using Booth.PortfolioManager.RestApi.CorporateActions;
using Booth.PortfolioManager.Web.Authentication;


namespace Booth.PortfolioManager.Web.Controllers
{
    [Authorize]
    [Route("api/stocks/{stockId:guid}/corporateactions")]
    [ApiController]
    public class CorporateActionController : ControllerBase
    {

        // GET : /api/stocks/{stockId}/corporateactions
        [HttpGet]
        public ActionResult<List<CorporateAction>> GetCorporateActions([FromRoute]Guid stockId, [FromQuery]DateTime? fromDate, [FromQuery]DateTime? toDate)
        {
            /*       var dateRange = new DateRange((fromDate != null) ? (DateTime)fromDate : DateUtils.NoStartDate, (toDate != null) ? (DateTime)toDate : DateTime.Today);

                   return _Service.GetCorporateActions(stockId, dateRange).ToList(); */
            throw new NotSupportedException();
        }

        // GET : /api/stocks/{stockId}/corporateactions/{id}
        [Route("{id:guid}")]
        [HttpGet]       
        public ActionResult<CorporateAction> GetCorporateAction([FromRoute]Guid stockId, [FromRoute]Guid id)
        {
            //   return _Service.GetCorporateAction(stockId, id);  
            throw new NotSupportedException();
        }

        // POST : /api/stocks/{stockId}/corporateactions
        [Authorize(Policy.CanMantainStocks)]
        [Route("")]
        [HttpPost]
        public ActionResult AddCorporateAction([FromRoute]Guid stockId, [FromBody] CorporateAction corporateAction)
        {
            /*  if (corporateAction == null)
                  throw new UnknownCorporateActionType();

              // Check id in URL and id in command match
              if (stockId != corporateAction.Stock)
                  return BadRequest("Id in command doesn't match id on URL");

              _Service.AddCorporateAction(stockId, corporateAction);

              return Ok(); */
            throw new NotSupportedException();
        }
    }

}

