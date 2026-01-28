using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xunit;
using Moq;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Web.Controllers;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.RestApi.Transactions;

namespace Booth.PortfolioManager.Web.Test.Controllers
{
    public class TransactionControllerTests
    {

        [Fact]
        public void GetTransactionNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var transactionId = Guid.NewGuid();
            var transaction = new Aquisition() { Id = transactionId };

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.GetTransaction(transactionId)).Returns(ServiceResult<Transaction>.NotFound()).Verifiable();

            var controller = new TransactionController();
            var result = controller.Get(service.Object, transactionId);

            result.Result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetTransaction()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var transactionId = Guid.NewGuid();
            var transaction = new Aquisition() { Id = transactionId };

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.GetTransaction(transactionId)).Returns(ServiceResult<Transaction>.Ok(transaction)).Verifiable();

            var controller = new TransactionController();
            var result = controller.Get(service.Object, transactionId);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(transaction);

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task AddTransactionValidationError()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var transactionId = Guid.NewGuid();
            var transaction = new Aquisition() { Id = transactionId };

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.AddTransactionAsync(transaction)).Returns(Task.FromResult(ServiceResult.Error("Error message"))).Verifiable();

            var controller = new TransactionController();
            var result = await controller.AddTransaction(service.Object, transaction);

            result.Should().BeBadRequestObjectResult().Which.Value.Should().BeEquivalentTo(new[] { "Error message" });

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task AddTransaction()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var transactionId = Guid.NewGuid();
            var transaction = new Aquisition() { Id = transactionId };

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.AddTransactionAsync(transaction)).Returns(Task.FromResult(ServiceResult.Ok())).Verifiable();

            var controller = new TransactionController();
            var result = await controller.AddTransaction(service.Object, transaction);

            result.Should().BeOkResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task UpdateTransactionNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var transactionId = Guid.NewGuid();
            var transaction = new Aquisition() { Id = transactionId };

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.UpdateTransactionAsync(transactionId, transaction)).Returns(Task.FromResult(ServiceResult.NotFound())).Verifiable();

            var controller = new TransactionController();
            var result = await controller.UpdateTransaction(service.Object, transactionId, transaction);

            result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task UpdateTransactionValidationError()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var transactionId = Guid.NewGuid();
            var transaction = new Aquisition() { Id = transactionId };

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.UpdateTransactionAsync(transactionId, transaction)).Returns(Task.FromResult(ServiceResult.Error("Error message"))).Verifiable();

            var controller = new TransactionController();
            var result = await controller.UpdateTransaction(service.Object, transactionId, transaction);

            result.Should().BeBadRequestObjectResult().Which.Value.Should().BeEquivalentTo(new[] { "Error message" });

            mockRepository.VerifyAll(); 
        }

        [Fact]
        public async Task UpdateTransaction()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var transactionId = Guid.NewGuid();
            var transaction = new Aquisition() { Id = transactionId };

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.UpdateTransactionAsync(transactionId, transaction)).Returns(Task.FromResult(ServiceResult.Ok())).Verifiable();

            var controller = new TransactionController();
            var result = await controller.UpdateTransaction(service.Object, transactionId, transaction);

            result.Should().BeOkResult();

            mockRepository.VerifyAll(); 
        }

        [Fact]
        public async Task DeleteTransactionNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var transactionId = Guid.NewGuid();

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.DeleteTransactionAsync(transactionId)).Returns(Task.FromResult(ServiceResult.NotFound())).Verifiable();

            var controller = new TransactionController();
            var result = await controller.DeleteTransaction(service.Object, transactionId);

            result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task DeleteTransactionValidationError()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var transactionId = Guid.NewGuid();

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.DeleteTransactionAsync(transactionId)).Returns(Task.FromResult(ServiceResult.Error("Error message"))).Verifiable();

            var controller = new TransactionController();
            var result = await controller.DeleteTransaction(service.Object, transactionId);

            result.Should().BeBadRequestObjectResult().Which.Value.Should().BeEquivalentTo(new[] { "Error message" });

            mockRepository.VerifyAll(); 
        }

        [Fact]
        public async Task DeleteTransaction()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var transactionId = Guid.NewGuid();

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.DeleteTransactionAsync(transactionId)).Returns(Task.FromResult(ServiceResult.Ok())).Verifiable();

            var controller = new TransactionController();
            var result = await controller.DeleteTransaction(service.Object, transactionId);

            result.Should().BeOkResult();

            mockRepository.VerifyAll(); 
        }

        [Fact]
        public void GetTransactionsForCorporateActionStockNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var transactions = new List<Transaction>();

            var service = mockRepository.Create<IPortfolioCorporateActionsService>();
            service.Setup(x => x.GetTransactionsForCorporateAction(stockId, actionId)).Returns(ServiceResult<List<Transaction>>.NotFound()).Verifiable();

            var controller = new TransactionController();
            var result = controller.GetTransactionsForCorporateAction(service.Object, stockId, actionId);

            result.Result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetTransactionsForCorporateActionActionNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var transactions = new List<Transaction>();

            var service = mockRepository.Create<IPortfolioCorporateActionsService>();
            service.Setup(x => x.GetTransactionsForCorporateAction(stockId, actionId)).Returns(ServiceResult<List<Transaction>>.NotFound()).Verifiable();

            var controller = new TransactionController();
            var result = controller.GetTransactionsForCorporateAction(service.Object, stockId, actionId);

            result.Result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetTransactionsForCorporateActionAction()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var transactions = new List<Transaction>();

            var service = mockRepository.Create<IPortfolioCorporateActionsService>();
            service.Setup(x => x.GetTransactionsForCorporateAction(stockId, actionId)).Returns(ServiceResult<List<Transaction>>.Ok(transactions)).Verifiable();

            var controller = new TransactionController();
            var result = controller.GetTransactionsForCorporateAction(service.Object, stockId, actionId);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(transactions);

            mockRepository.VerifyAll();
        }

    }
}
