using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Booth.PortfolioManager.Web.Authentication;
using Booth.PortfolioManager.RestApi.Transactions;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Mappers;

namespace Booth.PortfolioManager.Web.Controllers
{
    [Route("api/portfolio/{portfolioId:guid}/transactions")]
    [Authorize(Policy.IsPortfolioOwner)]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        // GET:  transactions/id
        [HttpGet("{id:guid}")]
        public ActionResult<Transaction> Get([FromServices] IPortfolioTransactionService service, Guid id)
        {
            var result = service.GetTransaction(id);

            return result.ToActionResult<Transaction>();
        }

        // POST: transactions
        [HttpPost]
        public ActionResult AddTransaction([FromServices] IPortfolioTransactionService service, [FromBody]Transaction transaction)
        {
            var result = service.ApplyTransaction(transaction);

            return result.ToActionResult();
        }

        // GET:  transactions/id/corporateaction/id
        [HttpGet("{stockId:guid}/corporateaction/{actionId:guid}")]
        public ActionResult<List<Transaction>> GetTransactionsForCorporateAction([FromServices] IPortfolioCorporateActionsService service, Guid stockId, Guid actionId)
        {
            var result = service.GetTransactionsForCorporateAction(stockId, actionId);

            return result.ToActionResult<List<Transaction>>();
        } 
    }
}
