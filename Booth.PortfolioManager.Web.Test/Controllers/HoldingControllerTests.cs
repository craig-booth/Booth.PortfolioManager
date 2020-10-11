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
using Booth.PortfolioManager.RestApi.Portfolios;

namespace Booth.PortfolioManager.Web.Test.Controllers
{
    public class HoldingControllerTests
    {

        [Fact]
        public void GetHoldingsNoDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new List<Holding>();

            var service = mockRepository.Create<IPortfolioService>();
            service.Setup(x => x.GetHoldings(Date.Today)).Returns(ServiceResult<List<Holding>>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.Get(service.Object, null, null, null);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetHoldingsAtDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new List<Holding>();

            var service = mockRepository.Create<IPortfolioService>();
            service.Setup(x => x.GetHoldings(new Date(2000, 01, 01))).Returns(ServiceResult<List<Holding>>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.Get(service.Object, new DateTime(2000, 01, 01), null, null);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetHoldingsStartDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new List<Holding>();

            var service = mockRepository.Create<IPortfolioService>();
            service.Setup(x => x.GetHoldings(new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<List<Holding>>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.Get(service.Object, null, new DateTime(2000, 01, 01), null);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetHoldingsEndDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new List<Holding>();

            var service = mockRepository.Create<IPortfolioService>();
            service.Setup(x => x.GetHoldings(new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<List<Holding>>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.Get(service.Object, null, null, new DateTime(2000, 12, 31));

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetHoldingsBothDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new List<Holding>();

            var service = mockRepository.Create<IPortfolioService>();
            service.Setup(x => x.GetHoldings(new DateRange(new Date(2000, 06, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<List<Holding>>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.Get(service.Object, null, new DateTime(2000, 06, 01), new DateTime(2000, 12, 31));

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetHoldingsAllDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new List<Holding>();

            var service = mockRepository.Create<IPortfolioService>();
            service.Setup(x => x.GetHoldings(new Date(1999, 01, 01))).Returns(ServiceResult<List<Holding>>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.Get(service.Object, new DateTime(1999, 01, 01), new DateTime(2000, 01, 01), new DateTime(2000, 12, 31));

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetHoldingNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();

            var service = mockRepository.Create<IPortfolioService>();
            service.Setup(x => x.GetHolding(stockId, new Date(2000, 01, 01))).Returns(ServiceResult<Holding>.NotFound()).Verifiable();

            var controller = new HoldingController();
            var result = controller.Get(service.Object, stockId, new DateTime(2000, 01, 01));

            result.Result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetHoldingNoDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new Holding();

            var service = mockRepository.Create<IPortfolioService>();
            service.Setup(x => x.GetHolding(stockId, Date.Today)).Returns(ServiceResult<Holding>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.Get(service.Object, stockId, null);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetHoldingForDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new Holding();

            var service = mockRepository.Create<IPortfolioService>();
            service.Setup(x => x.GetHolding(stockId, new Date(2000, 01, 01))).Returns(ServiceResult<Holding>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.Get(service.Object, stockId, new DateTime(2000, 01, 01));

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void ChangeDrpParticipationNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();

            var service = mockRepository.Create<IPortfolioService>();
            service.Setup(x => x.ChangeDrpParticipation(stockId, true)).Returns(ServiceResult<PortfolioSummaryResponse>.NotFound()).Verifiable();

            var controller = new HoldingController();
            var result = controller.ChangeDrpParticipation(service.Object, stockId, true);

            result.Should().BeNotFoundResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void ChangeDrpParticipation()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();

            var service = mockRepository.Create<IPortfolioService>();
            service.Setup(x => x.ChangeDrpParticipation(stockId, true)).Returns(ServiceResult<PortfolioSummaryResponse>.Ok()).Verifiable();

            var controller = new HoldingController();
            var result = controller.ChangeDrpParticipation(service.Object, stockId, true);

            result.Should().BeOkResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetValueNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new PortfolioValueResponse();

            var service = mockRepository.Create<IPortfolioValueService>();
            service.Setup(x => x.GetValue(stockId, new DateRange(Date.Today.AddYears(-1).AddDays(1), Date.Today), It.IsAny<ValueFrequency>())).Returns(ServiceResult<PortfolioValueResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetValue(service.Object, stockId, null, null, null);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetValueNoDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new PortfolioValueResponse();

            var service = mockRepository.Create<IPortfolioValueService>();
            service.Setup(x => x.GetValue(stockId, new DateRange(Date.Today.AddYears(-1).AddDays(1), Date.Today), It.IsAny<ValueFrequency>())).Returns(ServiceResult<PortfolioValueResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetValue(service.Object, stockId, null, null, null);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetValueStartDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new PortfolioValueResponse();

            var service = mockRepository.Create<IPortfolioValueService>();
            service.Setup(x => x.GetValue(stockId, new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)), It.IsAny<ValueFrequency>())).Returns(ServiceResult<PortfolioValueResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetValue(service.Object, stockId, new DateTime(2000, 01, 01), null, null);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetValueEndDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new PortfolioValueResponse();

            var service = mockRepository.Create<IPortfolioValueService>();
            service.Setup(x => x.GetValue(stockId, new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)), It.IsAny<ValueFrequency>())).Returns(ServiceResult<PortfolioValueResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetValue(service.Object, stockId, null, new DateTime(2000, 12, 31), null);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetValueBothDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new PortfolioValueResponse();

            var service = mockRepository.Create<IPortfolioValueService>();
            service.Setup(x => x.GetValue(stockId, new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)), It.IsAny<ValueFrequency>())).Returns(ServiceResult<PortfolioValueResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetValue(service.Object, stockId, new DateTime(2000, 01, 01), new DateTime(2000, 12, 31), null);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetValueDaily()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new PortfolioValueResponse();

            var service = mockRepository.Create<IPortfolioValueService>();
            service.Setup(x => x.GetValue(stockId, It.IsAny<DateRange>(), ValueFrequency.Day)).Returns(ServiceResult<PortfolioValueResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetValue(service.Object, stockId, null, null, ValueFrequency.Day);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetValueWeekly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new PortfolioValueResponse();

            var service = mockRepository.Create<IPortfolioValueService>();
            service.Setup(x => x.GetValue(stockId, It.IsAny<DateRange>(), ValueFrequency.Week)).Returns(ServiceResult<PortfolioValueResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetValue(service.Object, stockId, null, null, ValueFrequency.Week);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetValueMonthly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new PortfolioValueResponse();

            var service = mockRepository.Create<IPortfolioValueService>();
            service.Setup(x => x.GetValue(stockId, It.IsAny<DateRange>(), ValueFrequency.Month)).Returns(ServiceResult<PortfolioValueResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetValue(service.Object, stockId, null, null, ValueFrequency.Month);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetTransactionsNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new TransactionsResponse();

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.GetTransactions(stockId, new DateRange(Date.Today.AddYears(-1).AddDays(1), Date.Today))).Returns(ServiceResult<TransactionsResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetTransactions(service.Object, stockId, null, null);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetTransactionsNoDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new TransactionsResponse();

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.GetTransactions(stockId, new DateRange(Date.Today.AddYears(-1).AddDays(1), Date.Today))).Returns(ServiceResult<TransactionsResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetTransactions(service.Object, stockId, null, null);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetTransactionsStartDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new TransactionsResponse();

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.GetTransactions(stockId, new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<TransactionsResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetTransactions(service.Object, stockId, new DateTime(2000, 01, 01), null);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetTransactionsEndDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new TransactionsResponse();

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.GetTransactions(stockId, new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<TransactionsResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetTransactions(service.Object, stockId, null, new DateTime(2000, 12, 31));

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetTransactionsBothDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new TransactionsResponse();

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.GetTransactions(stockId, new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<TransactionsResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetTransactions(service.Object, stockId, new DateTime(2000, 01, 01), new DateTime(2000, 12, 31));

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCapitalGainsNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new SimpleUnrealisedGainsResponse();

            var service = mockRepository.Create<IPortfolioCapitalGainsService>();
            service.Setup(x => x.GetCapitalGains(stockId, Date.Today)).Returns(ServiceResult<SimpleUnrealisedGainsResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetCapitalGains(service.Object, stockId, null);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCapitalGainsNoDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new SimpleUnrealisedGainsResponse();

            var service = mockRepository.Create<IPortfolioCapitalGainsService>();
            service.Setup(x => x.GetCapitalGains(stockId, Date.Today)).Returns(ServiceResult<SimpleUnrealisedGainsResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetCapitalGains(service.Object, stockId, null);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCapitalGainsForDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new SimpleUnrealisedGainsResponse();

            var service = mockRepository.Create<IPortfolioCapitalGainsService>();
            service.Setup(x => x.GetCapitalGains(stockId, new Date(2000, 01, 01))).Returns(ServiceResult<SimpleUnrealisedGainsResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetCapitalGains(service.Object, stockId, new DateTime(2000, 01, 01));

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetDetailedCapitalGainsNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new DetailedUnrealisedGainsResponse();

            var service = mockRepository.Create<IPortfolioCapitalGainsService>();
            service.Setup(x => x.GetDetailedCapitalGains(stockId, Date.Today)).Returns(ServiceResult<DetailedUnrealisedGainsResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetDetailedCapitalGains(service.Object, stockId, null);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetDetailedCapitalGainsNoDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new DetailedUnrealisedGainsResponse();

            var service = mockRepository.Create<IPortfolioCapitalGainsService>();
            service.Setup(x => x.GetDetailedCapitalGains(stockId, Date.Today)).Returns(ServiceResult<DetailedUnrealisedGainsResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetDetailedCapitalGains(service.Object, stockId, null);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetDetailedCapitalGainsForDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new DetailedUnrealisedGainsResponse();

            var service = mockRepository.Create<IPortfolioCapitalGainsService>();
            service.Setup(x => x.GetDetailedCapitalGains(stockId, new Date(2000, 01, 01))).Returns(ServiceResult<DetailedUnrealisedGainsResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetDetailedCapitalGains(service.Object, stockId, new DateTime(2000, 01, 01));

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCorporateActionsNotFound()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new CorporateActionsResponse();

            var service = mockRepository.Create<IPortfolioCorporateActionsService>();
            service.Setup(x => x.GetCorporateActions(stockId)).Returns(ServiceResult<CorporateActionsResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetCorporateActions(service.Object, stockId);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCorporateActions()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockId = Guid.NewGuid();
            var response = new CorporateActionsResponse();

            var service = mockRepository.Create<IPortfolioCorporateActionsService>();
            service.Setup(x => x.GetCorporateActions(stockId)).Returns(ServiceResult<CorporateActionsResponse>.Ok(response)).Verifiable();

            var controller = new HoldingController();
            var result = controller.GetCorporateActions(service.Object, stockId);

            result.Result.Should().BeOkObjectResult().Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

       

    }
}
