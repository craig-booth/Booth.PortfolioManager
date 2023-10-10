using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MongoDB.Bson.Serialization;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Repository.Serialization;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;
using MongoDB.Driver;
using Booth.PortfolioManager.Domain.Stocks;


namespace Booth.PortfolioManager.Repository.Test.Serialization
{
    [Collection(Collection.Serialization)]
    public class SerializationProviderTests
    {

        private readonly SerializationTestFixture _Fixture;

        public SerializationProviderTests(SerializationTestFixture fixture)
        {
            _Fixture = fixture;
        }

        [Fact]
        public void SerializersRegistered()
        {

            var dateSerializer = BsonSerializer.LookupSerializer(typeof(Date));
            dateSerializer.Should().BeOfType<DateSerializer>();

            var portfolioSerializer = BsonSerializer.LookupSerializer(typeof(Portfolio));
            portfolioSerializer.Should().BeOfType<PortfolioSerializer>();
        }

        [Fact]
        public void SerializersNotRegisteredTwice()
        {
            var stockId = Guid.NewGuid();
            var transactionId = Guid.NewGuid();


            var stock1 = new Stock(stockId);
            stock1.List("ABC", "First Stock", new Date(1990, 01, 01), false, AssetCategory.AustralianStocks);
            _Fixture.SetStockCache(new[] { stock1 });
            
            var stock2 = new Stock(stockId);
            stock2.List("XYZ", "Second Stock", new Date(1990, 01, 01), false, AssetCategory.AustralianStocks);

            var stockResolver2 = new Mock<IStockResolver>();
            stockResolver2.Setup(x => x.GetStock(stockId)).Returns(stock2);

            var portfolioFactory = new Mock<IPortfolioFactory>();

            SerializationProvider.Register(portfolioFactory.Object, stockResolver2.Object);

            var transaction = BsonSerializer.Deserialize<Aquisition>("{ \"_t\" : \"Aquisition\"," +
                        " \"_id\" : CSUUID(\"" + transactionId.ToString() + "\")," +
                        " \"date\" : \"2000-01-01\"," +
                        " \"stock\" : CSUUID(\"" + stockId.ToString() + "\")," +
                        " \"comment\" : \"A comment\"," +
                        " \"units\" : 100," +
                        " \"averagePrice\" : \"1.30\"," +
                        " \"transactionCosts\" : \"19.95\"," +
                        " \"createCashTransaction\" : false }");

            transaction.Stock.Should().Be(stock1);
        }
    }
}