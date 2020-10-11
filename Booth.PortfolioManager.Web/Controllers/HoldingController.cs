using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

using Booth.Common;
using Booth.PortfolioManager.RestApi.Portfolios;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Authentication;
using Booth.PortfolioManager.Web.Mappers;

namespace Booth.PortfolioManager.Web.Controllers
{
    [Route("api/portfolio/{portfolioId:guid}/holdings")]
    [Authorize(Policy.IsPortfolioOwner)]
    [ApiController]
    public class HoldingController : ControllerBase
    {

        // GET:
        [HttpGet]
        public ActionResult<List<Holding>> Get([FromServices] IPortfolioService service, [FromQuery]DateTime? date, [FromQuery]DateTime? fromDate, [FromQuery]DateTime? toDate)
        {
            ServiceResult<List<Holding>> result;

            if (date != null)
                result = service.GetHoldings(DateFromParameter(date));
            else if ((fromDate == null) && (toDate == null))
                result = service.GetHoldings(DateFromParameter(date));
            else
                result = service.GetHoldings(DateRangeFromParameter(fromDate, toDate));

            return result.ToActionResult<List<Holding>>();
        }

        // GET:  id
        [Route("{id:guid}")]
        [HttpGet]
        public ActionResult<Holding> Get([FromServices] IPortfolioService service, [FromRoute]Guid id, [FromQuery]DateTime? date)
        {
            var result = service.GetHolding(id, DateFromParameter(date));

            return result.ToActionResult<Holding>();
        }

        // GET: properties
        [Route("{id:guid}/changedrpparticipation")]
        [HttpPost]
        public ActionResult ChangeDrpParticipation([FromServices] IPortfolioService service, [FromRoute] Guid id, [FromQuery] bool participate)
        {
            var result = service.ChangeDrpParticipation(id, participate);

            return result.ToActionResult();
        }

        // GET: id/value?fromDate&toDate
        [Route("{id:guid}/value")]
        [HttpGet]
        public ActionResult<PortfolioValueResponse> GetValue([FromServices] IPortfolioValueService service, [FromRoute]Guid id, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] ValueFrequency? frequency)
        {
            var requestedFrequency = frequency == null ? ValueFrequency.Day : frequency!.Value;

            var result = service.GetValue(id, DateRangeFromParameter(fromDate, toDate), requestedFrequency);

            return result.ToActionResult<PortfolioValueResponse>();
        } 

        // GET: transactions?fromDate&toDate
        [Route("{id:guid}/transactions")]
        [HttpGet]
        public ActionResult<TransactionsResponse> GetTransactions([FromServices] IPortfolioTransactionService service, [FromRoute] Guid id, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var result = service.GetTransactions(id, DateRangeFromParameter(fromDate, toDate));

            return result.ToActionResult<TransactionsResponse>();
        } 

        // GET: capitalgains?date
        [Route("{id:guid}/capitalgains")]
        [HttpGet]
        public ActionResult<SimpleUnrealisedGainsResponse> GetCapitalGains([FromServices] IPortfolioCapitalGainsService service, [FromRoute] Guid id, [FromQuery] DateTime? date)
        {
            var result = service.GetCapitalGains(id, DateFromParameter(date));

            return result.ToActionResult<SimpleUnrealisedGainsResponse>();
        } 

        // GET: detailedcapitalgains?date
        [Route("{id:guid}/detailedcapitalgains")]
        [HttpGet]
        public ActionResult<DetailedUnrealisedGainsResponse> GetDetailedCapitalGains([FromServices] IPortfolioCapitalGainsService service, [FromRoute] Guid id, [FromQuery] DateTime? date)
        {
            var result = service.GetDetailedCapitalGains(id, DateFromParameter(date));

            return result.ToActionResult<DetailedUnrealisedGainsResponse>();
        }

        // GET: corporateactions
        [Route("{id:guid}/corporateactions")]
        [HttpGet]
        public ActionResult<CorporateActionsResponse> GetCorporateActions([FromServices] IPortfolioCorporateActionsService service, [FromRoute] Guid id)
        {
            var result = service.GetCorporateActions(id);

            return result.ToActionResult<CorporateActionsResponse>();
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
                return new DateRange(new Date(fromDate!.Value), new Date(fromDate!.Value).AddYears(1).AddDays(-1));
            else if ((fromDate == null) && (toDate != null))
                return new DateRange(new Date(toDate!.Value).AddYears(-1).AddDays(1), new Date(toDate!.Value));
            else
                return new DateRange(Date.Today.AddYears(-1).AddDays(1), Date.Today);
        }
    } 
}