using System;

using Xunit;
using FluentAssertions;
using FluentAssertions.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Booth.Common;
using Booth.PortfolioManager.Web.Models.Transaction;
using Booth.PortfolioManager.Web.Serialization;

namespace Booth.PortfolioManager.Web.Test.Serialization
{
    public class TransactionSerializationTests
    {
    
        [Fact]
        public void DeserializeTypePropertyMissing()
        {
            var serializer = new ModelSerializer();

            var transactionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();

            var json = "{\"id\":\"" + transactionId + "\","
                            + "\"stock\":\"" + stockId + "\","
                            + "\"transactionDate\":\"2000-01-10\","
                            + "\"comment\":\"comment\","
                            + "\"description\":\"description\"}";

            
            Action a = () => serializer.Deserialize<Transaction>(json);
            a.Should().ThrowExactly<JsonReaderException>();
        }

        [Fact]
        public void DeserializeTypePropertyIncorrectValue()
        {
            var serializer = new ModelSerializer();

            var transactionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();

            var json = "{\"id\":\"" + transactionId + "\","
                            + "\"stock\":\"" + stockId + "\","
                            + "\"type\":\"xxx\","
                            + "\"transactionDate\":\"2000-01-10\","
                            + "\"comment\":\"comment\","
                            + "\"description\":\"description\"}";

            Action a = () => serializer.Deserialize<Transaction>(json);
            a.Should().ThrowExactly<JsonReaderException>();
        }

