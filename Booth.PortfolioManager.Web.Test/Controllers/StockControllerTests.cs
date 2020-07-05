using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

using Xunit;
using Moq;
using FluentAssertions;
using FluentAssertions.AspNetCore.Mvc;

using Booth.Common;
using Booth.PortfolioManager.Web.Controllers;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Mappers;
using Booth.PortfolioManager.Web.Utilities;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.RestApi.Stocks;

namespace Booth.PortfolioManager.Web.Test.Controllers
{
    public class StockControllerTests
    {
        private Guid _MissingStockId;
        private Guid _ValidationErrorId;
        private Stock _Stock;

        private Mock<IStockPriceHistory> _StockPriceHistory;
        private Mock<IStockQuery> _StockQueryMock;
        private Mock<IStockService> _StockServiceMock;

        private StockController _Controller;


        public StockControllerTests()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            _MissingStockId = Guid.NewGuid();
            _ValidationErrorId = Guid.NewGuid();

            _Stock = new Stock(Guid.NewGuid());
            _Stock.List("ABC", "ABC Pty Ltd", new Date(2000, 01, 01), false, Domain.Stocks.AssetCategory.AustralianStocks);
            _Stock.ChangeProperties(new Date(2010, 01, 01), "XYZ", "XYZ Pty Ltd", Domain.Stocks.AssetCategory.AustralianStocks);

            _StockPriceHistory = mockRepository.Create<IStockPriceHistory>();
            _StockPriceHistory.Setup(x => x.GetPrice(Date.Today)).Returns(12.25m);
            _StockPriceHistory.Setup(x => x.GetPrices(It.IsAny<DateRange>())).Returns(new[] { new StockPrice(new Date(2010, 01, 01), 0.10m) } );

            _Stock.SetPriceHistory(_StockPriceHistory.Object);

            _StockQueryMock = mockRepository.Create<IStockQuery>();
            _StockQueryMock.Setup(x => x.Get(_MissingStockId)).Returns(default(Stock));
            _StockQueryMock.Setup(x => x.Get(_Stock.Id)).Returns(_Stock);

            _StockQueryMock.Setup(x => x.All()).Returns(new[] { _Stock });
            _StockQueryMock.Setup(x => x.All(It.IsAny<Date>())).Returns(new[] { _Stock });

            _StockQueryMock.Setup(x => x.All()).Returns(new[] { _Stock });
            _StockQueryMock.Setup(x => x.All(It.IsAny<Date>())).Returns(new[] { _Stock });
            _StockQueryMock.Setup(x => x.All(It.IsAny<DateRange>())).Returns(new[] { _Stock });

            _StockQueryMock.Setup(x => x.Find(It.IsAny<Func<StockProperties, bool>>())).Returns(new[] { _Stock });
            _StockQueryMock.Setup(x => x.Find(It.IsAny<Date>(), It.IsAny<Func<StockProperties, bool>>())).Returns(new[] { _Stock });
            _StockQueryMock.Setup(x => x.Find(It.IsAny<DateRange>(), It.IsAny<Func<StockProperties, bool>>())).Returns(new[] { _Stock });

            _StockServiceMock = mockRepository.Create<IStockService>();
            _StockServiceMock.Setup(x => x.ChangeDividendRules(_MissingStockId, It.IsAny<Date>(), It.IsAny<decimal>(), It.IsAny<RoundingRule>(), It.IsAny<bool>(), It.IsAny<Domain.Stocks.DrpMethod>())).Returns(ServiceResult.NotFound());
            _StockServiceMock.Setup(x => x.ChangeDividendRules(_ValidationErrorId, It.IsAny<Date>(), It.IsAny<decimal>(), It.IsAny<RoundingRule>(), It.IsAny<bool>(), It.IsAny<Domain.Stocks.DrpMethod>())).Returns(ServiceResult.Error("Dummy Error"));
            _StockServiceMock.Setup(x => x.ChangeDividendRules(_Stock.Id, It.IsAny<Date>(), It.IsAny<decimal>(), It.IsAny<RoundingRule>(), It.IsAny<bool>(), It.IsAny<Domain.Stocks.DrpMethod>())).Returns(ServiceResult.Ok());

