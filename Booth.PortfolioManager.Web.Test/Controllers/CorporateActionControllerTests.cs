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
    public class CorporateActionControllerTests
    {
        [Fact]
        public void GetCorporateActionsStockNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.GetCorporateActions(stockId, new DateRange(Date.MinValue, Date.MaxValue))).Returns(ServiceResult<List<RestApi.CorporateActions.CorporateAction>>.NotFound()).Verifiable();

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
            var actions = new List<RestApi.CorporateActions.CorporateAction>() { new RestApi.CorporateActions.Dividend() { Id = actionId } };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.GetCorporateActions(stockId, new DateRange(Date.MinValue, Date.MaxValue))).Returns(ServiceResult<List<RestApi.CorporateActions.CorporateAction>>.Ok(actions)).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.GetCorporateActions(stockId, null, null);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(actions);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCorporateActionsStartDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var actions = new List<RestApi.CorporateActions.CorporateAction>() { new RestApi.CorporateActions.Dividend() { Id = actionId } };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.GetCorporateActions(stockId, new DateRange(new Date(2000, 01, 01), Date.MaxValue))).Returns(ServiceResult<List<RestApi.CorporateActions.CorporateAction>>.Ok(actions)).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.GetCorporateActions(stockId, new DateTime(2000, 01, 01), null);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(actions);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCorporateActionsEndDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var actions = new List<RestApi.CorporateActions.CorporateAction>() { new RestApi.CorporateActions.Dividend() { Id = actionId } };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.GetCorporateActions(stockId, new DateRange(Date.MinValue, new Date(2000, 01, 01)))).Returns(ServiceResult<List<RestApi.CorporateActions.CorporateAction>>.Ok(actions)).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.GetCorporateActions(stockId, null, new DateTime(2000, 01, 01));

            result.Result.Should().BeOkObjectResult().Value.Should().Be(actions);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCorporateActionsBothDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var actions = new List<RestApi.CorporateActions.CorporateAction>() { new RestApi.CorporateActions.Dividend() { Id = actionId } };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.GetCorporateActions(stockId, new DateRange(new Date(2000, 01, 01), new Date(2001, 01, 01)))).Returns(ServiceResult<List<RestApi.CorporateActions.CorporateAction>>.Ok(actions)).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.GetCorporateActions(stockId, new DateTime(2000, 01, 01), new DateTime(2001, 01, 01));

            result.Result.Should().BeOkObjectResult().Value.Should().Be(actions);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCorporateActionStockNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.GetCorporateAction(stockId, actionId)).Returns(ServiceResult<RestApi.CorporateActions.CorporateAction>.NotFound()).Verifiable();

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
            service.Setup(x => x.GetCorporateAction(stockId, actionId)).Returns(ServiceResult<RestApi.CorporateActions.CorporateAction>.NotFound()).Verifiable();

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
            var action = new RestApi.CorporateActions.Dividend() { Id = actionId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.GetCorporateAction(stockId, actionId)).Returns(ServiceResult<RestApi.CorporateActions.CorporateAction>.Ok(action)).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.GetCorporateAction(stockId, actionId);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(action);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void AddCorporateActionStockNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new RestApi.CorporateActions.Dividend() { Id = actionId, Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.AddCorporateAction(stockId, action)).Returns(ServiceResult.NotFound()).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.AddCorporateAction(stockId, action);

            result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void AddCorporateActionNullObject()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();

            var service = mockRepository.Create<ICorporateActionService>();

            var controller = new CorporateActionController(service.Object);
            var result = controller.AddCorporateAction(stockId, null);

            result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void AddCorporateActionStockWrong()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new RestApi.CorporateActions.CapitalReturn() { Id = actionId, Stock = Guid.NewGuid() };

            var service = mockRepository.Create<ICorporateActionService>();

            var controller = new CorporateActionController(service.Object);
            var result = controller.AddCorporateAction(stockId, action);

            result.Should().BeBadRequestResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void AddCapitalReturn()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new RestApi.CorporateActions.CapitalReturn() { Id = actionId, Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.AddCorporateAction(stockId, It.IsAny<RestApi.CorporateActions.CapitalReturn>())).Returns(ServiceResult.Ok()).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.AddCorporateAction(stockId, action);

            result.Should().BeOkResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void AddCompositeAction()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new RestApi.CorporateActions.CompositeAction() { Id = actionId, Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.AddCorporateAction(stockId, It.IsAny<RestApi.CorporateActions.CompositeAction>())).Returns(ServiceResult.Ok()).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.AddCorporateAction(stockId, action);

            result.Should().BeOkResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void AddDividend()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new RestApi.CorporateActions.Dividend() { Id = actionId, Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.AddCorporateAction(stockId, It.IsAny<RestApi.CorporateActions.Dividend>())).Returns(ServiceResult.Ok()).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.AddCorporateAction(stockId, action);

            result.Should().BeOkResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void AddSplitConsolidation()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new RestApi.CorporateActions.SplitConsolidation() { Id = actionId, Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.AddCorporateAction(stockId, It.IsAny<RestApi.CorporateActions.SplitConsolidation>())).Returns(ServiceResult.Ok()).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.AddCorporateAction(stockId, action);

            result.Should().BeOkResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void AddTransformation()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new RestApi.CorporateActions.Transformation() { Id = actionId, Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.AddCorporateAction(stockId, It.IsAny<RestApi.CorporateActions.Transformation>())).Returns(ServiceResult.Ok()).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.AddCorporateAction(stockId, action);

            result.Should().BeOkResult();

            mockRepository.VerifyAll();
        }


        [Fact]
        public void UpdateCorporateActionStockNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new RestApi.CorporateActions.Dividend() { Id = actionId, Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.UpdateCorporateAction(stockId, action)).Returns(ServiceResult.NotFound()).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.UpdateCorporateAction(stockId, actionId, action);

            result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void UpdateCorporateActionNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new RestApi.CorporateActions.CapitalReturn() { Id = actionId, Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.UpdateCorporateAction(stockId, action)).Returns(ServiceResult.NotFound()).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.UpdateCorporateAction(stockId, actionId, action);

            result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void UpdateCorporateActionNullObject()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();

            var service = mockRepository.Create<ICorporateActionService>();

            var controller = new CorporateActionController(service.Object);
            var result = controller.UpdateCorporateAction(stockId, actionId, null);

            result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void UpdateCorporateActionIdWrong()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new RestApi.CorporateActions.CapitalReturn() { Id = Guid.NewGuid(), Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();

            var controller = new CorporateActionController(service.Object);
            var result = controller.UpdateCorporateAction(stockId, actionId, action);

            result.Should().BeBadRequestResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void UpdateCorporateActionStockWrong()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new RestApi.CorporateActions.CapitalReturn() { Id = actionId, Stock = Guid.NewGuid() };

            var service = mockRepository.Create<ICorporateActionService>();

            var controller = new CorporateActionController(service.Object);
            var result = controller.UpdateCorporateAction(stockId, actionId, action);

            result.Should().BeBadRequestResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void UpdateCorporateAction()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var action = new RestApi.CorporateActions.CapitalReturn() { Id = actionId, Stock = stockId };

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.UpdateCorporateAction(stockId, action)).Returns(ServiceResult.Ok()).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.UpdateCorporateAction(stockId, actionId, action);

            result.Should().BeOkResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void DeleteCorporateActionStockNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.DeleteCorporateAction(stockId, actionId)).Returns(ServiceResult.NotFound()).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.DeleteCorporateAction(stockId, actionId);

            result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void DeleteCorporateActionNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.DeleteCorporateAction(stockId, actionId)).Returns(ServiceResult.NotFound()).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.DeleteCorporateAction(stockId, actionId);

            result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void DeleteCorporateAction()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var actionId = Guid.NewGuid();

            var service = mockRepository.Create<ICorporateActionService>();
            service.Setup(x => x.DeleteCorporateAction(stockId, actionId)).Returns(ServiceResult.Ok()).Verifiable();

            var controller = new CorporateActionController(service.Object);
            var result = controller.DeleteCorporateAction(stockId, actionId);

            result.Should().BeOkResult();

            mockRepository.VerifyAll();
        }
    }

}
