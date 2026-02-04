using System;

using Xunit;
using FluentAssertions;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;

using Booth.Common;
using Booth.PortfolioManager.Web.Models.Stock;
using Booth.PortfolioManager.Web.Serialization;


namespace Booth.PortfolioManager.Web.Test.Serialization
{
    public class StockSerializationTests
    {
        [Fact]
        public void SerializeChangeDividendRulesCommand()
        {
            var serializer = new ModelSerializer();

            var id = Guid.NewGuid();
            var command = new ChangeDividendRulesCommand()
            {
                Id = id,
                ChangeDate = new Date(2013, 01, 02),
                CompanyTaxRate = 0.30m,
                DividendRoundingRule = RoundingRule.Truncate,
                DrpActive = true,
                DrpMethod = DrpMethod.RoundDown
            };

            var json = JToken.Parse(serializer.Serialize(command));

            var expectedJson = JToken.Parse("{\"id\":\"" + id + "\","
                                + "\"changeDate\":\"2013-01-02\","
                                + "\"companyTaxRate\":0.30,"
                                + "\"dividendRoundingRule\":\"truncate\","
                                + "\"drpActive\":true,"
                                + "\"drpMethod\":\"roundDown\"}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeChangeDividendRulesCommand()
        {
            var serializer = new ModelSerializer();

            var id = Guid.NewGuid();
            var json = "{\"id\":\"" + id + "\","
                                + "\"changeDate\":\"2013-01-02\","
                                + "\"companyTaxRate\":0.30,"
                                + "\"dividendRoundingRule\":\"truncate\","
                                + "\"drpActive\":true,"
                                + "\"drpMethod\":\"roundDown\"}";

            var command = serializer.Deserialize<ChangeDividendRulesCommand>(json);

            var expected = new ChangeDividendRulesCommand()
            {
                Id = id,
                ChangeDate = new Date(2013, 01, 02),
                CompanyTaxRate = 0.30m,
                DividendRoundingRule = RoundingRule.Truncate,
                DrpActive = true,
                DrpMethod = DrpMethod.RoundDown
            };
            command.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SerializeChangeStockCommand()
        {
            var serializer = new ModelSerializer();

            var id = Guid.NewGuid();
            var command = new ChangeStockCommand()
            {
                Id = id,
                ChangeDate = new Date(2013, 01, 02),
                AsxCode = "ABC",
                Name = "ABC Pty Ltd",
                Category = AssetCategory.AustralianStocks
            };

            var json = JToken.Parse(serializer.Serialize(command));

            var expectedJson = JToken.Parse("{\"id\":\"" + id + "\","
                                + "\"changeDate\":\"2013-01-02\","
                                + "\"asxCode\":\"ABC\","
                                + "\"name\":\"ABC Pty Ltd\","
                                + "\"category\":\"australianStocks\"}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeChangeStockCommand()
        {
            var serializer = new ModelSerializer();

            var id = Guid.NewGuid();
            var json = "{\"id\":\"" + id + "\","
                                + "\"changeDate\":\"2013-01-02\","
                                + "\"asxCode\":\"ABC\","
                                + "\"name\":\"ABC Pty Ltd\","
                                + "\"category\":\"australianStocks\"}";

            var command = serializer.Deserialize<ChangeStockCommand>(json);

            var expected = new ChangeStockCommand()
            {
                Id = id,
                ChangeDate = new Date(2013, 01, 02),
                AsxCode = "ABC",
                Name = "ABC Pty Ltd",
                Category = AssetCategory.AustralianStocks
            };
            command.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SerializeCreateStockCommand()
        {
            var serializer = new ModelSerializer();

            var id = Guid.NewGuid();
            var command = new CreateStockCommand()
            {
                Id = id,
                ListingDate = new Date(2013, 01, 02),
                AsxCode = "ABC",
                Name = "ABC Pty Ltd",
                Trust = true,
                Category = AssetCategory.InternationalProperty,
            };

            var json = JToken.Parse(serializer.Serialize(command));

            var expectedJson = JToken.Parse("{\"id\":\"" + id + "\","
                            + "\"listingDate\":\"2013-01-02\","
                            + "\"asxCode\":\"ABC\","
                            + "\"name\":\"ABC Pty Ltd\","
                            + "\"trust\":true,"
                            + "\"category\":\"internationalProperty\""
                            + "}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeCreateStockCommand()
        {
            var serializer = new ModelSerializer();

            var id = Guid.NewGuid();
            var json = "{\"id\":\"" + id + "\","
                            + "\"listingDate\":\"2013-01-02\","
                            + "\"asxCode\":\"ABC\","
                            + "\"name\":\"ABC Pty Ltd\","
                            + "\"trust\":true,"
                            + "\"category\":\"internationalProperty\""
                            + "}";

            var command = serializer.Deserialize<CreateStockCommand>(json);

            var expected = new CreateStockCommand()
            {
                Id = id,
                ListingDate = new Date(2013, 01, 02),
                AsxCode = "ABC",
                Name = "ABC Pty Ltd",
                Trust = true,
                Category = AssetCategory.InternationalProperty,
            };
            command.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SerializeDelistStockCommand()
        {
            var serializer = new ModelSerializer();

            var id = Guid.NewGuid();
            var command = new DelistStockCommand()
            {
                Id = id,
                DelistingDate = new Date(2013, 01, 02)
            };

            var json = JToken.Parse(serializer.Serialize(command));

            var expectedJson = JToken.Parse("{\"id\":\"" + id + "\","
                            + "\"delistingDate\":\"2013-01-02\"}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeDelistStockCommand()
        {
            var serializer = new ModelSerializer();

            var id = Guid.NewGuid();
            var json = "{\"id\":\"" + id + "\","
                            + "\"delistingDate\":\"2013-01-02\"}";

            var command = serializer.Deserialize<DelistStockCommand>(json);

            var expected = new DelistStockCommand()
            {
                Id = id,
                DelistingDate = new Date(2013, 01, 02)
            };
            command.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SerializeStockHistoryResponse()
        {
            var serializer = new ModelSerializer();

            var id = Guid.NewGuid();
            var command = new StockHistoryResponse()
            {   
                Id = id,
                AsxCode = "ABC",
                Name = "ABC Pty Ltd",
                ListingDate = new Date(2000, 01, 01),
                DelistedDate = Date.MaxValue,
            };
            command.AddHistory(new Date(2001, 01, 02), new Date(2001, 01, 03), "OLD", "Old Name", AssetCategory.AustralianStocks);
            command.AddDividendRules(new Date(2001, 01, 02), new Date(2001, 01, 03), 0.45m, RoundingRule.Truncate, true, DrpMethod.RoundDown);

            var json = JToken.Parse(serializer.Serialize(command));

            var expectedJson = JToken.Parse("{\"id\":\"" + id + "\","
                            + "\"asxCode\":\"ABC\","
                            + "\"name\":\"ABC Pty Ltd\","
                            + "\"listingDate\":\"2000-01-01\","
                            + "\"delistedDate\":\"9999-12-31\","
                            + "\"history\":["
                                + "{\"fromDate\":\"2001-01-02\",\"toDate\":\"2001-01-03\",\"asxCode\":\"OLD\",\"name\":\"Old Name\",\"category\":\"australianStocks\"}"
                            + "],"
                            + "\"dividendRules\":["
                                + "{\"fromDate\":\"2001-01-02\",\"toDate\":\"2001-01-03\",\"companyTaxRate\":0.45,\"roundingRule\":\"truncate\",\"drpActive\":true,\"drpMethod\":\"roundDown\"}"
                            + "]}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeStockHistoryResponse()
        {
            var serializer = new ModelSerializer();

            var id = Guid.NewGuid();
            var json = "{\"id\":\"" + id + "\","
                            + "\"asxCode\":\"ABC\","
                            + "\"name\":\"ABC Pty Ltd\","
                            + "\"trust\":true,"
                            + "\"listingDate\":\"2000-01-01\","
                            + "\"delistedDate\":\"9999-12-31\","
                            + "\"history\":["
                                + "{\"fromDate\":\"2001-01-02\",\"toDate\":\"2001-01-03\",\"asxCode\":\"OLD\",\"name\":\"Old Name\",\"category\":\"australianStocks\"}"
                            + "],"
                            + "\"dividendRules\":["
                                + "{\"fromDate\":\"2001-01-02\",\"toDate\":\"2001-01-03\",\"companyTaxRate\":0.45,\"roundingRule\":\"truncate\",\"drpActive\":true,\"drpMethod\":\"roundDown\"}"
                            + "]}";

            var command = serializer.Deserialize<StockHistoryResponse>(json);

            var expected = new StockHistoryResponse()
            {
                Id = id,
                AsxCode = "ABC",
                Name = "ABC Pty Ltd",
                ListingDate = new Date(2000, 01, 01),
                DelistedDate = Date.MaxValue,
            };
            expected.AddHistory(new Date(2001, 01, 02), new Date(2001, 01, 03), "OLD", "Old Name", AssetCategory.AustralianStocks);
            expected.AddDividendRules(new Date(2001, 01, 02), new Date(2001, 01, 03), 0.45m, RoundingRule.Truncate, true, DrpMethod.RoundDown);

            command.Should().BeEquivalentTo(expected);
        }
        
        [Fact]
        public void SerializeStockPriceResponse()
        {
            var serializer = new ModelSerializer();

            var id = Guid.NewGuid();
            var command = new StockPriceResponse()
            {
                Id = id,
                AsxCode = "ABC",
                Name = "ABC Pty Ltd"
            };
            command.AddClosingPrice(new Date(2000, 01, 02), 12.00m);
            command.AddClosingPrice(new Date(2000, 01, 03), 13.00m);

            var json = JToken.Parse(serializer.Serialize(command));

            var expectedJson = JToken.Parse("{\"id\":\"" + id + "\","
                            + "\"asxCode\":\"ABC\","
                            + "\"name\":\"ABC Pty Ltd\","
                            + "\"closingPrices\":["
                                + "{\"date\":\"2000-01-02\",\"price\":12.00},"
                                + "{\"date\":\"2000-01-03\",\"price\":13.00}"
                            + "]}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeStockPriceResponse()
        {
            var serializer = new ModelSerializer();

            var id = Guid.NewGuid();
            var json = "{\"id\":\"" + id + "\","
                            + "\"asxCode\":\"ABC\","
                            + "\"name\":\"ABC Pty Ltd\","
                            + "\"closingPrices\":["
                                + "{\"date\":\"2000-01-02\",\"price\":12.00},"
                                + "{\"date\":\"2000-01-03\",\"price\":13.00}"
                            + "]}";

            var command = serializer.Deserialize<StockPriceResponse>(json);

            var expected = new StockPriceResponse()
            {
                Id = id,
                AsxCode = "ABC",
                Name = "ABC Pty Ltd"
            };
            expected.AddClosingPrice(new Date(2000, 01, 02), 12.00m);
            expected.AddClosingPrice(new Date(2000, 01, 03), 13.00m);

            command.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SerializeStockResponse()
        {
            var serializer = new ModelSerializer();

            var id = Guid.NewGuid();
            var command = new StockResponse()
            {
                Id = id,
                AsxCode = "ABC",
                Name = "ABC Pty Ltd",
                Category = AssetCategory.AustralianStocks,
                Trust = true,
                ListingDate = new Date(2000, 01, 01),
                DelistedDate = Date.MaxValue,
                LastPrice = 12.00m,
                CompanyTaxRate = 0.30m,
                DividendRoundingRule = RoundingRule.Round,
                DrpActive = true,
                DrpMethod = DrpMethod.RoundDown
            };

            var json = JToken.Parse(serializer.Serialize(command));

            var expectedJson = JToken.Parse("{\"id\":\"" + id + "\","
                            + "\"asxCode\":\"ABC\","
                            + "\"name\":\"ABC Pty Ltd\","
                            + "\"category\":\"australianStocks\","
                            + "\"trust\":true,"
                            + "\"listingDate\":\"2000-01-01\","
                            + "\"delistedDate\":\"9999-12-31\","
                            + "\"lastPrice\":12.00,"
                            + "\"companyTaxRate\":0.30,"
                            + "\"dividendRoundingRule\":\"round\","
                            + "\"drpActive\":true,"
                            + "\"drpMethod\":\"roundDown\""
                            + "}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeStockResponse()
        {
            var serializer = new ModelSerializer();

            var id = Guid.NewGuid();
            var json = "{\"id\":\"" + id + "\","
                            + "\"asxCode\":\"ABC\","
                            + "\"name\":\"ABC Pty Ltd\","
                            + "\"category\":\"australianStocks\","
                            + "\"trust\":true,"
                            + "\"listingDate\":\"2000-01-01\","
                            + "\"delistedDate\":\"9999-12-31\","
                            + "\"lastPrice\":12.00,"
                            + "\"companyTaxRate\":0.30,"
                            + "\"dividendRoundingRule\":\"round\","
                            + "\"drpActive\":true,"
                            + "\"drpMethod\":\"roundDown\""
                            + "}";

            var command = serializer.Deserialize<StockResponse>(json);

            var expected = new StockResponse()
            {
                Id = id,
                AsxCode = "ABC",
                Name = "ABC Pty Ltd",
                Category = AssetCategory.AustralianStocks,
                Trust = true,
                ListingDate = new Date(2000, 01, 01),
                DelistedDate = Date.MaxValue,
                LastPrice = 12.00m,
                CompanyTaxRate = 0.30m,
                DividendRoundingRule = RoundingRule.Round,
                DrpActive = true,
                DrpMethod = DrpMethod.RoundDown
            };

            command.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SerializeUpdateClosingPricesCommand()
        {
            var serializer = new ModelSerializer();

            var id = Guid.NewGuid();
            var command = new UpdateClosingPricesCommand() { Id = id };
            command.AddClosingPrice(new Date(2000, 01, 01), 10.00m);

            var json = JToken.Parse(serializer.Serialize(command));

            var expectedJson = JToken.Parse("{\"id\":\"" + id + "\","
                            + "\"closingPrices\":["
                                + "{\"date\":\"2000-01-01\",\"price\":10.00}"
                            + "]}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeUpdateClosingPricesCommand()
        {
            var serializer = new ModelSerializer();

            var id = Guid.NewGuid();
            var json = "{\"id\":\"" + id + "\","
                            + "\"closingPrices\":["
                                + "{\"date\":\"2000-01-01\",\"price\":10.00}"
                            + "]}";

            var command = serializer.Deserialize<UpdateClosingPricesCommand>(json);

            var expected = new UpdateClosingPricesCommand() { Id = id };
            expected.AddClosingPrice(new Date(2000, 01, 01), 10.00m);

            command.Should().BeEquivalentTo(expected);
        }   
    }
}