            _StockServiceMock.Setup(x => x.ListStock(_MissingStockId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Date>(), It.IsAny<bool>(), It.IsAny<Domain.Stocks.AssetCategory>())).Returns(ServiceResult.NotFound());
            _StockServiceMock.Setup(x => x.ListStock(_ValidationErrorId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Date>(), It.IsAny<bool>(), It.IsAny<Domain.Stocks.AssetCategory>())).Returns(ServiceResult.Error("Dummy Error"));
            _StockServiceMock.Setup(x => x.ListStock(_Stock.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Date>(), It.IsAny<bool>(), It.IsAny<Domain.Stocks.AssetCategory>())).Returns(ServiceResult.Ok());

            _StockServiceMock.Setup(x => x.ChangeStock(_MissingStockId, It.IsAny<Date>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Domain.Stocks.AssetCategory>())).Returns(ServiceResult.NotFound());
            _StockServiceMock.Setup(x => x.ChangeStock(_ValidationErrorId, It.IsAny<Date>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Domain.Stocks.AssetCategory>())).Returns(ServiceResult.Error("Dummy Error"));
            _StockServiceMock.Setup(x => x.ChangeStock(_Stock.Id, It.IsAny<Date>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Domain.Stocks.AssetCategory>())).Returns(ServiceResult.Ok());

            _StockServiceMock.Setup(x => x.DelistStock(_MissingStockId, It.IsAny<Date>())).Returns(ServiceResult.NotFound());
            _StockServiceMock.Setup(x => x.DelistStock(_ValidationErrorId, It.IsAny<Date>())).Returns(ServiceResult.Error("Dummy Error"));
            _StockServiceMock.Setup(x => x.DelistStock(_Stock.Id, It.IsAny<Date>())).Returns(ServiceResult.Ok());
            
            _StockServiceMock.Setup(x => x.UpdateClosingPrices(_MissingStockId, It.IsAny<IEnumerable<StockPrice>>())).Returns(ServiceResult.NotFound());
            _StockServiceMock.Setup(x => x.UpdateClosingPrices(_ValidationErrorId, It.IsAny<IEnumerable<StockPrice>>())).Returns(ServiceResult.Error("Dummy Error"));
            _StockServiceMock.Setup(x => x.UpdateClosingPrices(_Stock.Id, It.IsAny<IEnumerable<StockPrice>>())).Returns(ServiceResult.Ok());

            _StockServiceMock.Setup(x => x.UpdateCurrentPrice(_MissingStockId, It.IsAny<decimal>())).Returns(ServiceResult.NotFound());
            _StockServiceMock.Setup(x => x.UpdateCurrentPrice(_ValidationErrorId, It.IsAny<decimal>())).Returns(ServiceResult.Error("Dummy Error"));
            _StockServiceMock.Setup(x => x.UpdateCurrentPrice(_Stock.Id, It.IsAny<decimal>())).Returns(ServiceResult.Ok());

            _Controller = new StockController(_StockServiceMock.Object, _StockQueryMock.Object);
        }


        [Fact]
        public void GetByIdNotfound()
        {
            var response = _Controller.Get(_MissingStockId, null);

            response.Result.Should().BeNotFoundResult();
        }

        [Fact]
        public void GetByIdWithoutDate()
        {
            var response = _Controller.Get(_Stock.Id, null);

            response.Result.Should().BeOkObjectResult().Value.Should().BeEquivalentTo(_Stock.ToResponse(Date.Today));
        }

        [Fact]
        public void GetByIdDate()
        {
            var response = _Controller.Get(_Stock.Id, new DateTime(2005, 01, 01));

            response.Result.Should().BeOkObjectResult().Value.Should().BeEquivalentTo(_Stock.ToResponse(new Date(2005, 01, 01)));
        }


        [Fact]
        public void GetWithQueryAndNoDates()
        {
            var response = _Controller.Get("XYZ", null, null, null);

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(_Stock.ToResponse(Date.Today));

            _StockQueryMock.Verify(x => x.Find(It.IsAny<Func<StockProperties, bool>>())); 
        }

        [Fact]
        public void GetWithQueryAndDate()
        {
           var response = _Controller.Get("XYZ", new DateTime(2011, 01, 01), null, null);

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(_Stock.ToResponse(new Date(2011, 01, 01)));

            _StockQueryMock.Verify(x => x.Find(new Date(2011, 01, 01), It.IsAny<Func<StockProperties, bool>>())); 
        }

