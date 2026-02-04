using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xunit;
using Moq;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Web.Controllers;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Models.Transaction;

namespace Booth.PortfolioManager.Web.Test.Controllers
{
    public class CorporateActionControllerTests
    {
        [Fact]
        public void GetCorporateActionsStockNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.GetCorporateActions(stockId, new DateRange(Date.MinValue, Date.MaxValue))).Returns(ServiceResult<List<Models.CorporateAction.CorporateAction>>.NotFound()).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.GetCorporateActions(stockId, null, null);

            result.Result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCorporateActionsNoDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var actions = new List<Models.CorporateAction.CorporateAction>() { new Models.CorporateAction.Dividend() { Id = actionId } };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.GetCorporateActions(stockId, new DateRange(Date.MinValue, Date.MaxValue))).Returns(ServiceResult<List<Models.CorporateAction.CorporateAction>>.Ok(actions)).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.GetCorporateActions(stockId, null, null);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(actions);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCorporateActionsStartDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var actions = new List<Models.CorporateAction.CorporateAction>() { new Models.CorporateAction.Dividend() { Id = actionId } };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.GetCorporateActions(stockId, new DateRange(new Date(2000, 01, 01), Date.MaxValue))).Returns(ServiceResult<List<Models.CorporateAction.CorporateAction>>.Ok(actions)).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.GetCorporateActions(stockId, new DateTime(2000, 01, 01), null);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(actions);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCorporateActionsEndDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var actions = new List<Models.CorporateAction.CorporateAction>() { new Models.CorporateAction.Dividend() { Id = actionId } };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.GetCorporateActions(stockId, new DateRange(Date.MinValue, new Date(2000, 01, 01)))).Returns(ServiceResult<List<Models.CorporateAction.CorporateAction>>.Ok(actions)).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.GetCorporateActions(stockId, null, new DateTime(2000, 01, 01));

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(actions);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCorporateActionsBothDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var actions = new List<Models.CorporateAction.CorporateAction>() { new Models.CorporateAction.Dividend() { Id = actionId } };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.GetCorporateActions(stockId, new DateRange(new Date(2000, 01, 01), new Date(2001, 01, 01)))).Returns(ServiceResult<List<Models.CorporateAction.CorporateAction>>.Ok(actions)).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.GetCorporateActions(stockId, new DateTime(2000, 01, 01), new DateTime(2001, 01, 01));

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(actions);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCorporateActionStockNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.GetCorporateAction(stockId, actionId)).Returns(ServiceResult<Models.CorporateAction.CorporateAction>.NotFound()).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.GetCorporateAction(stockId, actionId);

            result.Result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCorporateActionNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.GetCorporateAction(stockId, actionId)).Returns(ServiceResult<Models.CorporateAction.CorporateAction>.NotFound()).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.GetCorporateAction(stockId, actionId);

            result.Result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCorporateAction()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new Models.CorporateAction.Dividend() { Id = actionId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.GetCorporateAction(stockId, actionId)).Returns(ServiceResult<Models.CorporateAction.CorporateAction>.Ok(action)).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.GetCorporateAction(stockId, actionId);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(action);

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task AddCorporateActionStockNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new Models.CorporateAction.Dividend() { Id = actionId, Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.AddCorporateActionAsync(stockId, action)).Returns(Task.FromResult(ServiceResult.NotFound())).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = await controller.AddCorporateAction(stockId, action);

            result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task AddCorporateActionNullObject()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();

            var service = mockRepository.Create<ICorporateActionService>();

            var controller = new CorporateActionController(service.Object);
            var result = await controller.AddCorporateAction(stockId, null);

            result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task AddCorporateActionStockWrong()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new Models.CorporateAction.CapitalReturn() { Id = actionId, Stock = Guid.NewGuid() };

            var service = mockRepository.Create<ICorporateActionService>();

            var controller = new CorporateActionController(service.Object);
            var result = await controller.AddCorporateAction(stockId, action);

            result.Should().BeBadRequestResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task AddCapitalReturn()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new Models.CorporateAction.CapitalReturn() { Id = actionId, Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.AddCorporateActionAsync(stockId, It.IsAny<Models.CorporateAction.CapitalReturn>())).Returns(Task.FromResult(ServiceResult.Ok())).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = await controller.AddCorporateAction(stockId, action);

            result.Should().BeOkResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task AddCompositeAction()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new Models.CorporateAction.CompositeAction() { Id = actionId, Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.AddCorporateActionAsync(stockId, It.IsAny<Models.CorporateAction.CompositeAction>())).Returns(Task.FromResult(ServiceResult.Ok())).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = await controller.AddCorporateAction(stockId, action);

            result.Should().BeOkResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task AddDividend()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new Models.CorporateAction.Dividend() { Id = actionId, Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.AddCorporateActionAsync(stockId, It.IsAny<Models.CorporateAction.Dividend>())).Returns(Task.FromResult(ServiceResult.Ok())).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = await controller.AddCorporateAction(stockId, action);

            result.Should().BeOkResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task AddSplitConsolidation()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new Models.CorporateAction.SplitConsolidation() { Id = actionId, Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.AddCorporateActionAsync(stockId, It.IsAny<Models.CorporateAction.SplitConsolidation>())).Returns(Task.FromResult(ServiceResult.Ok())).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = await controller.AddCorporateAction(stockId, action);

            result.Should().BeOkResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task AddTransformation()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new Models.CorporateAction.Transformation() { Id = actionId, Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.AddCorporateActionAsync(stockId, It.IsAny<Models.CorporateAction.Transformation>())).Returns(Task.FromResult(ServiceResult.Ok())).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = await controller.AddCorporateAction(stockId, action);

            result.Should().BeOkResult();

            mockRepository.VerifyAll();
        }


        [Fact]
        public async Task UpdateCorporateActionStockNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new Models.CorporateAction.Dividend() { Id = actionId, Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.UpdateCorporateActionAsync(stockId, action)).Returns(Task.FromResult(ServiceResult.NotFound())).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = await controller.UpdateCorporateAction(stockId, actionId, action);

            result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task UpdateCorporateActionNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new Models.CorporateAction.CapitalReturn() { Id = actionId, Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.UpdateCorporateActionAsync(stockId, action)).Returns(Task.FromResult(ServiceResult.NotFound())).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = await controller.UpdateCorporateAction(stockId, actionId, action);

            result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task UpdateCorporateActionNullObject()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();

            var service = mockRepository.Create<ICorporateActionService>();

            var controller = new CorporateActionController(service.Object);
            var result = await controller.UpdateCorporateAction(stockId, actionId, null);

            result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task UpdateCorporateActionIdWrong()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new Models.CorporateAction.CapitalReturn() { Id = Guid.NewGuid(), Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();

            var controller = new CorporateActionController(service.Object);
            var result = await controller.UpdateCorporateAction(stockId, actionId, action);

            result.Should().BeBadRequestResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task UpdateCorporateActionStockWrong()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new Models.CorporateAction.CapitalReturn() { Id = actionId, Stock = Guid.NewGuid() };

            var service = mockRepository.Create<ICorporateActionService>();

            var controller = new CorporateActionController(service.Object);
            var result = await controller.UpdateCorporateAction(stockId, actionId, action);

            result.Should().BeBadRequestResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task UpdateCorporateAction()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new Models.CorporateAction.CapitalReturn() { Id = actionId, Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.UpdateCorporateActionAsync(stockId, action)).Returns(Task.FromResult(ServiceResult.Ok())).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = await controller.UpdateCorporateAction(stockId, actionId, action);

            result.Should().BeOkResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task DeleteCorporateActionStockNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.DeleteCorporateActionAsync(stockId, actionId)).Returns(Task.FromResult(ServiceResult.NotFound())).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = await controller.DeleteCorporateAction(stockId, actionId);

            result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task DeleteCorporateActionNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.DeleteCorporateActionAsync(stockId, actionId)).Returns(Task.FromResult(ServiceResult.NotFound())).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = await controller.DeleteCorporateAction(stockId, actionId);

            result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task DeleteCorporateAction()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.DeleteCorporateActionAsync(stockId, actionId)).Returns(Task.FromResult(ServiceResult.Ok())).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = await controller.DeleteCorporateAction(stockId, actionId);

            result.Should().BeOkResult();

            mockRepository.VerifyAll();
        }
    }

}
