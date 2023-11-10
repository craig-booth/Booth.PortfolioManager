using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;


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
using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Web.Test.Controllers
{
    public class StockControllerTests
    {
        private Guid _MissingStockId;
        private Guid _ValidationErrorId;
        private Stock _Stock;

        private Mock<IStockPriceRetriever> _StockPriceRetriever;
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

            _StockPriceRetriever = mockRepository.Create<IStockPriceRetriever>();
            _StockPriceRetriever.Setup(x => x.GetPrice(_Stock.Id, Date.Today)).Returns(12.25m);
            _StockPriceRetriever.Setup(x => x.GetPrices(_Stock.Id, It.IsAny<DateRange>())).Returns<Guid, DateRange>((id, dateRange) => DateUtils.Days(dateRange.FromDate, dateRange.ToDate).Select(x => new StockPrice(x, 0.10m)));

            var stockMapper = new StockMapper(_StockPriceRetriever.Object);

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
            _StockServiceMock.Setup(x => x.ChangeDividendRulesAsync(_MissingStockId, It.IsAny<Date>(), It.IsAny<decimal>(), It.IsAny<RoundingRule>(), It.IsAny<bool>(), It.IsAny<Domain.Stocks.DrpMethod>())).Returns(Task.FromResult<ServiceResult>(ServiceResult.NotFound()));
            _StockServiceMock.Setup(x => x.ChangeDividendRulesAsync(_ValidationErrorId, It.IsAny<Date>(), It.IsAny<decimal>(), It.IsAny<RoundingRule>(), It.IsAny<bool>(), It.IsAny<Domain.Stocks.DrpMethod>())).Returns(Task.FromResult<ServiceResult>(ServiceResult.Error("Dummy Error")));
            _StockServiceMock.Setup(x => x.ChangeDividendRulesAsync(_Stock.Id, It.IsAny<Date>(), It.IsAny<decimal>(), It.IsAny<RoundingRule>(), It.IsAny<bool>(), It.IsAny<Domain.Stocks.DrpMethod>())).Returns(Task.FromResult<ServiceResult>(ServiceResult.Ok()));

            _StockServiceMock.Setup(x => x.ListStockAsync(_MissingStockId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Date>(), It.IsAny<bool>(), It.IsAny<Domain.Stocks.AssetCategory>())).Returns(Task.FromResult<ServiceResult>(ServiceResult.NotFound()));
            _StockServiceMock.Setup(x => x.ListStockAsync(_ValidationErrorId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Date>(), It.IsAny<bool>(), It.IsAny<Domain.Stocks.AssetCategory>())).Returns(Task.FromResult<ServiceResult>(ServiceResult.Error("Dummy Error")));
            _StockServiceMock.Setup(x => x.ListStockAsync(_Stock.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Date>(), It.IsAny<bool>(), It.IsAny<Domain.Stocks.AssetCategory>())).Returns(Task.FromResult<ServiceResult>(ServiceResult.Ok()));

            _StockServiceMock.Setup(x => x.ChangeStockAsync(_MissingStockId, It.IsAny<Date>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Domain.Stocks.AssetCategory>())).Returns(Task.FromResult<ServiceResult>(ServiceResult.NotFound()));
            _StockServiceMock.Setup(x => x.ChangeStockAsync(_ValidationErrorId, It.IsAny<Date>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Domain.Stocks.AssetCategory>())).Returns(Task.FromResult<ServiceResult>(ServiceResult.Error("Dummy Error")));
            _StockServiceMock.Setup(x => x.ChangeStockAsync(_Stock.Id, It.IsAny<Date>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Domain.Stocks.AssetCategory>())).Returns(Task.FromResult<ServiceResult>(ServiceResult.Ok()));

            _StockServiceMock.Setup(x => x.DelistStockAsync(_MissingStockId, It.IsAny<Date>())).Returns(Task.FromResult<ServiceResult>(ServiceResult.NotFound()));
            _StockServiceMock.Setup(x => x.DelistStockAsync(_ValidationErrorId, It.IsAny<Date>())).Returns(Task.FromResult<ServiceResult>(ServiceResult.Error("Dummy Error")));
            _StockServiceMock.Setup(x => x.DelistStockAsync(_Stock.Id, It.IsAny<Date>())).Returns(Task.FromResult<ServiceResult>(ServiceResult.Ok()));
            
            _StockServiceMock.Setup(x => x.UpdateClosingPricesAsync(_MissingStockId, It.IsAny<IEnumerable<StockPrice>>())).Returns(Task.FromResult<ServiceResult>(ServiceResult.NotFound()));
            _StockServiceMock.Setup(x => x.UpdateClosingPricesAsync(_ValidationErrorId, It.IsAny<IEnumerable<StockPrice>>())).Returns(Task.FromResult<ServiceResult>(ServiceResult.Error("Dummy Error")));
            _StockServiceMock.Setup(x => x.UpdateClosingPricesAsync(_Stock.Id, It.IsAny<IEnumerable<StockPrice>>())).Returns(Task.FromResult<ServiceResult>(ServiceResult.Ok()));

            _StockServiceMock.Setup(x => x.UpdateCurrentPrice(_MissingStockId, It.IsAny<decimal>())).Returns(ServiceResult.NotFound());
            _StockServiceMock.Setup(x => x.UpdateCurrentPrice(_ValidationErrorId, It.IsAny<decimal>())).Returns(ServiceResult.Error("Dummy Error"));
            _StockServiceMock.Setup(x => x.UpdateCurrentPrice(_Stock.Id, It.IsAny<decimal>())).Returns(ServiceResult.Ok());

            _Controller = new StockController(_StockServiceMock.Object, _StockQueryMock.Object, stockMapper);
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

            response.Result.Should().BeOkObjectResult().Value.Should().BeEquivalentTo(new
            {
                Id = _Stock.Id,
                AsxCode = "XYZ",
                Name = "XYZ Pty Ltd",
                ListingDate = new Date(2000, 01, 01),
                Category = RestApi.Stocks.AssetCategory.AustralianStocks,
                Trust = false,
                StapledSecurity = false,
                DelistedDate = Date.MaxValue,
                LastPrice = 12.25m,
                CompanyTaxRate = 0.30m,
                DividendRoundingRule = RoundingRule.Round,
                DrpActive = false,
                DrpMethod = RestApi.Stocks.DrpMethod.Round
            });
        }

        [Fact]
        public void GetByIdDate()
        {
            var response = _Controller.Get(_Stock.Id, new DateTime(2005, 01, 01));

            response.Result.Should().BeOkObjectResult().Value.Should().BeEquivalentTo(new
            {
                Id = _Stock.Id,
                AsxCode = "ABC",
                Name = "ABC Pty Ltd",
                ListingDate = new Date(2000, 01, 01),
                Category = RestApi.Stocks.AssetCategory.AustralianStocks,
                Trust = false,
                StapledSecurity = false,
                DelistedDate = Date.MaxValue,
                LastPrice = 12.25m,
                CompanyTaxRate = 0.30m,
                DividendRoundingRule = RoundingRule.Round,
                DrpActive = false,
                DrpMethod = RestApi.Stocks.DrpMethod.Round
            });
        }


        [Fact]
        public void GetWithQueryAndNoDates()
        {
            var response = _Controller.Get("XYZ", null, null, null);

            _StockQueryMock.Verify(x => x.Find(It.IsAny<Func<StockProperties, bool>>()));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new
                {
                    Id = _Stock.Id,
                    AsxCode = "XYZ",
                    Name = "XYZ Pty Ltd",
                    ListingDate = new Date(2000, 01, 01),
                    Category = RestApi.Stocks.AssetCategory.AustralianStocks,
                    Trust = false,
                    StapledSecurity = false,
                    DelistedDate = Date.MaxValue,
                    LastPrice = 12.25m,
                    CompanyTaxRate = 0.30m,
                    DividendRoundingRule = RoundingRule.Round,
                    DrpActive = false,
                    DrpMethod = RestApi.Stocks.DrpMethod.Round
                });
        }

        [Fact]
        public void GetWithQueryAndDate()
        {
            var response = _Controller.Get("XYZ", new DateTime(2011, 01, 01), null, null);

            _StockQueryMock.Verify(x => x.Find(new Date(2011, 01, 01), It.IsAny<Func<StockProperties, bool>>()));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new
                {
                    Id = _Stock.Id,
                    AsxCode = "XYZ",
                    Name = "XYZ Pty Ltd",
                    ListingDate = new Date(2000, 01, 01),
                    Category = RestApi.Stocks.AssetCategory.AustralianStocks,
                    Trust = false,
                    StapledSecurity = false,
                    DelistedDate = Date.MaxValue,
                    LastPrice = 12.25m,
                    CompanyTaxRate = 0.30m,
                    DividendRoundingRule = RoundingRule.Round,
                    DrpActive = false,
                    DrpMethod = RestApi.Stocks.DrpMethod.Round
                });
        }

        [Fact]
        public void GetWithQueryAndFromDate()
        {
            var response = _Controller.Get("XYZ", null, new DateTime(2009, 01, 01), null);

            _StockQueryMock.Verify(x => x.Find(new DateRange(new Date(2009, 01, 01), Date.MaxValue), It.IsAny<Func<StockProperties, bool>>()));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new
                {
                    Id = _Stock.Id,
                    AsxCode = "XYZ",
                    Name = "XYZ Pty Ltd",
                    ListingDate = new Date(2000, 01, 01),
                    Category = RestApi.Stocks.AssetCategory.AustralianStocks,
                    Trust = false,
                    StapledSecurity = false,
                    DelistedDate = Date.MaxValue,
                    LastPrice = 12.25m,
                    CompanyTaxRate = 0.30m,
                    DividendRoundingRule = RoundingRule.Round,
                    DrpActive = false,
                    DrpMethod = RestApi.Stocks.DrpMethod.Round
                });
        }

        [Fact]
        public void GetWithQueryAndToDate()
        {
            var response = _Controller.Get("XYZ", null, null, new DateTime(2055, 01, 01));

            _StockQueryMock.Verify(x => x.Find(new DateRange(Date.MinValue, new Date(2055, 01, 01)), It.IsAny<Func<StockProperties, bool>>()));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new
                {
                    Id = _Stock.Id,
                    AsxCode = "XYZ",
                    Name = "XYZ Pty Ltd",
                    ListingDate = new Date(2000, 01, 01),
                    Category = RestApi.Stocks.AssetCategory.AustralianStocks,
                    Trust = false,
                    StapledSecurity = false,
                    DelistedDate = Date.MaxValue,
                    LastPrice = 12.25m,
                    CompanyTaxRate = 0.30m,
                    DividendRoundingRule = RoundingRule.Round,
                    DrpActive = false,
                    DrpMethod = RestApi.Stocks.DrpMethod.Round
                });
        }

        [Fact]
        public void GetWithQueryAndFromAndToDate()
        {
            var response = _Controller.Get("XYZ", null, new DateTime(2009, 01, 01), new DateTime(2012, 01, 01));

            _StockQueryMock.Verify(x => x.Find(new DateRange(new Date(2009, 01, 01), new Date(2012, 01, 01)), It.IsAny<Func<StockProperties, bool>>()));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new
                {
                    Id = _Stock.Id,
                    AsxCode = "XYZ",
                    Name = "XYZ Pty Ltd",
                    ListingDate = new Date(2000, 01, 01),
                    Category = RestApi.Stocks.AssetCategory.AustralianStocks,
                    Trust = false,
                    StapledSecurity = false,
                    DelistedDate = Date.MaxValue,
                    LastPrice = 12.25m,
                    CompanyTaxRate = 0.30m,
                    DividendRoundingRule = RoundingRule.Round,
                    DrpActive = false,
                    DrpMethod = RestApi.Stocks.DrpMethod.Round
                });
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

            _StockQueryMock.Verify(x => x.All());

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new
                {
                    Id = _Stock.Id,
                    AsxCode = "XYZ",
                    Name = "XYZ Pty Ltd",
                    ListingDate = new Date(2000, 01, 01),
                    Category = RestApi.Stocks.AssetCategory.AustralianStocks,
                    Trust = false,
                    StapledSecurity = false,
                    DelistedDate = Date.MaxValue,
                    LastPrice = 12.25m,
                    CompanyTaxRate = 0.30m,
                    DividendRoundingRule = RoundingRule.Round,
                    DrpActive = false,
                    DrpMethod = RestApi.Stocks.DrpMethod.Round
                });
        }

        [Fact]
        public void GetWithoutQueryAndDate()
        {
            var response = _Controller.Get(null, new DateTime(2011, 01, 01), null, null);

            _StockQueryMock.Verify(x => x.All(new Date(2011, 01, 01)));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new
                {
                    Id = _Stock.Id,
                    AsxCode = "XYZ",
                    Name = "XYZ Pty Ltd",
                    ListingDate = new Date(2000, 01, 01),
                    Category = RestApi.Stocks.AssetCategory.AustralianStocks,
                    Trust = false,
                    StapledSecurity = false,
                    DelistedDate = Date.MaxValue,
                    LastPrice = 12.25m,
                    CompanyTaxRate = 0.30m,
                    DividendRoundingRule = RoundingRule.Round,
                    DrpActive = false,
                    DrpMethod = RestApi.Stocks.DrpMethod.Round
                });
        }

        [Fact]
        public void GetWithoutQueryAndFromDate()
        {
            var response = _Controller.Get(null, null, new DateTime(2009, 01, 01), null);

            _StockQueryMock.Verify(x => x.All(new DateRange(new Date(2009, 01, 01), Date.MaxValue)));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new
                {
                    Id = _Stock.Id,
                    AsxCode = "XYZ",
                    Name = "XYZ Pty Ltd",
                    ListingDate = new Date(2000, 01, 01),
                    Category = RestApi.Stocks.AssetCategory.AustralianStocks,
                    Trust = false,
                    StapledSecurity = false,
                    DelistedDate = Date.MaxValue,
                    LastPrice = 12.25m,
                    CompanyTaxRate = 0.30m,
                    DividendRoundingRule = RoundingRule.Round,
                    DrpActive = false,
                    DrpMethod = RestApi.Stocks.DrpMethod.Round
                });
        }

        [Fact]
        public void GetWithoutQueryAndToDate()
        {
            var response = _Controller.Get(null, null, null, new DateTime(2055, 01, 01));

            _StockQueryMock.Verify(x => x.All(new DateRange(Date.MinValue, new Date(2055, 01, 01))));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new
                {
                    Id = _Stock.Id,
                    AsxCode = "XYZ",
                    Name = "XYZ Pty Ltd",
                    ListingDate = new Date(2000, 01, 01),
                    Category = RestApi.Stocks.AssetCategory.AustralianStocks,
                    Trust = false,
                    StapledSecurity = false,
                    DelistedDate = Date.MaxValue,
                    LastPrice = 12.25m,
                    CompanyTaxRate = 0.30m,
                    DividendRoundingRule = RoundingRule.Round,
                    DrpActive = false,
                    DrpMethod = RestApi.Stocks.DrpMethod.Round
                });

        }

        [Fact]
        public void GetWithoutQueryAndFromAndToDate()
        {
            var response = _Controller.Get(null, null, new DateTime(2009, 01, 01), new DateTime(2012, 01, 01));

            _StockQueryMock.Verify(x => x.All(new DateRange(new Date(2009, 01, 01), new Date(2012, 01, 01))));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<List<StockResponse>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new
                {
                    Id = _Stock.Id,
                    AsxCode = "XYZ",
                    Name = "XYZ Pty Ltd",
                    ListingDate = new Date(2000, 01, 01),
                    Category = RestApi.Stocks.AssetCategory.AustralianStocks,
                    Trust = false,
                    StapledSecurity = false,
                    DelistedDate = Date.MaxValue,
                    LastPrice = 12.25m,
                    CompanyTaxRate = 0.30m,
                    DividendRoundingRule = RoundingRule.Round,
                    DrpActive = false,
                    DrpMethod = RestApi.Stocks.DrpMethod.Round
                });

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
                .Which.Should().BeEquivalentTo(new {
                
                    Id = _Stock.Id,
                    AsxCode = "XYZ",
                    Name = "XYZ Pty Ltd",
                    ListingDate = new Date(2000, 01, 01),
                    DelistedDate = Date.MaxValue,

                    History = new []
                        {
                            new StockHistoryResponse.HistoricProperties()
                            {
                                FromDate = new Date(2000, 01, 01),
                                ToDate = new Date(2009, 12, 31),
                                AsxCode = "ABC",
                                Name = "ABC Pty Ltd",
                                Category = RestApi.Stocks.AssetCategory.AustralianStocks
                            },
                            new StockHistoryResponse.HistoricProperties()
                            {
                                FromDate = new Date(2010, 01, 01),
                                ToDate = Date.MaxValue,
                                AsxCode = "XYZ",
                                Name = "XYZ Pty Ltd",
                                Category = RestApi.Stocks.AssetCategory.AustralianStocks
                            }
                        },

                    DividendRules = new []
                    {
                        new StockHistoryResponse.HistoricDividendRules
                        {
                            FromDate = new Date(2000, 01, 01),
                            ToDate = Date.MaxValue,
                            CompanyTaxRate = 0.30m,
                            DrpActive = false,
                            DrpMethod = RestApi.Stocks.DrpMethod.Round,
                            RoundingRule = RoundingRule.Round
                        }
                    }
                });
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
            _StockPriceRetriever.Verify(x => x.GetPrices(_Stock.Id, expectedDateRange));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<StockPriceResponse>()
                .Which.ClosingPrices.Should().StartWith(new ClosingPrice() { Date = new Date(2022, 11, 11), Price = 0.10m })
                .And.EndWith(new ClosingPrice() { Date = Date.Today, Price = 0.10m })
                .And.HaveCount(365);
        }

        [Fact]
        public void GetClosingPricesWithFromDateOnly()
        {
            var response = _Controller.GetClosingPrices(_Stock.Id, new DateTime(2010, 01, 01), null);

            var expectedDateRange = new DateRange(new Date(2010, 01, 01), new Date(2010, 12, 31));
            _StockPriceRetriever.Verify(x => x.GetPrices(_Stock.Id, expectedDateRange));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<StockPriceResponse>()
                .Which.ClosingPrices.Should().StartWith(new ClosingPrice() { Date = new Date(2010, 01, 01), Price = 0.10m })
                .And.EndWith(new ClosingPrice() { Date = new Date(2010, 12, 31), Price = 0.10m })
                .And.HaveCount(365);
        }

        [Fact]
        public void GetClosingPricesWithToDateOnly()
        {
            var response = _Controller.GetClosingPrices(_Stock.Id, null, new DateTime(2015, 01, 01));

            var expectedDateRange = new DateRange(new Date(2014, 01, 02), new Date(2015, 01, 01));
            _StockPriceRetriever.Verify(x => x.GetPrices(_Stock.Id, expectedDateRange));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<StockPriceResponse>()
                .Which.ClosingPrices.Should().StartWith(new ClosingPrice() { Date = new Date(2014, 01, 02), Price = 0.10m })
                .And.EndWith(new ClosingPrice() { Date = new Date(2015, 01, 01), Price = 0.10m })
                .And.HaveCount(365);
        }

        [Fact]
        public void GetClosingPricesWithBothDates()
        {
            var response = _Controller.GetClosingPrices(_Stock.Id, new DateTime(2010, 01, 01), new DateTime(2010, 01, 20));

            var expectedDateRange = new DateRange(new Date(2010, 01, 01), new Date(2010, 01, 20));
            _StockPriceRetriever.Verify(x => x.GetPrices(_Stock.Id, expectedDateRange));

            response.Result.Should().BeOkObjectResult()
                .Value.Should().BeOfType<StockPriceResponse>()
                .Which.ClosingPrices.Should().StartWith(new ClosingPrice() { Date = new Date(2010, 01, 01), Price = 0.10m })
                .And.EndWith(new ClosingPrice() { Date = new Date(2010, 01, 20), Price = 0.10m })
                .And.HaveCount(20);

        }

        [Fact]
        public async Task CreateStockValidationError()
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

            var response = await _Controller.CreateStock(command);

            response.Should().BeBadRequestObjectResult();
        }

        [Fact]
        public async Task CreateStock()
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

            var response = await _Controller.CreateStock(command);

            response.Should().BeOkResult();
        }

        [Fact]
        public async Task ChangeStockInvalidId()
        {
            var command = new ChangeStockCommand()
            {
                Id = _MissingStockId,
                ChangeDate = new Date(2012, 01, 01),
                AsxCode = "XYZ",
                Name = "XYZ Name",
                Category = RestApi.Stocks.AssetCategory.AustralianFixedInterest
            };
            var response = await _Controller.ChangeStock(_MissingStockId, command);

            response.Should().BeNotFoundResult();

        }

        [Fact]
        public async Task ChangeStockValidationError()
        {
            var command = new ChangeStockCommand()
            {
                Id = _ValidationErrorId,
                ChangeDate = new Date(2009, 01, 01),
                AsxCode = "XYZ",
                Name = "XYZ Name",
                Category = RestApi.Stocks.AssetCategory.AustralianFixedInterest
            };
            var response = await _Controller.ChangeStock(_Stock.Id, command);

            response.Should().BeBadRequestObjectResult();
        }

        [Fact]
        public async Task ChangeStock()
        {
            var command = new ChangeStockCommand()
            {
                Id = _Stock.Id,
                ChangeDate = new Date(2009, 01, 01),
                AsxCode = "XYZ",
                Name = "XYZ Name",
                Category = RestApi.Stocks.AssetCategory.AustralianFixedInterest
            };
            var response = await _Controller.ChangeStock(_Stock.Id, command);

            response.Should().BeOkResult();

            _StockServiceMock.Verify(x => x.ChangeStockAsync(_Stock.Id, new Date(2009, 01, 01), "XYZ", "XYZ Name", Domain.Stocks.AssetCategory.AustralianFixedInterest));
        }

        [Fact]
        public async Task DelistStockInvalidId()
        {
            var command = new DelistStockCommand()
            {
                Id = _MissingStockId,
                DelistingDate = new Date(2012, 01, 01)
            };
            var response = await _Controller.DelistStock(_MissingStockId, command);

            response.Should().BeNotFoundResult();
        }

        [Fact]
        public async Task DelistStockValidationError()
        {
            var command = new DelistStockCommand()
            {
                Id = _ValidationErrorId,
                DelistingDate = new Date(2012, 01, 01)
            };
            var response = await _Controller.DelistStock(_MissingStockId, command);

            response.Should().BeBadRequestObjectResult();
        }

        [Fact]
        public async Task DelistStock()
        {
            var command = new DelistStockCommand()
            {
                Id = _Stock.Id,
                DelistingDate = new Date(2012, 01, 01)
            };
            var response = await _Controller.DelistStock(_Stock.Id, command);

            response.Should().BeOkResult();
        }


        [Fact]
        public async Task UpdateClosingPricesInvalidId()
        {
            var command = new UpdateClosingPricesCommand()
            {
                Id = _MissingStockId
            };
            var response = await _Controller.UpdateClosingPrices(_MissingStockId, command);

            response.Should().BeNotFoundResult();
        }

        [Fact]
        public async Task UpdateClosingPricesValidationError()
        {
            var command = new UpdateClosingPricesCommand()
            {
                Id = _ValidationErrorId
            };
            var response = await _Controller.UpdateClosingPrices(_ValidationErrorId, command);

            response.Should().BeBadRequestObjectResult();
        }

        [Fact]
        public async Task UpdateClosingPrices()
        {
            var command = new UpdateClosingPricesCommand()
            {
                Id = _Stock.Id
            };
            var response = await _Controller.UpdateClosingPrices(_Stock.Id, command);

            response.Should().BeOkResult();
        }

        [Fact]
        public async Task ChangeDividendRulesInvalidId()
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
            var response = await _Controller.ChangeDividendRules(_MissingStockId, command);

            response.Should().BeNotFoundResult();   
        }

        [Fact]
        public async Task ChangeDividendRulesValidationError()
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
            var response = await _Controller.ChangeDividendRules(_Stock.Id, command);

            response.Should().BeBadRequestObjectResult();
        }

        [Fact]
        public async Task ChangeDividendRules()
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
            var response = await _Controller.ChangeDividendRules(_Stock.Id, command);

            response.Should().BeOkResult();

            _StockServiceMock.Verify(x => x.ChangeDividendRulesAsync(_Stock.Id, new Date(2009, 01, 01), 0.40m, RoundingRule.Truncate, true, Domain.Stocks.DrpMethod.RoundUp));
        }

    }
}
