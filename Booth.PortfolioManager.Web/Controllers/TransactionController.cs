using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Booth.PortfolioManager.Web.Authentication;
using Booth.PortfolioManager.Web.Models.Transaction;
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
        public async Task<ActionResult> AddTransaction([FromServices] IPortfolioTransactionService service, [FromBody]Transaction transaction)
        {
            var result = await service.AddTransactionAsync(transaction);

            return result.ToActionResult();
        }

        // POST: transactions/id
        [HttpPost("{id:guid}")]
        public async Task<ActionResult> UpdateTransaction([FromServices] IPortfolioTransactionService service, Guid id, [FromBody] Transaction transaction)
        {
            var result = await service.UpdateTransactionAsync(id, transaction);

            return result.ToActionResult();
        }

        // DELETE: transactions/id
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteTransaction([FromServices] IPortfolioTransactionService service, Guid id)
        {
            var result = await service.DeleteTransactionAsync(id);

            return result.ToActionResult();
        }

        // GET:  transactions/id/corporateaction/id
        [HttpGet("{stockId:guid}/corporateactions/{actionId:guid}")]
        public ActionResult<List<Transaction>> GetTransactionsForCorporateAction([FromServices] IPortfolioCorporateActionsService service, Guid stockId, Guid actionId)
        {
            var result = service.GetTransactionsForCorporateAction(stockId, actionId);

            return result.ToActionResult<List<Transaction>>();
        } 
    }
}
