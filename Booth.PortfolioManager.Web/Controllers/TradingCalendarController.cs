using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

using Booth.PortfolioManager.RestApi.TradingCalendars;
using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.Web.Authentication;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Mappers;

namespace Booth.PortfolioManager.Web.Controllers
{
    [Route("api/tradingcalendars")]
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
        public ActionResult<RestApi.TradingCalendars.TradingCalendar> Get([FromRoute]int year)
        {
            var result = _Service.Get(TradingCalendarIds.ASX, year);

            return result.ToActionResult<RestApi.TradingCalendars.TradingCalendar>();
        }

        // POST: api/tradingcalendar/{year}
        [Authorize(Policy.CanMantainStocks)]
        [HttpPost]
        [Route("{year:int}")]
        public ActionResult Update([FromRoute]int year, [FromBody] RestApi.TradingCalendars.TradingCalendar tradingCalendar)
        {
            var result = _Service.Update(TradingCalendarIds.ASX, tradingCalendar);

            return result.ToActionResult();
        } 
    }
}
