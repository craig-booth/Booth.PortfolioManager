using System;
using System.Collections.Generic;
using System.Linq;


using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Web.Mappers;
using Booth.PortfolioManager.Web.Models.Stock;
using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Web.Test.Mappers
{
    public class StockMapperTests
    {

        [Fact]
        public void StockToResponse()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(2000, 01, 01), true, Domain.Stocks.AssetCategory.InternationalProperty);
            stock.ChangeDividendRules(new Date(2000, 01, 01), 0.30m, RoundingRule.Truncate, true, Domain.Stocks.DrpMethod.RoundDown);
            stock.DeList(new Date(2010, 01, 01));

            var priceRetriever = mockRepository.Create<IStockPriceRetriever>();
            priceRetriever.Setup(x => x.GetPrice(stock.Id, Date.Today)).Returns(1.20m);
            var mapper = new StockMapper(priceRetriever.Object);

            var reponse = mapper.ToResponse(stock, new Common.Date(2010, 01, 01));

            reponse.Should().BeEquivalentTo(new
            { 
                Id = stock.Id,
                AsxCode = "ABC",
                Name = "ABC Pty Ltd",
                ListingDate = new Date(2000, 01, 01),
                Category = Models.Stock.AssetCategory.InternationalProperty,
                Trust = true,
                DelistedDate = new Date(2010, 01, 01),
                LastPrice= 1.20m,
                CompanyTaxRate = 0.30m,
                DividendRoundingRule = RoundingRule.Truncate,
                DrpActive = true,
                DrpMethod = Models.Stock.DrpMethod.RoundDown
            });

        }

        [Fact]
        public void StockToHistoryResponse()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(2000, 01, 01), true, Domain.Stocks.AssetCategory.InternationalProperty);
            stock.ChangeProperties(new Date(2010, 01, 01), "XYZ", "XYZ Pty Ltd", Domain.Stocks.AssetCategory.InternationalStocks);
            stock.ChangeDividendRules(new Date(2008, 01, 01), 0.40m, RoundingRule.Truncate, true, Domain.Stocks.DrpMethod.RoundDown);
            stock.DeList(new Date(2020, 01, 01));

            var priceRetriever = mockRepository.Create<IStockPriceRetriever>();
            var mapper = new StockMapper(priceRetriever.Object);

            var reponse = mapper.ToHistoryResponse(stock);

            using (new AssertionScope())
            {
                reponse.Should().BeEquivalentTo(new
                {
                    Id = stock.Id,
                    AsxCode = "XYZ",
                    Name = "XYZ Pty Ltd",
                    ListingDate = new Date(2000, 01, 01),
                    DelistedDate = new Date(2020, 01, 01),
                });

                reponse.History.Should().SatisfyRespectively(

                    first => first.Should().BeEquivalentTo(new {
                        FromDate = new Date(2000, 01, 01),
                        ToDate = new Date(2009, 12, 31),
                        AsxCode = "ABC",
                        Name = "ABC Pty Ltd",
                        Category = Models.Stock.AssetCategory.InternationalProperty
                    }),
                    second => second.Should().BeEquivalentTo(new
                     {
                        FromDate = new Date(2010, 01, 01),
                        ToDate = new Date(2020, 01, 01),
                        AsxCode = "XYZ",
                        Name = "XYZ Pty Ltd",
                        Category = Models.Stock.AssetCategory.InternationalStocks
                    }));

                reponse.DividendRules.Should().SatisfyRespectively(

                    first => first.Should().BeEquivalentTo(new {
                        FromDate = new Date(2000, 01, 01),
                        ToDate = new Date(2007, 12, 31),
                        CompanyTaxRate = 0.30m,
                        RoundingRule = RoundingRule.Round,
                        DrpActive = false,
                        DrpMethod = Models.Stock.DrpMethod.Round
                    }),
                    second => second.Should().BeEquivalentTo(new
                    {
                        FromDate = new Date(2008, 01, 01),
                        ToDate = new Date(2020, 01, 01),
                        CompanyTaxRate = 0.40m,
                        RoundingRule = RoundingRule.Truncate,
                        DrpActive = true,
                        DrpMethod = Models.Stock.DrpMethod.RoundDown
                    }));
            }

        }

        [Fact]
        public void StockToPriceResponse()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var dateRange = new DateRange(new Date(2010, 01, 02), new Date(2010, 01, 05));
            var prices = new []
            {
                new StockPrice(new Date(2010, 01, 01), 0.10m),
                new StockPrice(new Date(2010, 01, 02), 0.20m),
                new StockPrice(new Date(2010, 01, 03), 0.30m)
            };

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(2000, 01, 01), true, Domain.Stocks.AssetCategory.InternationalProperty);
            stock.ChangeProperties(new Date(2010, 01, 01), "XYZ", "XYZ Pty Ltd", Domain.Stocks.AssetCategory.InternationalStocks);
            stock.ChangeDividendRules(new Date(2008, 01, 01), 0.40m, RoundingRule.Truncate, true, Domain.Stocks.DrpMethod.RoundDown);
            stock.DeList(new Date(2020, 01, 01));

            var priceRetriever = mockRepository.Create<IStockPriceRetriever>();
            priceRetriever.Setup(x => x.GetPrices(stock.Id, dateRange)).Returns(prices);
            var mapper = new StockMapper(priceRetriever.Object);

            var reponse = mapper.ToPriceResponse(stock, dateRange);

            using (new AssertionScope())
            {
                reponse.Should().BeEquivalentTo(new
                {
                    Id = stock.Id,
                    AsxCode = "XYZ",
                    Name = "XYZ Pty Ltd"
                });

                reponse.ClosingPrices.Should().SatisfyRespectively(

                    first => first.Should().BeEquivalentTo(new
                    {
                        Date = new Date(2010, 01, 01),
                        Price = 0.10
                    }),
                    second => second.Should().BeEquivalentTo(new
                    {
                        Date = new Date(2010, 01, 02),
                        Price = 0.20
                    }),
                    third => third.Should().BeEquivalentTo(new
                    {
                        Date = new Date(2010, 01, 03),
                        Price = 0.30
                    }));
            }

        }

    }
}
