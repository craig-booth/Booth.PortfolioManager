using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Web.Models.Portfolio;
using Booth.PortfolioManager.Web.Authentication;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Mappers;


namespace Booth.PortfolioManager.Web.Controllers
{
    [Route("api/portfolio/{portfolioId:guid}")]
    [Authorize(Policy.IsPortfolioOwner)]
    [ApiController]
    public class PortfolioController : ControllerBase
    {

        // GET: properties
        [Route("properties")]
        [HttpGet]
        public ActionResult<PortfolioPropertiesResponse> GetProperties([FromServices] IPortfolioPropertiesService service)
        {
            var result = service.GetProperties();

            return result.ToActionResult();
        }


        // GET: summary
        [Route("summary")]
        [HttpGet]
        public ActionResult<PortfolioSummaryResponse> GetSummary([FromServices] IPortfolioSummaryService service, [FromQuery] DateTime? date)
        {
            var requestedDate = DateFromParameter(date);
            var result = service.GetSummary(requestedDate);

            return result.ToActionResult();
        }

        // GET: performance
        [Route("performance")]
        [HttpGet]
        public ActionResult<PortfolioPerformanceResponse> GetPerformance([FromServices] IPortfolioPerformanceService service, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var dateRange = DateRangeFromParameter(fromDate, toDate);
            var result = service.GetPerformance(dateRange);

            return result.ToActionResult();
        }

        // GET: value
        [Route("value")]
        [HttpGet]
        public async Task<ActionResult<PortfolioValueResponse>> GetValue([FromServices] IPortfolioValueService service, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] ValueFrequency? frequency)
        {
            var dateRange = DateRangeFromParameter(fromDate, toDate);

            var requestedFrequency = frequency == null ? ValueFrequency.Day : frequency!.Value;

            var result = await service.GetValueAsync(dateRange, requestedFrequency);

            return result.ToActionResult();
        }

        // GET: transactions
        [Route("transactions")]
        [HttpGet]
        public ActionResult<TransactionsResponse> GetTransactions([FromServices] IPortfolioTransactionService service, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var dateRange = DateRangeFromParameter(fromDate, toDate);
            var result = service.GetTransactions(dateRange);

            return result.ToActionResult();
        }

        // GET: capitalgains
        [Route("capitalgains")]
        [HttpGet]
        public ActionResult<SimpleUnrealisedGainsResponse> GetCapitalGains([FromServices] IPortfolioCapitalGainsService service, [FromQuery] DateTime? date)
        {
            var requestedDate = DateFromParameter(date);
            var result = service.GetCapitalGains(requestedDate);

            return result.ToActionResult();
        }

        // GET: detailedcapitalgains
        [Route("detailedcapitalgains")]
        [HttpGet]
        public ActionResult<DetailedUnrealisedGainsResponse> GetDetailedCapitalGains([FromServices] IPortfolioCapitalGainsService service, [FromQuery] DateTime? date)
        {
            var requestedDate = DateFromParameter(date);
            var result = service.GetDetailedCapitalGains(requestedDate);

            return result.ToActionResult();
        }

        // GET: cgtliability
        [Route("cgtliability")]
        [HttpGet]
        public ActionResult<CgtLiabilityResponse> GetCGTLiability([FromServices] IPortfolioCgtLiabilityService service, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var dateRange = DateRangeFromParameter(fromDate, toDate);
            var result = service.GetCGTLiability(dateRange);

            return result.ToActionResult();
        }

        // GET: cashaccount
        [Route("cashaccount")]
        [HttpGet]
        public ActionResult<CashAccountTransactionsResponse> GetCashAccount([FromServices] ICashAccountService service, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var dateRange = DateRangeFromParameter(fromDate, toDate);
            var result = service.GetTransactions(dateRange);

            return result.ToActionResult();
        }

        // GET: income
        [Route("income")]
        [HttpGet]
        public ActionResult<IncomeResponse> GetIncome([FromServices] IPortfolioIncomeService service, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var dateRange = DateRangeFromParameter(fromDate, toDate);
            var result = service.GetIncome(dateRange);

            return result.ToActionResult();
        }

        // GET: corporateactions
        [Route("corporateactions")]
        [HttpGet]
        public ActionResult<CorporateActionsResponse> GetCorporateActions([FromServices] IPortfolioCorporateActionsService service)
        {
            var result = service.GetCorporateActions();

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
                return new DateRange(new Date(fromDate!.Value), new Date(fromDate!.Value).AddYears(1).AddDays(-1));
            else if ((fromDate == null) && (toDate != null))
                return new DateRange(new Date(toDate!.Value).AddYears(-1).AddDays(1), new Date(toDate!.Value)); 
            else
                return new DateRange(Date.Today.AddYears(-1).AddDays(1), Date.Today);
        }
    }
}
