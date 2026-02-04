using System;

using Xunit;
using FluentAssertions;
using FluentAssertions.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Booth.Common;
using Booth.PortfolioManager.Web.Models.CorporateAction;
using Booth.PortfolioManager.Web.Serialization;

namespace Booth.PortfolioManager.Web.Test.Serialization
{
    public class CorporateActionSerializationTests
    {
        [Fact]
        public void DeserializeTypePropertyMissing()
        {
            var serializer = new ModelSerializer();

            var transactionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();

            var json = "{\"id\":\"" + transactionId + "\","
                            + "\"stock\":\"" + stockId + "\","
                            + "\"actionDate\":\"2000-01-10\","
                            + "\"description\":\"description\"}";


            Action a = () => serializer.Deserialize<CorporateAction>(json);
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
                            + "\"actionDate\":\"2000-01-10\","
                            + "\"description\":\"description\"}";

            Action a = () => serializer.Deserialize<CorporateAction>(json);
            a.Should().ThrowExactly<JsonReaderException>();
        }

        [Fact]
        public void SerializeCapitalReturn()
        {
            var serializer = new ModelSerializer();

            var actionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();
            var action = new CapitalReturn()
            {
                Id = actionId,
                Stock = stockId,
                ActionDate = new Date(2000, 01, 10),
                Description = "description",
                PaymentDate = new Date(2000, 02, 01),
                Amount = 12.00m
            };

            var json = JToken.Parse(serializer.Serialize(action));

            var expectedJson = JToken.Parse("{\"id\":\"" + actionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"capitalReturn\","
                             + "\"actionDate\":\"2000-01-10\","
                             + "\"description\":\"description\","
                             + "\"paymentDate\":\"2000-02-01\","
                             + "\"amount\":12.00}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeCapitalReturn()
        {
            var serializer = new ModelSerializer();

            var actionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();

            var json = "{\"id\":\"" + actionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"capitalReturn\","
                             + "\"actionDate\":\"2000-01-10\","
                             + "\"description\":\"description\","
                             + "\"paymentDate\":\"2000-02-01\","
                             + "\"amount\":12.00}";

            var transaction = serializer.Deserialize<CorporateAction>(json);

            var expected = new CapitalReturn()
            {
                Id = actionId,
                Stock = stockId,
                ActionDate = new Date(2000, 01, 10),
                Description = "description",
                PaymentDate = new Date(2000, 02, 01),
                Amount = 12.00m
            };

            transaction.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SerializeCompositeAction()
        {
            var serializer = new ModelSerializer();

            var actionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();
            var action = new CompositeAction()
            {
                Id = actionId,
                Stock = stockId,
                ActionDate = new Date(2000, 01, 10),
                Description = "description"
            };
            action.ChildActions.Add(new CapitalReturn()
            {
                Id = actionId,
                Stock = stockId,
                ActionDate = new Date(2000, 01, 10),
                Description = "description",
                PaymentDate = new Date(2000, 02, 01),
                Amount = 12.00m
            });

            var json = JToken.Parse(serializer.Serialize(action));

            var expectedJson = JToken.Parse("{\"id\":\"" + actionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"compositeAction\","
                             + "\"actionDate\":\"2000-01-10\","
                             + "\"description\":\"description\","
                             + "\"childActions\":["
                                + "{\"id\":\"" + actionId + "\","
                                + "\"stock\":\"" + stockId + "\","
                                + "\"type\":\"capitalReturn\","
                                + "\"actionDate\":\"2000-01-10\","
                                + "\"description\":\"description\","
                                + "\"paymentDate\":\"2000-02-01\","
                                + "\"amount\":12.00}"
                            + "]}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeCompositeAction()
        {
            var serializer = new ModelSerializer();

            var actionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();

            var json = "{\"id\":\"" + actionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"compositeAction\","
                             + "\"actionDate\":\"2000-01-10\","
                             + "\"description\":\"description\","
                             + "\"childActions\":["
                                + "{\"id\":\"" + actionId + "\","
                                + "\"stock\":\"" + stockId + "\","
                                + "\"type\":\"capitalReturn\","
                                + "\"actionDate\":\"2000-01-10\","
                                + "\"description\":\"description\","
                                + "\"paymentDate\":\"2000-02-01\","
                                + "\"amount\":12.00}"
                            + "]}";

            var transaction = serializer.Deserialize<CorporateAction>(json);

            var expected = new CompositeAction()
            {
                Id = actionId,
                Stock = stockId,
                ActionDate = new Date(2000, 01, 10),
                Description = "description"
            };
            expected.ChildActions.Add(new CapitalReturn()
            {
                Id = actionId,
                Stock = stockId,
                ActionDate = new Date(2000, 01, 10),
                Description = "description",
                PaymentDate = new Date(2000, 02, 01),
                Amount = 12.00m
            });

            transaction.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SerializeDividend()
        {
            var serializer = new ModelSerializer();

            var actionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();
            var action = new Dividend()
            {
                Id = actionId,
                Stock = stockId,
                ActionDate = new Date(2000, 01, 10),
                Description = "description",
                PaymentDate = new Date(2000, 02, 01),
                Amount = 12.00m,
                PercentFranked = 1.00m,
                DrpPrice = 13.00m
            };

            var json = JToken.Parse(serializer.Serialize(action));

            var expectedJson = JToken.Parse("{\"id\":\"" + actionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"dividend\","
                             + "\"actionDate\":\"2000-01-10\","
                             + "\"description\":\"description\","
                             + "\"paymentDate\":\"2000-02-01\","
                             + "\"amount\":12.00,"
                             + "\"percentFranked\":1.00,"
                             + "\"drpPrice\":13.00}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeDividend()
        {
            var serializer = new ModelSerializer();

            var actionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();

            var json = "{\"id\":\"" + actionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"dividend\","
                             + "\"actionDate\":\"2000-01-10\","
                             + "\"description\":\"description\","
                             + "\"paymentDate\":\"2000-02-01\","
                             + "\"amount\":12.00,"
                             + "\"percentFranked\":1.00,"
                             + "\"drpPrice\":13.00}";

            var transaction = serializer.Deserialize<CorporateAction>(json);

            var expected = new Dividend()
            {
                Id = actionId,
                Stock = stockId,
                ActionDate = new Date(2000, 01, 10),
                Description = "description",
                PaymentDate = new Date(2000, 02, 01),
                Amount = 12.00m,
                PercentFranked = 1.00m,
                DrpPrice = 13.00m
            };

            transaction.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SerializeSplitConsolidation()
        {
            var serializer = new ModelSerializer();

            var actionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();
            var action = new SplitConsolidation()
            {
                Id = actionId,
                Stock = stockId,
                ActionDate = new Date(2000, 01, 10),
                Description = "description",
                OriginalUnits = 1,
                NewUnits = 2
            };

            var json = JToken.Parse(serializer.Serialize(action));

            var expectedJson = JToken.Parse("{\"id\":\"" + actionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"splitConsolidation\","
                             + "\"actionDate\":\"2000-01-10\","
                             + "\"description\":\"description\","
                             + "\"originalUnits\":1,"
                             + "\"newUnits\":2}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeSplitConsolidation()
        {
            var serializer = new ModelSerializer();

            var actionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();

            var json = "{\"id\":\"" + actionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"splitConsolidation\","
                             + "\"actionDate\":\"2000-01-10\","
                             + "\"description\":\"description\","
                             + "\"originalUnits\":1,"
                             + "\"newUnits\":2}";

            var transaction = serializer.Deserialize<CorporateAction>(json);

            var expected = new SplitConsolidation()
            {
                Id = actionId,
                Stock = stockId,
                ActionDate = new Date(2000, 01, 10),
                Description = "description",
                OriginalUnits = 1,
                NewUnits = 2
            };

            transaction.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SerializeTransformation()
        {
            var serializer = new ModelSerializer();

            var actionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();
            var resultStockId = Guid.NewGuid();
            var action = new Transformation()
            {
                Id = actionId,
                Stock = stockId,
                ActionDate = new Date(2000, 01, 10),
                Description = "description",
                ImplementationDate = new Date(2000, 02, 01),
                CashComponent = 3.00m,
                RolloverRefliefApplies = true
            };
            action.AddResultingStock(resultStockId, 1, 2, 45.00m, new Date(2000, 03, 04));

            var json = JToken.Parse(serializer.Serialize(action));

            var expectedJson = JToken.Parse("{\"id\":\"" + actionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"transformation\","
                             + "\"actionDate\":\"2000-01-10\","
                             + "\"description\":\"description\","
                             + "\"implementationDate\":\"2000-02-01\","
                             + "\"cashComponent\":3.00,"
                             + "\"rolloverRefliefApplies\":true,"
                             + "\"resultingStocks\":["
                                + "{\"stock\":\"" + resultStockId + "\","
                                + "\"originalUnits\":1,"
                                + "\"newUnits\":2,"
                                + "\"costBase\":45.00,"
                                + "\"aquisitionDate\":\"2000-03-04\"}"
                             + "]}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeTransformation()
        {
            var serializer = new ModelSerializer();

            var actionId = Guid.NewGuid();
            var stockId = Guid.NewGuid();
            var resultStockId = Guid.NewGuid();
            var json = "{\"id\":\"" + actionId + "\","
                             + "\"stock\":\"" + stockId + "\","
                             + "\"type\":\"transformation\","
                             + "\"actionDate\":\"2000-01-10\","
                             + "\"description\":\"description\","
                             + "\"implementationDate\":\"2000-02-01\","
                             + "\"cashComponent\":3.00,"
                             + "\"rolloverRefliefApplies\":true,"
                             + "\"resultingStocks\":["
                                + "{\"stock\":\"" + resultStockId + "\","
                                + "\"originalUnits\":1,"
                                + "\"newUnits\":2,"
                                + "\"costBase\":45.00,"
                                + "\"aquisitionDate\":\"2000-03-04\"}"
                             + "]}";

            var transaction = serializer.Deserialize<CorporateAction>(json);

            var expected = new Transformation()
            {
                Id = actionId,
                Stock = stockId,
                ActionDate = new Date(2000, 01, 10),
                Description = "description",
                ImplementationDate = new Date(2000, 02, 01),
                CashComponent = 3.00m,
                RolloverRefliefApplies = true
            };
            expected.AddResultingStock(resultStockId, 1, 2, 45.00m, new Date(2000, 03, 04));

            transaction.Should().BeEquivalentTo(expected);
        }
    }
}
