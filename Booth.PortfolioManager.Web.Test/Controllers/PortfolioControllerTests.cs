using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using Moq;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Web.Controllers;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Models.Portfolio;
using Booth.PortfolioManager.Web.Models.Transaction;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Booth.PortfolioManager.Web.Test.Controllers
{
    public class PortfolioControllerTests
    {
        [Fact]
        public async Task CreatePortfolio()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var portfolioId = Guid.NewGuid();
            var name = "My Portfolio";
            var ownerId = Guid.NewGuid();

            var service = mockRepository.Create<IPortfolioService>();
            service.Setup(x => x.CreatePortfolioAsync(portfolioId, name, ownerId)).Returns(Task.FromResult(ServiceResult.Ok())).Verifiable();


            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, ownerId.ToString()));

            var controller = new PortfolioCreateController();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

            var command = new CreatePortfolioCommand()
            {
                Id = portfolioId,
                Name = name
            };
            var result = await controller.CreatePortfolio(service.Object, command);

            result.Should().BeOkResult();

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetProperties()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new PortfolioPropertiesResponse();

            var service = mockRepository.Create<IPortfolioPropertiesService>();
            service.Setup(x => x.GetProperties()).Returns(ServiceResult<PortfolioPropertiesResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetProperties(service.Object);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetSummaryNoDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new PortfolioSummaryResponse();

            var service = mockRepository.Create<IPortfolioSummaryService>();
            service.Setup(x => x.GetSummary(Date.Today)).Returns(ServiceResult<PortfolioSummaryResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetSummary(service.Object, null);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetSummaryForDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new PortfolioSummaryResponse();

            var service = mockRepository.Create<IPortfolioSummaryService>();
            service.Setup(x => x.GetSummary(new Date(2000, 01, 01))).Returns(ServiceResult<PortfolioSummaryResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetSummary(service.Object, new DateTime(2000, 01, 01));

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetPerformanceNoDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new PortfolioPerformanceResponse();

            var service = mockRepository.Create<IPortfolioPerformanceService>();
            service.Setup(x => x.GetPerformance(new DateRange(Date.Today.AddYears(-1).AddDays(1), Date.Today))).Returns(ServiceResult<PortfolioPerformanceResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetPerformance(service.Object, null, null);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetPerformanceStartDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new PortfolioPerformanceResponse();

            var service = mockRepository.Create<IPortfolioPerformanceService>();
            service.Setup(x => x.GetPerformance(new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<PortfolioPerformanceResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetPerformance(service.Object, new DateTime(2000, 01, 01), null);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetPerformanceEndDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new PortfolioPerformanceResponse();

            var service = mockRepository.Create<IPortfolioPerformanceService>();
            service.Setup(x => x.GetPerformance(new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<PortfolioPerformanceResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetPerformance(service.Object, null, new DateTime(2000, 12, 31));

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetPerformanceBothDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new PortfolioPerformanceResponse();

            var service = mockRepository.Create<IPortfolioPerformanceService>();
            service.Setup(x => x.GetPerformance(new DateRange(new Date(2000, 06, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<PortfolioPerformanceResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetPerformance(service.Object, new DateTime(2000, 06, 01), new DateTime(2000, 12, 31));

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetValueNoDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new PortfolioValueResponse();

            var service = mockRepository.Create<IPortfolioValueService>();
            service.Setup(x => x.GetValueAsync(new DateRange(Date.Today.AddYears(-1).AddDays(1), Date.Today), It.IsAny<ValueFrequency>())).Returns(Task.FromResult(ServiceResult<PortfolioValueResponse>.Ok(response))).Verifiable();

            var controller = new PortfolioController();
            var result = await controller.GetValue(service.Object, null, null, null);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetValueStartDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new PortfolioValueResponse();

            var service = mockRepository.Create<IPortfolioValueService>();
            service.Setup(x => x.GetValueAsync(new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)), It.IsAny<ValueFrequency>())).Returns(Task.FromResult(ServiceResult<PortfolioValueResponse>.Ok(response))).Verifiable();

            var controller = new PortfolioController();
            var result = await controller.GetValue(service.Object, new DateTime(2000, 01, 01), null, null);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetValueEndDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new PortfolioValueResponse();

            var service = mockRepository.Create<IPortfolioValueService>();
            service.Setup(x => x.GetValueAsync(new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)), It.IsAny<ValueFrequency>())).Returns(Task.FromResult(ServiceResult<PortfolioValueResponse>.Ok(response))).Verifiable();

            var controller = new PortfolioController();
            var result = await controller.GetValue(service.Object, null, new DateTime(2000, 12, 31), null);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetValueBothDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new PortfolioValueResponse();

            var service = mockRepository.Create<IPortfolioValueService>();
            service.Setup(x => x.GetValueAsync(new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)), It.IsAny<ValueFrequency>())).Returns(Task.FromResult(ServiceResult<PortfolioValueResponse>.Ok(response))).Verifiable();

            var controller = new PortfolioController();
            var result = await controller.GetValue(service.Object, new DateTime(2000, 01, 01), new DateTime(2000, 12, 31), null);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetValueDaily()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new PortfolioValueResponse();

            var service = mockRepository.Create<IPortfolioValueService>();
            service.Setup(x => x.GetValueAsync(It.IsAny<DateRange>(), ValueFrequency.Day)).Returns(Task.FromResult(ServiceResult<PortfolioValueResponse>.Ok(response))).Verifiable();

            var controller = new PortfolioController();
            var result = await controller.GetValue(service.Object, null, null, ValueFrequency.Day);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetValueWeekly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new PortfolioValueResponse();

            var service = mockRepository.Create<IPortfolioValueService>();
            service.Setup(x => x.GetValueAsync(It.IsAny<DateRange>(), ValueFrequency.Week)).Returns(Task.FromResult(ServiceResult<PortfolioValueResponse>.Ok(response))).Verifiable();

            var controller = new PortfolioController();
            var result = await controller.GetValue(service.Object, null, null, ValueFrequency.Week);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetValueMonthly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new PortfolioValueResponse();

            var service = mockRepository.Create<IPortfolioValueService>();
            service.Setup(x => x.GetValueAsync(It.IsAny<DateRange>(), ValueFrequency.Month)).Returns(Task.FromResult(ServiceResult<PortfolioValueResponse>.Ok(response))).Verifiable();

            var controller = new PortfolioController();
            var result = await controller.GetValue(service.Object, null, null, ValueFrequency.Month);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }


        [Fact]
        public void GetTransactionsNoDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new TransactionsResponse();

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.GetTransactions(new DateRange(Date.Today.AddYears(-1).AddDays(1), Date.Today))).Returns(ServiceResult<TransactionsResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetTransactions(service.Object, null, null);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetTransactionsStartDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new TransactionsResponse();

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.GetTransactions(new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<TransactionsResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetTransactions(service.Object, new DateTime(2000, 01, 01), null);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetTransactionsEndDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new TransactionsResponse();

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.GetTransactions(new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<TransactionsResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetTransactions(service.Object, null, new DateTime(2000, 12, 31));

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetTransactionsBothDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new TransactionsResponse();

            var service = mockRepository.Create<IPortfolioTransactionService>();
            service.Setup(x => x.GetTransactions(new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<TransactionsResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetTransactions(service.Object, new DateTime(2000, 01, 01), new DateTime(2000, 12, 31));

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCapitalGainsNoDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new SimpleUnrealisedGainsResponse();

            var service = mockRepository.Create<IPortfolioCapitalGainsService>();
            service.Setup(x => x.GetCapitalGains(Date.Today)).Returns(ServiceResult<SimpleUnrealisedGainsResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetCapitalGains(service.Object, null);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCapitalGainsForDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new SimpleUnrealisedGainsResponse();

            var service = mockRepository.Create<IPortfolioCapitalGainsService>();
            service.Setup(x => x.GetCapitalGains(new Date(2000, 01, 01))).Returns(ServiceResult<SimpleUnrealisedGainsResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetCapitalGains(service.Object, new DateTime(2000, 01, 01));

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetDetailedCapitalGainsNoDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new DetailedUnrealisedGainsResponse();

            var service = mockRepository.Create<IPortfolioCapitalGainsService>();
            service.Setup(x => x.GetDetailedCapitalGains(Date.Today)).Returns(ServiceResult<DetailedUnrealisedGainsResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetDetailedCapitalGains(service.Object, null);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetDetailedCapitalGainsForDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new DetailedUnrealisedGainsResponse();

            var service = mockRepository.Create<IPortfolioCapitalGainsService>();
            service.Setup(x => x.GetDetailedCapitalGains(new Date(2000, 01, 01))).Returns(ServiceResult<DetailedUnrealisedGainsResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetDetailedCapitalGains(service.Object, new DateTime(2000, 01, 01));

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCGTLiabilityNoDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new CgtLiabilityResponse();

            var service = mockRepository.Create<IPortfolioCgtLiabilityService>();
            service.Setup(x => x.GetCGTLiability(new DateRange(Date.Today.AddYears(-1).AddDays(1), Date.Today))).Returns(ServiceResult<CgtLiabilityResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetCGTLiability(service.Object, null, null);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCGTLiabilityStartDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new CgtLiabilityResponse();

            var service = mockRepository.Create<IPortfolioCgtLiabilityService>();
            service.Setup(x => x.GetCGTLiability(new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<CgtLiabilityResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetCGTLiability(service.Object, new DateTime(2000, 01, 01), null);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCGTLiabilityEndDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new CgtLiabilityResponse();

            var service = mockRepository.Create<IPortfolioCgtLiabilityService>();
            service.Setup(x => x.GetCGTLiability(new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<CgtLiabilityResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetCGTLiability(service.Object, null, new DateTime(2000, 12, 31));

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCGTLiabilityBothDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new CgtLiabilityResponse();

            var service = mockRepository.Create<IPortfolioCgtLiabilityService>();
            service.Setup(x => x.GetCGTLiability(new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<CgtLiabilityResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetCGTLiability(service.Object, new DateTime(2000, 01, 01), new DateTime(2000, 12, 31));

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCashAccountNoDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new CashAccountTransactionsResponse();

            var service = mockRepository.Create<ICashAccountService>();
            service.Setup(x => x.GetTransactions(new DateRange(Date.Today.AddYears(-1).AddDays(1), Date.Today))).Returns(ServiceResult<CashAccountTransactionsResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetCashAccount(service.Object, null, null);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCashAccountStartDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new CashAccountTransactionsResponse();

            var service = mockRepository.Create<ICashAccountService>();
            service.Setup(x => x.GetTransactions(new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<CashAccountTransactionsResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetCashAccount(service.Object, new DateTime(2000, 01, 01), null);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCashAccountEndDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new CashAccountTransactionsResponse();

            var service = mockRepository.Create<ICashAccountService>();
            service.Setup(x => x.GetTransactions(new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<CashAccountTransactionsResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetCashAccount(service.Object, null, new DateTime(2000, 12, 31));

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCashAccountBothDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new CashAccountTransactionsResponse();

            var service = mockRepository.Create<ICashAccountService>();
            service.Setup(x => x.GetTransactions(new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<CashAccountTransactionsResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetCashAccount(service.Object, new DateTime(2000, 01, 01), new DateTime(2000, 12, 31));

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetIncomeNoDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new IncomeResponse();

            var service = mockRepository.Create<IPortfolioIncomeService>();
            service.Setup(x => x.GetIncome(new DateRange(Date.Today.AddYears(-1).AddDays(1), Date.Today))).Returns(ServiceResult<IncomeResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetIncome(service.Object, null, null);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetIncomeStartDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new IncomeResponse();

            var service = mockRepository.Create<IPortfolioIncomeService>();
            service.Setup(x => x.GetIncome(new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<IncomeResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetIncome(service.Object, new DateTime(2000, 01, 01), null);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetIncomeEndDateOnly()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new IncomeResponse();

            var service = mockRepository.Create<IPortfolioIncomeService>();
            service.Setup(x => x.GetIncome(new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<IncomeResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetIncome(service.Object, null, new DateTime(2000, 12, 31));

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetIncomeBothDates()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new IncomeResponse();

            var service = mockRepository.Create<IPortfolioIncomeService>();
            service.Setup(x => x.GetIncome(new DateRange(new Date(2000, 01, 01), new Date(2000, 12, 31)))).Returns(ServiceResult<IncomeResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetIncome(service.Object, new DateTime(2000, 01, 01), new DateTime(2000, 12, 31));

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

        [Fact]
        public void GetCorporateActions()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var response = new CorporateActionsResponse();

            var service = mockRepository.Create<IPortfolioCorporateActionsService>();
            service.Setup(x => x.GetCorporateActions()).Returns(ServiceResult<CorporateActionsResponse>.Ok(response)).Verifiable();

            var controller = new PortfolioController();
            var result = controller.GetCorporateActions(service.Object);

            result.Result.Should().BeOkObjectResult().Which.Value.Should().Be(response);

            mockRepository.VerifyAll();
        }

       

    }
}