        [Fact]
        public void GetWithQueryAndFromDate()
        {
            var response = _Controller.Get("XYZ", null, new DateTime(2009, 01, 01), null);

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(_Stock.ToResponse(Date.MaxValue));

            _StockQueryMock.Verify(x => x.Find(new DateRange(new Date(2009, 01, 01), Date.MaxValue), It.IsAny<Func<StockProperties, bool>>())); 
        }

        [Fact]
        public void GetWithQueryAndToDate()
        {
            var response = _Controller.Get("XYZ", null, null, new DateTime(2055, 01, 01));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(_Stock.ToResponse(new Date(2055, 01, 01)));

            _StockQueryMock.Verify(x => x.Find(new DateRange(Date.MinValue, new Date(2055, 01, 01)), It.IsAny<Func<StockProperties, bool>>())); 
        }

        [Fact]
        public void GetWithQueryAndFromAndToDate()
        {
            var response = _Controller.Get("XYZ", null, new DateTime(2009, 01, 01), new DateTime(2012, 01, 01));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(_Stock.ToResponse(new Date(2012, 01, 01)));

            _StockQueryMock.Verify(x => x.Find(new DateRange(new Date(2009, 01, 01), new Date(2012, 01, 01)), It.IsAny<Func<StockProperties, bool>>()));
        }

        [Fact]
        public void GetWithQueryAndAllDates()
        {
            var response = _Controller.Get("XYZ", new DateTime(2009, 01, 01), new DateTime(2009, 01, 01), new DateTime(2012, 01, 01));

            response.Result.Should().BeBadRequestObjectResult();
        }

        [Fact]
        public void GetWithoutQueryAndNoDates()
        {
            var response = _Controller.Get(null, null, null, null);

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(_Stock.ToResponse(Date.Today));

            _StockQueryMock.Verify(x => x.All()); 
        }

        [Fact]
        public void GetWithoutQueryAndDate()
        {
            var response = _Controller.Get(null, new DateTime(2011, 01, 01), null, null);

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(_Stock.ToResponse(new Date(2011, 01, 01)));

            _StockQueryMock.Verify(x => x.All(new Date(2011, 01, 01))); 
        }

        [Fact]
        public void GetWithoutQueryAndFromDate()
        {
            var response = _Controller.Get(null, null, new DateTime(2009, 01, 01), null);

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(_Stock.ToResponse(Date.MaxValue));

            _StockQueryMock.Verify(x => x.All(new DateRange(new Date(2009, 01, 01), Date.MaxValue))); 

        }

        [Fact]
        public void GetWithoutQueryAndToDate()
        {
            var response = _Controller.Get(null, null, null, new DateTime(2055, 01, 01));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(_Stock.ToResponse(new Date(2055, 01, 01)));

            _StockQueryMock.Verify(x => x.All(new DateRange(Date.MinValue, new Date(2055, 01, 01)))); 

        }

        [Fact]
        public void GetWithoutQueryAndFromAndToDate()
        {
            var response = _Controller.Get(null, null, new DateTime(2009, 01, 01), new DateTime(2012, 01, 01));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(_Stock.ToResponse(new Date(2012, 01, 01)));

            _StockQueryMock.Verify(x => x.All(new DateRange(new Date(2009, 01, 01), new Date(2012, 01, 01)))); 

        }

        [Fact]
        public void GetWithoutQueryAndAllDates()
        {
            var response = _Controller.Get(null, new DateTime(2009, 01, 01), new DateTime(2009, 01, 01), new DateTime(2012, 01, 01));

            response.Result.Should().BeBadRequestObjectResult();
        }

        [Fact]
        public void GetHistoryInvaidId()
        {
            var response = _Controller.GetHistory(_MissingStockId);

            response.Result.Should().BeNotFoundResult();
        }

        [Fact]
        public void GetHistory()
        {
            var response = _Controller.GetHistory(_Stock.Id);

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<StockHistoryResponse>()
                .Which.Should().BeEquivalentTo(_Stock.ToHistoryResponse());

        }

        [Fact]
        public void GetClosingPricesInvalidId()
        {
            var response = _Controller.GetClosingPrices(_MissingStockId, null, null);

            response.Result.Should().BeNotFoundResult();
        }

