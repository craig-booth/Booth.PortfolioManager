using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.RestApi.Stocks;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Utilities;
using Booth.PortfolioManager.Web.Mappers;
using Booth.PortfolioManager.Web.Authentication;

namespace Booth.PortfolioManager.Web.Controllers
{

    [ApiController]
    [Authorize]
    [Route("api/stocks")]
    public class StockController : ControllerBase
    {
        private readonly  IStockService _StockService;
        private readonly IStockQuery _StockQuery;

        public StockController(IStockService stockService, IStockQuery stockQuery)
        {
            _StockService = stockService;
            _StockQuery = stockQuery;
        }

        // GET: api/stocks
        [HttpGet]
        public ActionResult<List<StockResponse>> Get([FromQuery] string query, [FromQuery] DateTime? date, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            IEnumerable<Stock> stocks = null;
            Date resultDate;

            if ((date != null) && (fromDate != null) && (toDate != null))
                return BadRequest("Cannot specify a date and a date range");

            if (query == null)
            {
                if (date != null)
                {
                    resultDate = DateFromParameter(date, Date.Today);
                    stocks = _StockQuery.All(resultDate);
                }
                else if ((fromDate != null) || (toDate != null))
                {
                    var dateRange = DateRangeFromParameter(fromDate, toDate);
                    resultDate = dateRange.ToDate;

                    stocks = _StockQuery.All(dateRange);    
                }
                else
                {
                    stocks = _StockQuery.All();
                    resultDate = Date.Today;
                }
            }
            else
            {
                if (date != null)
                {
                    resultDate = DateFromParameter(date, Date.Today);
                    stocks = _StockQuery.Find(resultDate, x => MatchesQuery(x, query));
                }
                else if ((fromDate != null) || (toDate != null))
                {
                    var dateRange = DateRangeFromParameter(fromDate, toDate);
                    resultDate = dateRange.ToDate;

                    stocks = _StockQuery.Find(dateRange, x => MatchesQuery(x, query));
                }
                else
                {
                    stocks = _StockQuery.Find(x => MatchesQuery(x, query));
                    resultDate = Date.Today;
                }
            } 

            return Ok(stocks.Select(x => x.ToResponse(resultDate)).ToList()); 
        }

        // GET: api/stocks/{id}
        [HttpGet]
        [Route("{id:guid}")]
        public ActionResult<StockResponse> Get([FromRoute] Guid id, [FromQuery] DateTime? date)
        {     
            var stock = _StockQuery.Get(id);
            if (stock == null)
                return NotFound();

            Date requestedDate;
            if (date != null)
                requestedDate = new Date((DateTime)date);
            else
                requestedDate = Date.Today;

            return Ok(stock.ToResponse(requestedDate));
        }

        // GET : /api/stocks/{id}/history
        [HttpGet]
        [Route("{id:guid}/history")]
        public ActionResult<StockHistoryResponse> GetHistory([FromRoute] Guid id)
        {
            var stock = _StockQuery.Get(id);
            if (stock == null)
                return NotFound();

           return Ok(stock.ToHistoryResponse()); 
        }

        // GET : /api/stocks/{id}/closingprices
        [HttpGet]
        [Route("{id:guid}/closingprices")]
        public ActionResult<StockPriceResponse> GetClosingPrices([FromRoute] Guid id, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var stock = _StockQuery.Get(id);
            if (stock == null)
                return NotFound();

            DateRange dateRange;
            if ((fromDate == null) && (toDate == null))
                dateRange = new DateRange(Date.Today.AddYears(-1).AddDays(1), Date.Today);
            else if ((fromDate != null) && (toDate == null))
                dateRange = new DateRange(new Date(fromDate!.Value), new Date(fromDate!.Value).AddYears(1).AddDays(-1));
            else if ((fromDate != null) && (toDate == null))
                dateRange = new DateRange(new Date(toDate!.Value).AddYears(-1).AddDays(1), new Date(toDate!.Value));
            else
                dateRange = new DateRange(new Date(toDate!.Value), new Date(toDate!.Value));

            return Ok(stock.ToPriceResponse(dateRange)); 
        }

        // POST : /api/stocks
        [Authorize(Policy.CanMantainStocks)]
        [HttpPost]
        public ActionResult CreateStock([FromBody] CreateStockCommand command)
        {
            ServiceResult result;
         //   if (command.ChildSecurities.Count == 0)
                result = _StockService.ListStock(command.Id, command.AsxCode, command.Name, command.ListingDate, command.Trust, command.Category.ToDomain());
            //  else
            //      result = _StockService.ListStapledSecurity(command.Id, command.AsxCode, command.Name, command.ListingDate, command.Category, command.ChildSecurities.Select(x => new StapledSecurityChild(x.ASXCode, x.Name, x.Trust)));

            return result.ToActionResult();
        }

        // POST : /api/stocks/{id}/change
        [Authorize(Policy.CanMantainStocks)]
        [Route("{id:guid}/change")]
        [HttpPost]
        public ActionResult ChangeStock([FromRoute] Guid id, [FromBody] ChangeStockCommand command)
        {
            // Check id in URL and id in command match
            if (id != command.Id)
                return BadRequest("Id in command doesn't match id on URL");

            var result = _StockService.ChangeStock(id, command.ChangeDate, command.AsxCode, command.Name, command.Category.ToDomain());

            return result.ToActionResult();
        }

