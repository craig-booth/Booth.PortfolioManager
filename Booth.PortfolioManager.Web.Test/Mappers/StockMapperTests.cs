using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Web.Mappers;
using Booth.PortfolioManager.RestApi.Stocks;
using System.Linq;

namespace Booth.PortfolioManager.Web.Test.Mappers
{
    public class StockMapperTests
    {

        [Fact]
        public void StockToResponse()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var priceHistory = mockRepository.Create<IStockPriceHistory>();
            priceHistory.Setup(x => x.GetPrice(Date.Today)).Returns(1.20m);

            var stock = new Stock(Guid.NewGuid());
            stock.SetPriceHistory(priceHistory.Object);
            stock.List("ABC", "ABC Pty Ltd", new Date(2000, 01, 01), true, Domain.Stocks.AssetCategory.InternationalProperty);
            stock.ChangeDividendRules(new Date(2000, 01, 01), 0.30m, RoundingRule.Truncate, true, Domain.Stocks.DrpMethod.RoundDown);
            stock.DeList(new Date(2010, 01, 01));
           
            var reponse = stock.ToResponse(new Common.Date(2010, 01, 01));

            reponse.Should().BeEquivalentTo(new
            { 
                Id = stock.Id,
                AsxCode = "ABC",
                Name = "ABC Pty Ltd",
                ListingDate = new Date(2000, 01, 01),
                Category = RestApi.Stocks.AssetCategory.InternationalProperty,
                Trust = true,
                StapledSecurity = false,
                DelistedDate = new Date(2010, 01, 01),
                LastPrice= 1.20m,
                CompanyTaxRate = 0.30m,
                DividendRoundingRule = RoundingRule.Truncate,
                DrpActive = true,
                DrpMethod = RestApi.Stocks.DrpMethod.RoundDown
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

            var reponse = stock.ToHistoryResponse();

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
                        Category = RestApi.Stocks.AssetCategory.InternationalProperty
                    }),
                    second => second.Should().BeEquivalentTo(new
                     {
                        FromDate = new Date(2010, 01, 01),
                        ToDate = new Date(2020, 01, 01),
                        AsxCode = "XYZ",
                        Name = "XYZ Pty Ltd",
                        Category = RestApi.Stocks.AssetCategory.InternationalStocks
                    }));

                reponse.DividendRules.Should().SatisfyRespectively(

                    first => first.Should().BeEquivalentTo(new {
                        FromDate = new Date(2000, 01, 01),
                        ToDate = new Date(2007, 12, 31),
                        CompanyTaxRate = 0.30m,
                        RoundingRule = RoundingRule.Round,
                        DrpActive = false,
                        DrpMethod = RestApi.Stocks.DrpMethod.Round
                    }),
                    second => second.Should().BeEquivalentTo(new
                    {
                        FromDate = new Date(2008, 01, 01),
                        ToDate = new Date(2020, 01, 01),
                        CompanyTaxRate = 0.40m,
                        RoundingRule = RoundingRule.Truncate,
                        DrpActive = true,
                        DrpMethod = RestApi.Stocks.DrpMethod.RoundDown
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

            var priceHistory = mockRepository.Create<IStockPriceHistory>();
            priceHistory.Setup(x => x.GetPrices(dateRange)).Returns(prices);


            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(2000, 01, 01), true, Domain.Stocks.AssetCategory.InternationalProperty);
            stock.ChangeProperties(new Date(2010, 01, 01), "XYZ", "XYZ Pty Ltd", Domain.Stocks.AssetCategory.InternationalStocks);
            stock.ChangeDividendRules(new Date(2008, 01, 01), 0.40m, RoundingRule.Truncate, true, Domain.Stocks.DrpMethod.RoundDown);
            stock.DeList(new Date(2020, 01, 01));

            stock.SetPriceHistory(priceHistory.Object);

            var reponse = stock.ToPriceResponse(dateRange);

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
