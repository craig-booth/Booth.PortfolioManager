using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using Moq;
using FluentAssertions;
using FluentAssertions.AspNetCore.Mvc;

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

            result.Result.Should().BeOkObjectResult().Value.Should().Be(transaction);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void AddTransactionValidationError()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var transactionId = Guid.NewGuid();
            var transaction = new Aquisition() { Id = transactionId };

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.ApplyTransaction(transaction)).Returns(ServiceResult.Error("Error message")).Verifiable();

            var controller = new TransactionController();
            var result = controller.AddTransaction(service.Object, transaction);

            result.Should().BeBadRequestObjectResult().Error.Should().BeEquivalentTo(new[] { "Error message" });

            mockRepository.VerifyAll();
        }

        [Fact]
        public void AddTransaction()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var transactionId = Guid.NewGuid();
            var transaction = new Aquisition() { Id = transactionId };

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.ApplyTransaction(transaction)).Returns(ServiceResult.Ok()).Verifiable();

            var controller = new TransactionController();
            var result = controller.AddTransaction(service.Object, transaction);

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

            result.Result.Should().BeOkObjectResult().Value.Should().Be(transactions);

            mockRepository.VerifyAll();
        }

    }
}