        // POST : /api/stocks/{id}/delist
        [Authorize(Policy.CanMantainStocks)]
        [Route("{id:guid}/delist")]
        [HttpPost]
        public ActionResult DelistStock([FromRoute] Guid id, [FromBody] DelistStockCommand command)
        {
            // Check id in URL and id in command match
            if (id != command.Id)
                return BadRequest("Id in command doesn't match id on URL");

            var result = _StockService.DelistStock(id, command.DelistingDate);

            return result.ToActionResult();

        }

        // POST : /api/stocks/{id}/closingprices
        [Authorize(Policy.CanMantainStocks)]
        [Route("{id:guid}/closingprices")]
        [HttpPost]
        public ActionResult UpdateClosingPrices([FromRoute] Guid id, [FromBody] UpdateClosingPricesCommand command)
        {
            // Check id in URL and id in command match
            if (id != command.Id)
                return BadRequest("Id in command doesn't match id on URL");

            var closingPrices = command.ClosingPrices.Select(x => new StockPrice(x.Date, x.Price));

            var result = _StockService.UpdateClosingPrices(id, closingPrices);

            return result.ToActionResult();
        }

        // POST : /api/stocks/{id}/changedividendrules
        [Authorize(Policy.CanMantainStocks)]
        [Route("{id}/changedividendrules")]
        [HttpPost]
        public ActionResult ChangeDividendRules([FromRoute] Guid id, [FromBody] ChangeDividendRulesCommand command)
        {
            // Check id in URL and id in command match
            if (id != command.Id)
                return BadRequest("Id in command doesn't match id on URL");
         
            var result = _StockService.ChangeDividendRules(id, command.ChangeDate, command.CompanyTaxRate, command.DividendRoundingRule, command.DrpActive, command.DrpMethod.ToDomain());

            return result.ToActionResult();
        }

  /*      // GET : /api/stocks/{id}/relativenta
        [Route("{id:guid}/relativenta")]
        [HttpGet]
        public ActionResult<RelativeNtaResponse> GetRelativeNta([FromRoute] Guid id, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            throw new NotSupportedException();
                      var stock = _StockQuery.Get(id);
                      if (stock == null)
                          return NotFound();

                      if (stock is StapledSecurity stapledSecurity)
                      {
                          var dateRange = new DateRange((fromDate != null) ? (DateTime)fromDate : DateUtils.NoStartDate, (toDate != null) ? (DateTime)toDate : DateTime.Today);

                          return Ok(stapledSecurity.ToRelativeNTAResponse(dateRange));
                      }
                      else
                      {
                          return BadRequest("Relative NTAs only apply stapled securities");
                      }

                     
        }*/

        // POST : /api/stocks/{id}/relativenta
     /*   [Authorize(Policy.CanMantainStocks)]
        [Route("{id:guid}/relativenta")]
        [HttpPost]
        public ActionResult ChangeRelativeNta([FromRoute] Guid id, [FromBody] ChangeRelativeNtaCommand command)
        {
            throw new NotSupportedException();
                        // Check id in URL and id in command match
                        if (id != command.Id)
                            return BadRequest("Id in command doesn't match id on URL");

                        var stock = _StockQuery.Get(id);
                        if (stock == null)
                            return NotFound();

                        if (stock is StapledSecurity stapledSecurity)
                        {
                            if (command.RelativeNTAs.Count != stapledSecurity.ChildSecurities.Count)
                            {
                                return BadRequest(String.Format("The number of relative ntas provided ({0}) did not match the number of child securities ({1})", command.RelativeNTAs.Count, stapledSecurity.ChildSecurities.Count));
                            }

                            var ntas = new decimal[stapledSecurity.ChildSecurities.Count];
                            for (var i = 0; i < stapledSecurity.ChildSecurities.Count; i++)
                            {
                                var nta = command.RelativeNTAs.Find(x => x.ChildSecurity == stapledSecurity.ChildSecurities[i].ASXCode);
                                if (nta == null)
                                    return BadRequest(String.Format("Relative nta not provided for {0}", stapledSecurity.ChildSecurities[i].ASXCode));

                                ntas[i] = nta.Percentage;
                            }

                            try
                            {
                                _StockService.ChangeRelativeNTAs(id, command.ChangeDate, ntas);
                            }
                            catch (Exception e)
                            {
                                return BadRequest(e.Message);
                            }

                            return Ok();
                        }
                        else
                        {
                            return BadRequest("Relative NTAs only apply stapled securities");
                        }

                        
        } */

        private bool MatchesQuery(StockProperties stock, string query)
        {
            return ((stock.AsxCode.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) || (stock.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        private Date DateFromParameter(DateTime? date, Date defaultDate)
        {
            return date == null ? default : new Date(date!.Value);
        }
        private DateRange DateRangeFromParameter(DateTime? fromDate, DateTime? toDate)
        {
            return new DateRange((fromDate != null) ? new Date(fromDate!.Value) : Date.MinValue, (toDate != null) ? new Date(toDate!.Value) : Date.MaxValue);
        }

        private DateRange DateRangeFromParameter(DateTime? fromDate, Date defaultFromDate, DateTime? toDate, Date defaultToDate)
        {
            return new DateRange((fromDate != null) ? new Date(fromDate!.Value) : Date.MinValue, (toDate != null) ? new Date(toDate!.Value) : Date.MaxValue);
        }
    }
}