        [Fact]
        public void GetClosingPricesWithoutDates()
        {
            var response = _Controller.GetClosingPrices(_Stock.Id, null, null);

            var expectedDateRange = new DateRange(Date.Today.AddYears(-1).AddDays(1), Date.Today);

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<StockPriceResponse>()
                .Which.Should().BeEquivalentTo(_Stock.ToPriceResponse(expectedDateRange));

            _StockPriceHistory.Verify(x => x.GetPrices(expectedDateRange));
        }

        [Fact]
        public void GetClosingPricesWithFromDateOnly()
        {
            var response = _Controller.GetClosingPrices(_Stock.Id, new DateTime(2010, 01, 01), null);

            var expectedDateRange = new DateRange(new Date(2010, 01, 01), new Date(2010, 12, 31));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<StockPriceResponse>()
                .Which.Should().BeEquivalentTo(_Stock.ToPriceResponse(expectedDateRange));

            _StockPriceHistory.Verify(x => x.GetPrices(expectedDateRange));
        }

        [Fact]
        public void GetClosingPricesWithToDateOnly()
        {
            var response = _Controller.GetClosingPrices(_Stock.Id, null, new DateTime(2015, 01, 01));

            var expectedDateRange = new DateRange(new Date(2014, 01, 02), new Date(2015, 01, 01));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<StockPriceResponse>()
                .Which.Should().BeEquivalentTo(_Stock.ToPriceResponse(expectedDateRange));

            _StockPriceHistory.Verify(x => x.GetPrices(expectedDateRange));
        }

        [Fact]
        public void GetClosingPricesWithBothDates()
        {
            var response = _Controller.GetClosingPrices(_Stock.Id, new DateTime(2010, 01, 01), new DateTime(2015, 01, 01));

            var expectedDateRange = new DateRange(new Date(2010, 01, 01), new Date(2015, 01, 01));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<StockPriceResponse>()
                .Which.Should().BeEquivalentTo(_Stock.ToPriceResponse(expectedDateRange));

            _StockPriceHistory.Verify(x => x.GetPrices(expectedDateRange));
        }

        [Fact]
        public void CreateStockValidationError()
        {
            var command = new CreateStockCommand()
            {
                Id = _ValidationErrorId,
                ListingDate = new Date(2009, 01, 01),
                AsxCode = "XYZ",
                Name = "XYZ Name",
                Trust = true,
                Category = RestApi.Stocks.AssetCategory.AustralianFixedInterest
            };

            var response = _Controller.CreateStock(command);

            response.Should().BeBadRequestObjectResult();
        }

        [Fact]
        public void CreateStock()
        {
            var command = new CreateStockCommand()
            {
                Id = _Stock.Id,
                ListingDate = new Date(2009, 01, 01),
                AsxCode = "XYZ",
                Name = "XYZ Name",
                Trust = true,
                Category = RestApi.Stocks.AssetCategory.AustralianFixedInterest
            };

            var response = _Controller.CreateStock(command);

            response.Should().BeOkResult();
        }

        [Fact]
        public void ChangeStockInvalidId()
        {
            var command = new ChangeStockCommand()
            {
                Id = _MissingStockId,
                ChangeDate = new Date(2012, 01, 01),
                AsxCode = "XYZ",
                Name = "XYZ Name",
                Category = RestApi.Stocks.AssetCategory.AustralianFixedInterest
            };
            var response = _Controller.ChangeStock(_MissingStockId, command);

            response.Should().BeNotFoundResult();

        }

        [Fact]
        public void ChangeStockValidationError()
        {
            var command = new ChangeStockCommand()
            {
                Id = _ValidationErrorId,
                ChangeDate = new Date(2009, 01, 01),
                AsxCode = "XYZ",
                Name = "XYZ Name",
                Category = RestApi.Stocks.AssetCategory.AustralianFixedInterest
            };
            var response = _Controller.ChangeStock(_Stock.Id, command);

            response.Should().BeBadRequestObjectResult();
        }

