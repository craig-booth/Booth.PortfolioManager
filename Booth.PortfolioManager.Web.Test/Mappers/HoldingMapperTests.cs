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
using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Web.Test.Mappers
{
    public class HoldingMapperTests
    {
        [Fact]  
        public void HoldingToApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(2000, 01, 01), true, Domain.Stocks.AssetCategory.InternationalProperty);

            var holding = new Holding(stock, new Date(2005, 01, 01));
            holding.AddParcel(new Date(2005, 01, 01), new Date(2005, 01, 01), 30, 100.00m, 120.00m, null);

            var priceRetriever = mockRepository.Create<IStockPriceRetriever>();
            priceRetriever.Setup(x => x.GetPrice(stock.Id, new Date(2010, 01, 01))).Returns(1.20m);

            var mapper = new HoldingMapper(priceRetriever.Object);

            var reponse = mapper.ToApi(holding, new Date(2010, 01, 01));

            reponse.Should().BeEquivalentTo(new 
            {
                Stock = new RestApi.Portfolios.Stock()
                {
                    Id = stock.Id,
                    AsxCode = "ABC",
                    Name = "ABC Pty Ltd",
                    Category = RestApi.Stocks.AssetCategory.InternationalProperty
                },
                Units = 30,
                Value = 36.00m,
                Cost = 100.00m,
                CostBase = 120.00m
            });

        }
    }
}
