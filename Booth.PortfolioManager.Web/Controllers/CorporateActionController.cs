using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

using Booth.Common;
using Booth.PortfolioManager.RestApi.CorporateActions;
using Booth.PortfolioManager.Web.Authentication;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Mappers;

namespace Booth.PortfolioManager.Web.Controllers
{
    [Authorize]
    [Route("api/stocks/{stockId:guid}/corporateactions")]
    [ApiController]
    public class CorporateActionController : ControllerBase
    {
        private readonly ICorporateActionService _Service; 
        public CorporateActionController(ICorporateActionService service)
        {
            _Service = service;
        }

        // GET : /api/stocks/{stockId}/corporateactions
        [HttpGet]
        public ActionResult<List<CorporateAction>> GetCorporateActions([FromRoute]Guid stockId, [FromQuery]DateTime? fromDate, [FromQuery]DateTime? toDate)
        {
            var result = _Service.GetCorporateActions(stockId, DateRangeFromParameter(fromDate, toDate));

            return result.ToActionResult<List<CorporateAction>>();
        }

        // GET : /api/stocks/{stockId}/corporateactions/{id}
        [Route("{id:guid}")]
        [HttpGet]       
        public ActionResult<CorporateAction> GetCorporateAction([FromRoute]Guid stockId, [FromRoute]Guid id)
        {
            var result = _Service.GetCorporateAction(stockId, id);

            return result.ToActionResult<CorporateAction>();
        }

        // POST : /api/stocks/{stockId}/corporateactions
        [Authorize(Policy.CanMantainStocks)]
        [Route("")]
        [HttpPost]
        public ActionResult AddCorporateAction([FromRoute]Guid stockId, [FromBody] CorporateAction corporateAction)
        {
            if (corporateAction == null)
                return NotFound();
            
            if (corporateAction.Stock != stockId)
                return BadRequest();

            var result = _Service.AddCorporateAction(stockId, corporateAction);

            return result.ToActionResult();
        }

        // POST : /api/stocks/{stockId}/corporateactions/{id}
        [Authorize(Policy.CanMantainStocks)]
        [Route("{id:guid}")]
        [HttpPost]
        public ActionResult UpdateCorporateAction([FromRoute] Guid stockId, [FromRoute] Guid id, [FromBody] CorporateAction corporateAction)
        {
            if (corporateAction == null)
                return NotFound();

            if (corporateAction.Stock != stockId)
                return BadRequest();

            if (corporateAction.Id != id)
                return BadRequest();

            var result = _Service.UpdateCorporateAction(stockId, corporateAction);

            return result.ToActionResult();
        }

        // DELETE : /api/stocks/{stockId}/corporateactions/{id}
        [Authorize(Policy.CanMantainStocks)]
        [Route("{id:guid}")]
        [HttpPost]
        public ActionResult DeleteCorporateAction([FromRoute] Guid stockId, [FromRoute] Guid id)
        {
            var result = _Service.DeleteCorporateAction(stockId, id);

            return result.ToActionResult();
        }



        private Date DateFromParameter(DateTime? date)
        {
            return date == null ? Date.Today : new Date(date!.Value);
        }

        private DateRange DateRangeFromParameter(DateTime? fromDate, DateTime? toDate)
        {
            if ((fromDate != null) && (toDate != null))
                return new DateRange(new Date(fromDate!.Value), new Date(toDate!.Value));
            else if ((fromDate != null) && (toDate == null))
                return new DateRange(new Date(fromDate!.Value), Date.MaxValue);
            else if ((fromDate == null) && (toDate != null))
                return new DateRange(Date.MinValue, new Date(toDate!.Value));
            else
                return new DateRange(Date.MinValue, Date.MaxValue);
        }
    }

}

