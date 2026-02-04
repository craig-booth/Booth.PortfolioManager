using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Web.Models.Stock;
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
        private readonly IStockService _StockService;
        private readonly IStockQuery _StockQuery;
        private readonly IStockMapper _Mapper;

        public StockController(IStockService stockService, IStockQuery stockQuery, IStockMapper mapper)
        {
            _StockService = stockService;
            _StockQuery = stockQuery;
            _Mapper = mapper;
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
                    resultDate = DateFromParameter(date);
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
                    resultDate = DateFromParameter(date);
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

            return Ok(stocks.Select(x => _Mapper.ToResponse(x, resultDate)).ToList()); 
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

            return Ok(_Mapper.ToResponse(stock, requestedDate));
        }

        // GET : /api/stocks/{id}/history
        [HttpGet]
        [Route("{id:guid}/history")]
        public ActionResult<StockHistoryResponse> GetHistory([FromRoute] Guid id)
        {
            var stock = _StockQuery.Get(id);
            if (stock == null)
                return NotFound();

           return Ok(_Mapper.ToHistoryResponse(stock)); 
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
            else if ((fromDate == null) && (toDate != null))
                dateRange = new DateRange(new Date(toDate!.Value).AddYears(-1).AddDays(1), new Date(toDate!.Value));
            else
                dateRange = new DateRange(new Date(fromDate!.Value), new Date(toDate!.Value));

            return Ok(_Mapper.ToPriceResponse(stock, dateRange)); 
        }

        // POST : /api/stocks
        [Authorize(Policy.CanMantainStocks)]
        [HttpPost]
        public async Task<ActionResult> CreateStock([FromBody] CreateStockCommand command)
        {
            var result = await _StockService.ListStockAsync(command.Id, command.AsxCode, command.Name, command.ListingDate, command.Trust, command.Category.ToDomain());

            return result.ToActionResult();
        }

        // POST : /api/stocks/{id}/change
        [Authorize(Policy.CanMantainStocks)]
        [Route("{id:guid}/change")]
        [HttpPost]
        public async Task<ActionResult> ChangeStock([FromRoute] Guid id, [FromBody] ChangeStockCommand command)
        {
            // Check id in URL and id in command match
            if (id != command.Id)
                return BadRequest("Id in command doesn't match id on URL");

            var result = await _StockService.ChangeStockAsync(id, command.ChangeDate, command.AsxCode, command.Name, command.Category.ToDomain());

            return result.ToActionResult();
        }

        // POST : /api/stocks/{id}/delist
        [Authorize(Policy.CanMantainStocks)]
        [Route("{id:guid}/delist")]
        [HttpPost]
        public async Task<ActionResult> DelistStock([FromRoute] Guid id, [FromBody] DelistStockCommand command)
        {
            // Check id in URL and id in command match
            if (id != command.Id)
                return BadRequest("Id in command doesn't match id on URL");

            var result = await _StockService.DelistStockAsync(id, command.DelistingDate);

            return result.ToActionResult();

        }

        // POST : /api/stocks/{id}/closingprices
        [Authorize(Policy.CanMantainStocks)]
        [Route("{id:guid}/closingprices")]
        [HttpPost]
        public async Task<ActionResult> UpdateClosingPrices([FromRoute] Guid id, [FromBody] UpdateClosingPricesCommand command)
        {
            // Check id in URL and id in command match
            if (id != command.Id)
                return BadRequest("Id in command doesn't match id on URL");

            var closingPrices = command.ClosingPrices.Select(x => new StockPrice(x.Date, x.Price));

            var result = await _StockService.UpdateClosingPricesAsync(id, closingPrices);

            return result.ToActionResult();
        }

        // POST : /api/stocks/{id}/changedividendrules
        [Authorize(Policy.CanMantainStocks)]
        [Route("{id}/changedividendrules")]
        [HttpPost]
        public async Task<ActionResult> ChangeDividendRules([FromRoute] Guid id, [FromBody] ChangeDividendRulesCommand command)
        {
            // Check id in URL and id in command match
            if (id != command.Id)
                return BadRequest("Id in command doesn't match id on URL");
         
            var result = await _StockService.ChangeDividendRulesAsync(id, command.ChangeDate, command.CompanyTaxRate, command.DividendRoundingRule, command.DrpActive, command.DrpMethod.ToDomain());

            return result.ToActionResult();
        }

        private bool MatchesQuery(StockProperties stock, string query)
        {
            return ((stock.AsxCode.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) || (stock.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        private Date DateFromParameter(DateTime? date)
        {
            return date == null ? Date.Today : new Date(date!.Value);
        }
        private DateRange DateRangeFromParameter(DateTime? fromDate, DateTime? toDate)
        {
            return new DateRange((fromDate != null) ? new Date(fromDate!.Value) : Date.MinValue, (toDate != null) ? new Date(toDate!.Value) : Date.MaxValue);
        }

    }
}