        [Fact]
        public void SerializeAquisition()
        {
            var serializer = new ModelSerializer();

            var transactionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();
            var transaction = new Aquisition()
            {
                Id = transactionId,
                Stock = stockId,
                TransactionDate = new Date(2000, 01, 10),
                Comment = "comment",
                Description = "description",
                Units = 100,
                AveragePrice = 12.00m,
                TransactionCosts = 19.95m,
                CreateCashTransaction = true
            };

            var json = JToken.Parse(serializer.Serialize(transaction));

            var expectedJson = JToken.Parse("{\"id\":\"" + transactionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"aquisition\","
                             + "\"transactionDate\":\"2000-01-10\","
                             + "\"comment\":\"comment\","
                             + "\"description\":\"description\","
                             + "\"units\":100,"
                             + "\"averagePrice\":12.00,"
                             + "\"transactionCosts\":19.95,"
                             + "\"createCashTransaction\":true}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeAquisition()
        {
            var serializer = new ModelSerializer();

            var transactionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();

            var json = "{\"id\":\"" + transactionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"aquisition\","
                             + "\"transactionDate\":\"2000-01-10\","
                             + "\"comment\":\"comment\","
                             + "\"description\":\"description\","
                             + "\"units\":\"100\","
                             + "\"averagePrice\":\"12.00\","
                             + "\"transactionCosts\":\"19.95\","
                             + "\"createCashTransaction\":\"true\"}";

            var transaction = serializer.Deserialize<Transaction>(json);

            var expected = new Aquisition()
            {
                Id = transactionId,
                Stock = stockId,
                TransactionDate = new Date(2000, 01, 10),
                Comment = "comment",
                Description = "description",
                Units = 100,
                AveragePrice = 12.00m,
                TransactionCosts = 19.95m,
                CreateCashTransaction = true
            };

            transaction.Should().BeEquivalentTo(expected);  
        }

        [Fact]
        public void SerializeCashTransaction()
        {
            var serializer = new ModelSerializer();

            var transactionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();
            var transaction = new CashTransaction()
            {
                Id = transactionId,
                Stock = stockId,
                TransactionDate = new Date(2000, 01, 10),
                Comment = "comment",
                Description = "description",
                CashTransactionType = CashTransactionType.Fee,
                Amount = 15.00m
            };

            var json = JToken.Parse(serializer.Serialize(transaction));

            var expectedJson = JToken.Parse("{\"id\":\"" + transactionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"cashTransaction\","
                             + "\"transactionDate\":\"2000-01-10\","
                             + "\"comment\":\"comment\","
                             + "\"description\":\"description\","
                             + "\"cashTransactionType\":\"fee\","
                             + "\"amount\":15.00}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeCashTransaction()
        {
            var serializer = new ModelSerializer();

            var transactionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();

            var json = "{\"id\":\"" + transactionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"cashtransaction\","
                             + "\"transactionDate\":\"2000-01-10\","
                             + "\"comment\":\"comment\","
                             + "\"description\":\"description\","
                             + "\"cashTransactionType\":\"fee\","
                             + "\"amount\":15.00}";

            var transaction = serializer.Deserialize<Transaction>(json);

            var expected = new CashTransaction()
            {
                Id = transactionId,
                Stock = stockId,
                TransactionDate = new Date(2000, 01, 10),
                Comment = "comment",
                Description = "description",
                CashTransactionType = CashTransactionType.Fee,
                Amount = 15.00m
            };

            transaction.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SerializeCostBaseAdjustment()
        {
            var serializer = new ModelSerializer();

            var transactionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();
            var transaction = new CostBaseAdjustment()
            {
                Id = transactionId,
                Stock = stockId,
                TransactionDate = new Date(2000, 01, 10),
                Comment = "comment",
                Description = "description", 
                Percentage = 0.35m
            };

            var json = JToken.Parse(serializer.Serialize(transaction));

            var expectedJson = JToken.Parse("{\"id\":\"" + transactionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"costBaseAdjustment\","
                             + "\"transactionDate\":\"2000-01-10\","
                             + "\"comment\":\"comment\","
                             + "\"description\":\"description\","
                             + "\"percentage\":0.35}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeCostBaseAdjustment()
        {
            var serializer = new ModelSerializer();

            var transactionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();

            var json = "{\"id\":\"" + transactionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"costbaseadjustment\","
                             + "\"transactionDate\":\"2000-01-10\","
                             + "\"comment\":\"comment\","
                             + "\"description\":\"description\","
                             + "\"percentage\":0.35}";

            var transaction = serializer.Deserialize<Transaction>(json);

            var expected = new CostBaseAdjustment()
            {
                Id = transactionId,
                Stock = stockId,
                TransactionDate = new Date(2000, 01, 10),
                Comment = "comment",
                Description = "description",
                Percentage = 0.35m
            };

            transaction.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SerializeDisposal()
        {
            var serializer = new ModelSerializer();

            var transactionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();
            var transaction = new Disposal()
            {
                Id = transactionId,
                Stock = stockId,
                TransactionDate = new Date(2000, 01, 10),
                Comment = "comment",
                Description = "description",
                Units = 100,
                AveragePrice = 12.45m,
                TransactionCosts = 19.95m,
                CgtMethod = CgtCalculationMethod.MinimizeGain,
                CreateCashTransaction = true
            };

            var json = JToken.Parse(serializer.Serialize(transaction));

            var expectedJson = JToken.Parse("{\"id\":\"" + transactionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"disposal\","
                             + "\"transactionDate\":\"2000-01-10\","
                             + "\"comment\":\"comment\","
                             + "\"description\":\"description\","
                             + "\"units\":100,"
                             + "\"averagePrice\":12.45,"
                             + "\"transactionCosts\":19.95,"
                             + "\"cgtMethod\":\"minimizeGain\","
                             + "\"createCashTransaction\":true}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeDisposal()
        {
            var serializer = new ModelSerializer();

            var transactionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();

            var json = "{\"id\":\"" + transactionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"disposal\","
                             + "\"transactionDate\":\"2000-01-10\","
                             + "\"comment\":\"comment\","
                             + "\"description\":\"description\","
                             + "\"units\":100,"
                             + "\"averagePrice\":12.45,"
                             + "\"transactionCosts\":19.95,"
                             + "\"cgtMethod\":\"minimizeGain\","
                             + "\"createCashTransaction\":true}";

            var transaction = serializer.Deserialize<Transaction>(json);

            var expected = new Disposal()
            {
                Id = transactionId,
                Stock = stockId,
                TransactionDate = new Date(2000, 01, 10),
                Comment = "comment",
                Description = "description",
                Units = 100,
                AveragePrice = 12.45m,
                TransactionCosts = 19.95m,
                CgtMethod = CgtCalculationMethod.MinimizeGain,
                CreateCashTransaction = true
            };

            transaction.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SerializeIncomeReceived()
        {
            var serializer = new ModelSerializer();

            var transactionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();
            var transaction = new IncomeReceived()
            {
                Id = transactionId,
                Stock = stockId,
                TransactionDate = new Date(2000, 01, 10),
                Comment = "comment",
                Description = "description",
                RecordDate = new Date(2000, 01, 02),
                FrankedAmount = 10.00m,
                UnfrankedAmount = 11.00m,
                FrankingCredits = 3.00m,
                Interest = 4.00m,
                TaxDeferred = 7.00m,
                DrpCashBalance = 9.00m,
                CreateCashTransaction = true
            };

            var json = JToken.Parse(serializer.Serialize(transaction));

            var expectedJson = JToken.Parse("{\"id\":\"" + transactionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"incomeReceived\","
                             + "\"transactionDate\":\"2000-01-10\","
                             + "\"comment\":\"comment\","
                             + "\"description\":\"description\","
                             + "\"recordDate\":\"2000-01-02\","
                             + "\"frankedAmount\":10.00,"
                             + "\"unfrankedAmount\":11.00,"
                             + "\"frankingCredits\":3.00,"
                             + "\"interest\":4.00,"
                             + "\"taxDeferred\":7.00,"
                             + "\"drpCashBalance\":9.00,"
                             + "\"createCashTransaction\":true}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeIncomeReceived()
        {
            var serializer = new ModelSerializer();

            var transactionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();

            var json = "{\"id\":\"" + transactionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"incomereceived\","
                             + "\"transactionDate\":\"2000-01-10\","
                             + "\"comment\":\"comment\","
                             + "\"description\":\"description\","
                             + "\"recordDate\":\"2000-01-02\","
                             + "\"frankedAmount\":10.00,"
                             + "\"unfrankedAmount\":11.00,"
                             + "\"frankingCredits\":3.00,"
                             + "\"interest\":4.00,"
                             + "\"taxDeferred\":7.00,"
                             + "\"drpCashBalance\":9.00,"
                             + "\"createCashTransaction\":true}";

            var transaction = serializer.Deserialize<Transaction>(json);

            var expected = new IncomeReceived()
            {
                Id = transactionId,
                Stock = stockId,
                TransactionDate = new Date(2000, 01, 10),
                Comment = "comment",
                Description = "description",
                RecordDate = new Date(2000, 01, 02),
                FrankedAmount = 10.00m,
                UnfrankedAmount = 11.00m,
                FrankingCredits = 3.00m,
                Interest = 4.00m,
                TaxDeferred = 7.00m,
                DrpCashBalance = 9.00m,
                CreateCashTransaction = true
            };

            transaction.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SerializeOpeningBalance()
        {
            var serializer = new ModelSerializer();

            var transactionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();
            var transaction = new OpeningBalance()
            {
                Id = transactionId,
                Stock = stockId,
                TransactionDate = new Date(2000, 01, 10),
                Comment = "comment",
                Description = "description",
                Units = 100,
                CostBase = 1450.45m,
                AquisitionDate = new Date(2000, 01, 01)
            };

            var json = JToken.Parse(serializer.Serialize(transaction));

            var expectedJson = JToken.Parse("{\"id\":\"" + transactionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"openingBalance\","
                             + "\"transactionDate\":\"2000-01-10\","
                             + "\"comment\":\"comment\","
                             + "\"description\":\"description\","
                             + "\"units\":100,"
                             + "\"costBase\":1450.45,"
                             + "\"aquisitionDate\":\"2000-01-01\"}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeOpeningBalance()
        {
            var serializer = new ModelSerializer();

            var transactionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();

            var json = "{\"id\":\"" + transactionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"openingbalance\","
                             + "\"transactionDate\":\"2000-01-10\","
                             + "\"comment\":\"comment\","
                             + "\"description\":\"description\","
                             + "\"units\":100,"
                             + "\"costBase\":1450.45,"
                             + "\"aquisitionDate\":\"2000-01-01\"}";

            var transaction = serializer.Deserialize<Transaction>(json);

            var expected = new OpeningBalance()
            {
                Id = transactionId,
                Stock = stockId,
                TransactionDate = new Date(2000, 01, 10),
                Comment = "comment",
                Description = "description",
                Units = 100,
                CostBase = 1450.45m,
                AquisitionDate = new Date(2000, 01, 01)
            };

            transaction.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SerializeReturnOfCapital()
        {
            var serializer = new ModelSerializer();

            var transactionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();
            var transaction = new ReturnOfCapital()
            {
                Id = transactionId,
                Stock = stockId,
                TransactionDate = new Date(2000, 01, 10),
                Comment = "comment",
                Description = "description",
                RecordDate = new Date(2000, 01, 01),
                Amount = 45.00m,
                CreateCashTransaction = true
            };

            var json = JToken.Parse(serializer.Serialize(transaction));

            var expectedJson = JToken.Parse("{\"id\":\"" + transactionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"returnOfCapital\","
                             + "\"transactionDate\":\"2000-01-10\","
                             + "\"comment\":\"comment\","
                             + "\"description\":\"description\","
                             + "\"recordDate\":\"2000-01-01\","
                             + "\"amount\":45.00,"
                             + "\"createCashTransaction\":true}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeReturnOfCapital()
        {
            var serializer = new ModelSerializer();

            var transactionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();

            var json = "{\"id\":\"" + transactionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"returnofcapital\","
                             + "\"transactionDate\":\"2000-01-10\","
                             + "\"comment\":\"comment\","
                             + "\"description\":\"description\","
                             + "\"recordDate\":\"2000-01-01\","
                             + "\"amount\":45.00,"
                             + "\"createCashTransaction\":true}";

            var transaction = serializer.Deserialize<Transaction>(json);

            var expected = new ReturnOfCapital()
            {
                Id = transactionId,
                Stock = stockId,
                TransactionDate = new Date(2000, 01, 10),
                Comment = "comment",
                Description = "description",
                RecordDate = new Date(2000, 01, 01),
                Amount = 45.00m,
                CreateCashTransaction = true
            };

            transaction.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SerializeUnitCountAdjustment()
        {
            var serializer = new ModelSerializer();

            var transactionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();
            var transaction = new UnitCountAdjustment()
            {
                Id = transactionId,
                Stock = stockId,
                TransactionDate = new Date(2000, 01, 10),
                Comment = "comment",
                Description = "description",
                OriginalUnits = 1,
                NewUnits = 2
            };

            var json = JToken.Parse(serializer.Serialize(transaction));

            var expectedJson = JToken.Parse("{\"id\":\"" + transactionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"unitCountAdjustment\","
                             + "\"transactionDate\":\"2000-01-10\","
                             + "\"comment\":\"comment\","
                             + "\"description\":\"description\","
                             + "\"originalUnits\":1,"
                             + "\"newUnits\":2}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeUnitCountAdjustment()
        {
            var serializer = new ModelSerializer();

            var transactionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();

            var json = "{\"id\":\"" + transactionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"unitcountadjustment\","
                             + "\"transactionDate\":\"2000-01-10\","
                             + "\"comment\":\"comment\","
                             + "\"description\":\"description\","
                             + "\"originalUnits\":1,"
                             + "\"newUnits\":2}";

            var transaction = serializer.Deserialize<Transaction>(json);

            var expected = new UnitCountAdjustment()
            {
                Id = transactionId,
                Stock = stockId,
                TransactionDate = new Date(2000, 01, 10),
                Comment = "comment",
                Description = "description",
                OriginalUnits = 1,
                NewUnits = 2
            };

            transaction.Should().BeEquivalentTo(expected);
        }
    } 
}