        [Fact]
        public void ChangeStock()
        {
            var command = new ChangeStockCommand()
            {
                Id = _Stock.Id,
                ChangeDate = new Date(2009, 01, 01),
                AsxCode = "XYZ",
                Name = "XYZ Name",
                Category = RestApi.Stocks.AssetCategory.AustralianFixedInterest
            };
            var response = _Controller.ChangeStock(_Stock.Id, command);

            response.Should().BeOkResult();

            _StockServiceMock.Verify(x => x.ChangeStock(_Stock.Id, new Date(2009, 01, 01), "XYZ", "XYZ Name", Domain.Stocks.AssetCategory.AustralianFixedInterest));
        }

        [Fact]
        public void DelistStockInvalidId()
        {
            var command = new DelistStockCommand()
            {
                Id = _MissingStockId,
                DelistingDate = new Date(2012, 01, 01)
            };
            var response = _Controller.DelistStock(_MissingStockId, command);

            response.Should().BeNotFoundResult();
        }

        [Fact]
        public void DelistStockValidationError()
        {
            var command = new DelistStockCommand()
            {
                Id = _ValidationErrorId,
                DelistingDate = new Date(2012, 01, 01)
            };
            var response = _Controller.DelistStock(_MissingStockId, command);

            response.Should().BeBadRequestObjectResult();
        }

        [Fact]
        public void DelistStock()
        {
            var command = new DelistStockCommand()
            {
                Id = _Stock.Id,
                DelistingDate = new Date(2012, 01, 01)
            };
            var response = _Controller.DelistStock(_Stock.Id, command);

            response.Should().BeOkResult();
        }


        [Fact]
        public void UpdateClosingPricesInvalidId()
        {
            var command = new UpdateClosingPricesCommand()
            {
                Id = _MissingStockId
            };
            var response = _Controller.UpdateClosingPrices(_MissingStockId, command);

            response.Should().BeNotFoundResult();
        }

        [Fact]
        public void UpdateClosingPricesValidationError()
        {
            var command = new UpdateClosingPricesCommand()
            {
                Id = _ValidationErrorId
            };
            var response = _Controller.UpdateClosingPrices(_ValidationErrorId, command);

            response.Should().BeBadRequestObjectResult();
        }

        [Fact]
        public void UpdateClosingPrices()
        {
            var command = new UpdateClosingPricesCommand()
            {
                Id = _Stock.Id
            };
            var response = _Controller.UpdateClosingPrices(_Stock.Id, command);

            response.Should().BeOkResult();
        }

        [Fact]
        public void ChangeDividendRulesInvalidId()
        { 
            var command = new ChangeDividendRulesCommand()
            {
                Id = _MissingStockId,
                ChangeDate = new Date(2012, 01, 01),
                CompanyTaxRate = 0.40m,
                DividendRoundingRule = RoundingRule.Truncate,
                DrpActive = true,
                DrpMethod = RestApi.Stocks.DrpMethod.RoundUp
            };
            var response = _Controller.ChangeDividendRules(_MissingStockId, command);

            response.Should().BeNotFoundResult();   
        }

        [Fact]
        public void ChangeDividendRulesValidationError()
        {
            var command = new ChangeDividendRulesCommand()
            {
                Id = _ValidationErrorId,
                ChangeDate = new Date(2009, 01, 01),
                CompanyTaxRate = 10.45m,
                DividendRoundingRule = RoundingRule.Truncate,
                DrpActive = true,
                DrpMethod = RestApi.Stocks.DrpMethod.RoundUp
            };
            var response = _Controller.ChangeDividendRules(_Stock.Id, command);

            response.Should().BeBadRequestObjectResult();
        }

        [Fact]
        public void ChangeDividendRules()
        {
            var command = new ChangeDividendRulesCommand()
            {
                Id = _Stock.Id,
                ChangeDate = new Date(2009, 01, 01),
                CompanyTaxRate = 0.40m,
                DividendRoundingRule = RoundingRule.Truncate,
                DrpActive = true,
                DrpMethod = RestApi.Stocks.DrpMethod.RoundUp
            };
            var response = _Controller.ChangeDividendRules(_Stock.Id, command);

            response.Should().BeOkResult();

            _StockServiceMock.Verify(x => x.ChangeDividendRules(_Stock.Id, new Date(2009, 01, 01), 0.40m, RoundingRule.Truncate, true, Domain.Stocks.DrpMethod.RoundUp));
        }

    }
}
