using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Repository.Serialization;
using Booth.PortfolioManager.Domain.Users;
using Booth.PortfolioManager.Domain.Stocks;
using FluentAssertions.Execution;

namespace Booth.PortfolioManager.Repository.Test.Serialization
{


    [Collection(Collection.Serialization)]
    public class PortfolioSerializerTests
    {
        private readonly SerializationTestFixture _Fixture;

        public PortfolioSerializerTests(SerializationTestFixture fixture)
        {
            _Fixture = fixture;
        }

        [Fact]
        public void ShouldSerializePortfolio()
        {
            var portfolioId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var stockId = Guid.NewGuid();
            var transaction1Id = Guid.NewGuid();
            var transaction2Id = Guid.NewGuid();

            var stock = new Stock(stockId);
            stock.List("ABC", "Test Stock", new Date(1990, 01, 01), false, AssetCategory.AustralianStocks);
            _Fixture.SetStockCache(new [] { stock });        

            var portfolio = _Fixture.PortfolioFactory.CreatePortfolio(portfolioId);
            portfolio.Create("Test", ownerId);
            portfolio.AquireShares(stockId, new Date(2000, 01, 01), 100, 1.30m, 19.95m, false, "A comment", transaction1Id);
            portfolio.ChangeDrpParticipation(stockId, true);
            portfolio.IncomeReceived(stockId, new Date(2000, 06, 07), new Date(2000, 07, 01), 145.00m, 8.00m, 9.00m, 10.00m, 3.45m, 1.89m, false, "Another comment", transaction2Id);

            var json = BsonExtensionMethods.ToJson(portfolio);

            var expected = "{ " +
                "\"_id\" : CSUUID(\"" + portfolioId.ToString() + "\")," +
                " \"name\" : \"Test\"," +
                " \"owner\" : CSUUID(\"" + ownerId.ToString() + "\")," +
                " \"participateInDrp\" : [" +
                    "CSUUID(\"" + stockId.ToString() + "\")" +
                    "]," +
                " \"transactions\" : [" +
                    "{ \"_t\" : \"Aquisition\"," +
                        " \"_id\" : CSUUID(\"" + transaction1Id.ToString() + "\")," +
                        " \"date\" : \"2000-01-01\"," +
                        " \"stock\" : CSUUID(\"" + stockId.ToString() + "\")," +
                        " \"comment\" : \"A comment\"," +
                        " \"units\" : 100," +
                        " \"averagePrice\" : { \"$numberDecimal\" : \"1.30\" }," +
                        " \"transactionCosts\" : { \"$numberDecimal\" : \"19.95\" }," +
                        " \"createCashTransaction\" : false }, " +
                    "{ \"_t\" : \"IncomeReceived\"," +
                        " \"_id\" : CSUUID(\"" + transaction2Id.ToString() + "\")," +
                        " \"date\" : \"2000-07-01\"," +
                        " \"stock\" : CSUUID(\"" + stockId.ToString() + "\")," +
                        " \"comment\" : \"Another comment\"," +
                        " \"recordDate\" : \"2000-06-07\"," +
                        " \"frankedAmount\" : { \"$numberDecimal\" : \"145.00\" }," +
                        " \"unfrankedAmount\" : { \"$numberDecimal\" : \"8.00\" }," +
                        " \"frankingCredits\" : { \"$numberDecimal\" : \"9.00\" }," +
                        " \"interest\" : { \"$numberDecimal\" : \"10.00\" }," +
                        " \"taxDeferred\" : { \"$numberDecimal\" : \"3.45\" }," +
                        " \"createCashTransaction\" : false," +
                        " \"drpCashBalance\" : { \"$numberDecimal\" : \"1.89\" } }" +
                     "] " +
                "}";

            json.Should().Be(expected);
        }

        [Fact]
        public void ShouldDeserializePortfolio()
        {
            var portfolioId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var stockId = Guid.NewGuid();
            var transaction1Id = Guid.NewGuid();
            var transaction2Id = Guid.NewGuid();

            var stock = new Stock(stockId);
            stock.List("ABC", "Test Stock", new Date(1990, 01, 01), false, AssetCategory.AustralianStocks);
            _Fixture.SetStockCache(new[] { stock });

            var json = "{ " +
                "\"_id\" : CSUUID(\"" + portfolioId.ToString() + "\")," +
                " \"name\" : \"Test\"," +
                " \"owner\" : CSUUID(\"" + ownerId.ToString() + "\")," +
                " \"participateInDrp\" : [" +
                    "CSUUID(\"" + stockId.ToString() + "\")" +
                    "]," +
                " \"transactions\" : [" +
                    "{ \"_t\" : \"Aquisition\"," +
                        " \"_id\" : CSUUID(\"" + transaction1Id.ToString() + "\")," +
                        " \"date\" : \"2000-01-01\"," +
                        " \"stock\" : CSUUID(\"" + stockId.ToString() + "\")," +
                        " \"comment\" : \"A comment\"," +
                        " \"units\" : 100," +
                        " \"averagePrice\" : { \"$numberDecimal\" : \"1.30\" }," +
                        " \"transactionCosts\" : { \"$numberDecimal\" : \"19.95\" }," +
                        " \"createCashTransaction\" : false }, " +
                    "{ \"_t\" : \"IncomeReceived\"," +
                        " \"_id\" : CSUUID(\"" + transaction2Id.ToString() + "\")," +
                        " \"date\" : \"2000-07-01\"," +
                        " \"stock\" : CSUUID(\"" + stockId.ToString() + "\")," +
                        " \"comment\" : \"Another comment\"," +
                        " \"recordDate\" : \"2000-06-07\"," +
                        " \"frankedAmount\" : { \"$numberDecimal\" : \"145.00\" }," +
                        " \"unfrankedAmount\" : { \"$numberDecimal\" : \"8.00\" }," +
                        " \"frankingCredits\" : { \"$numberDecimal\" : \"9.00\" }," +
                        " \"interest\" : { \"$numberDecimal\" : \"10.00\" }," +
                        " \"taxDeferred\" : { \"$numberDecimal\" : \"3.45\" }," +
                        " \"createCashTransaction\" : false," +
                        " \"drpCashBalance\" : { \"$numberDecimal\" : \"1.89\" } }" +
                     "] " +
                "}";

            var portfolio = BsonSerializer.Deserialize<Portfolio>(json);

            using (new AssertionScope())
            {
                portfolio.Should().BeEquivalentTo(new { Id = portfolioId, Name = "Test", Owner = ownerId });
                portfolio.Transactions.Should().HaveCount(2);
                portfolio.Holdings[stockId].Settings.ParticipateInDrp.Should().BeTrue();
                portfolio.Holdings[stockId].Properties[new Date(2001, 01, 01)].Should().BeEquivalentTo(new { Units = 100, Amount = 149.95m, CostBase = 146.50m });
            };
        }
    }
}
