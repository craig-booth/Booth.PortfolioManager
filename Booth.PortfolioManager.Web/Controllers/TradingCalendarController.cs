using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

using Booth.PortfolioManager.RestApi.TradingCalendars;
using Booth.PortfolioManager.Web.Authentication;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Mappers;

namespace Booth.PortfolioManager.Web.Controllers
{
    [Route("api/tradingcalendar")]
    [Authorize]
    [ApiController]
    public class TradingCalendarController : ControllerBase
    {

        private readonly ITradingCalendarService _Service;

        public TradingCalendarController(ITradingCalendarService service)
        {
            _Service = service;
        }

        // GET: api/tradingcalendar/{year}
        [HttpGet]
        [Route("{year:int}")]
        public ActionResult<TradingCalendar> Get([FromRoute]int year)
        {
            var result = _Service.Get(year);

            return result.ToActionResult<TradingCalendar>();
        }

        // POST: api/tradingcalendar/{year}
        [Authorize(Policy.CanMantainStocks)]
        [HttpPost]
        [Route("{year:int}")]
        public ActionResult Update([FromRoute]int year, [FromBody] TradingCalendar tradingCalendar)
        {
            var result = _Service.Update(tradingCalendar);

            return result.ToActionResult();
        } 
    }
}
