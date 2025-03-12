using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Http;
using System.Linq;

using Xunit;
using FluentAssertions;
using Moq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.TradingCalendars;

namespace Booth.PortfolioManager.DataServices.Test
{
    public class AsxDataServiceTests
    {

        [Fact]
        public async Task GetSinglePrice()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var json = "{\"data\": { \"dateListed\": \"1885-08-13\", \"displayName\": \"BHP GROUP LIMITED\", \"priceAsk\": 43.800000000000004, \"priceBid\": 43.79, \"priceChange\": 0.37000000000000455, \"priceChangePercent\": 0.8519456596822578, \"priceLast\": 43.800000000000004, \"sector\": \"Materials\", \"industryGroup\": \"Materials\", \"securityType\": 1, \"symbol\": \"BHP\", \"volume\": 6157139, \"xid\": \"60947\", \"marketCap\": 220359528595, \"statusCode\": \"\" }}";

            HttpRequestMessage requestMessage = null;
            var messageHandler = mockRepository.CreateJsonMessageHandler(json, x => requestMessage = x);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AsxDataService(httpClient);

            var result = await dataService.GetSinglePrice("BHP", CancellationToken.None);

            requestMessage.Method.Should().Be(HttpMethod.Get);
            requestMessage.RequestUri.AbsoluteUri.Should().Be("https://asx.api.markitdigital.com/asx-research/1.0/companies/BHP/header");
            result.Should().BeEquivalentTo(new StockPrice("BHP", Date.Today, 43.80m));
            
        }

        [Fact]
        public async Task GetSinglePriceUnexpectedJson()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var json = "{ \"error\": { \"code\": 400, \"message\": \"Bad Request: Symbol not found\", \"errors\": [ { \"message\": \"Bad Request: Symbol not found\"} ] } }";

            var messageHandler = mockRepository.CreateJsonMessageHandler(json);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AsxDataService(httpClient);

            var result = await dataService.GetSinglePrice("IBM", CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetSinglePriceInvalidJson()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var json = "xxxxx";

            var messageHandler = mockRepository.CreateJsonMessageHandler(json);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AsxDataService(httpClient);

            var result = await dataService.GetSinglePrice("IBM", CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetSinglePriceHttpError()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var messageHandler = mockRepository.CreateHttpStatusMessageHandler(HttpStatusCode.NotFound);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AsxDataService(httpClient);

            var result = await dataService.GetSinglePrice("IBM", CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetMultiplePrices()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var json = "{\"data\": { \"dateListed\": \"1885-08-13\", \"displayName\": \"BHP GROUP LIMITED\", \"priceAsk\": 43.800000000000004, \"priceBid\": 43.79, \"priceChange\": 0.37000000000000455, \"priceChangePercent\": 0.8519456596822578, \"priceLast\": 43.800000000000004, \"sector\": \"Materials\", \"industryGroup\": \"Materials\", \"securityType\": 1, \"symbol\": \"BHP\", \"volume\": 6157139, \"xid\": \"60947\", \"marketCap\": 220359528595, \"statusCode\": \"\" }}";

            var requestMessage = new List<HttpRequestMessage>(); ;
            var messageHandler = mockRepository.CreateJsonMessageHandler(json, x => requestMessage.Add(x));

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AsxDataService(httpClient);

            var result = await dataService.GetMultiplePrices(new string[] { "BHP", "COH" }, CancellationToken.None);

            requestMessage[0].Method.Should().Be(HttpMethod.Get);
            requestMessage[0].RequestUri.AbsoluteUri.Should().StartWith("https://asx.api.markitdigital.com/asx-research/1.0/companies/BHP/header");

            requestMessage[1].Method.Should().Be(HttpMethod.Get);
            requestMessage[1].RequestUri.AbsoluteUri.Should().StartWith("https://asx.api.markitdigital.com/asx-research/1.0/companies/COH/header");
        }

        [Fact]
        public async Task GetHistoricalPriceData()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var json = "{\"data\": ["
                   + "   {\"code\":\"BHP\",\"close_date\":\"2020-05-20T00:00:00+1000\",\"close_price\":34.05,\"change_price\":1.95,\"volume\":11664353,\"day_high_price\":35.11,\"day_low_price\":34.32,\"change_in_percent\":\"5.891 % \"},"
                   + "   {\"code\":\"BHP\",\"close_date\":\"2020-05-19T00:00:00+1000\",\"close_price\":35.05,\"change_price\":1.95,\"volume\":11664353,\"day_high_price\":35.11,\"day_low_price\":34.32,\"change_in_percent\":\"5.891 % \"},"
                   + "   {\"code\":\"BHP\",\"close_date\":\"2020-05-18T00:00:00+1000\",\"close_price\":33.1,\"change_price\":1.43,\"volume\":7894590,\"day_high_price\":33.2,\"day_low_price\":32.37,\"change_in_percent\":\"4.515 % \"}]}";

            HttpRequestMessage requestMessage = null;
            var messageHandler = mockRepository.CreateJsonMessageHandler(json, x => requestMessage = x);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AsxDataService(httpClient);

            var result = await dataService.GetHistoricalPriceData("BHP", new DateRange(new Date(2020, 05, 18), new Date(2020, 05, 19)), CancellationToken.None);

            requestMessage.Method.Should().Be(HttpMethod.Get);
            var days = (Date.Today - new Date(2020, 05, 18)).TotalDays;
            requestMessage.RequestUri.AbsoluteUri.Should().StartWith("https://www.asx.com.au/asx/1/share/BHP/prices?interval=daily&count=" + days.ToString());
            result.Should().BeEquivalentTo(new StockPrice[] { new StockPrice("BHP", new Date(2020, 05, 19), 35.05m), new StockPrice("BHP", new Date(2020, 05, 18), 33.10m) });

        }

        [Fact]
        public async Task GetHistoricalPriceDataUnexpectedJson()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var json = "{\"field\": \"value\" }";

            var messageHandler = mockRepository.CreateJsonMessageHandler(json);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AsxDataService(httpClient);

            var result = await dataService.GetHistoricalPriceData("IBM", new DateRange(new Date(2020, 05, 14), new Date(2020, 05, 15)), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetHistoricalPriceDataInvalidJson()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var json = "xxxxx";

            var messageHandler = mockRepository.CreateJsonMessageHandler(json);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AsxDataService(httpClient);

            var result = await dataService.GetHistoricalPriceData("IBM", new DateRange(new Date(2020, 05, 14), new Date(2020, 05, 15)), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetHistoricalPriceDataHttpError()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var messageHandler = mockRepository.CreateHttpStatusMessageHandler(HttpStatusCode.NotFound);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AsxDataService(httpClient);

            var result = await dataService.GetHistoricalPriceData("IBM", new DateRange(new Date(2020, 05, 14), new Date(2020, 05, 15)), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData(new object[] { 2015, 8 })]
        [InlineData(new object[] { 2016, 8 })]
        [InlineData(new object[] { 2017, 8 })]
        [InlineData(new object[] { 2018, 8 })]
        [InlineData(new object[] { 2019, 8 })]
        [InlineData(new object[] { 2020, 8 })]
        [InlineData(new object[] { 2021, 8 })]
        [InlineData(new object[] { 2022, 8 })]
        [InlineData(new object[] { 2023, 8 })]
        [InlineData(new object[] { 2024, 8 })]
        [InlineData(new object[] { 2025, 8 })]
        public async Task GetNonTradingDays(int year, int expectedCount)
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            HttpRequestMessage requestMessage = null;
            var fileName = String.Format("asx-trading-calendar-{0}.htm", year);
            var messageHandler = mockRepository.CreateFileContentMessageHandler(fileName, "text/html", x => requestMessage = x);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AsxDataService(httpClient);

            var result = await dataService.GetNonTradingDays(year, CancellationToken.None);

            requestMessage.Method.Should().Be(HttpMethod.Get);
            if (year <= 2020)
                requestMessage.RequestUri.AbsoluteUri.Should().Be("http://www.asx.com.au/about/" + fileName);
            else
                requestMessage.RequestUri.AbsoluteUri.Should().Be("https://www2.asx.com.au/markets/market-resources/trading-hours-calendar/cash-market-trading-hours/trading-calendar");
            result.Should().HaveCount(expectedCount);

            result.Should().AllSatisfy(x => x.Date.Year.Should().Be(year));

            var newYearsDay = new Date(year, 01, 01);
            while (!newYearsDay.WeekDay())
                newYearsDay = newYearsDay.AddDays(1);

            result.First().Should().BeEquivalentTo(new NonTradingDay(newYearsDay, "New Year's Day"));

        }

        [Fact]
        public async Task GetNonTradingDaysUnexpectedResponse()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var json = "{ \"Meta Data2\": " +
                       "   { \"1. Information\": \"Daily Prices (open, high, low, close) and Volumes\"," +
                       "     \"2. Symbol\": \"IBM\"," +
                       "     \"3. Last Refreshed\": \"2020-05-15\"," +
                       "     \"4. Output Size\": \"Compact\"," +
                       "     \"5. Time Zone\": \"US/Eastern\"" +
                       "   }" +
                       "}";

            var messageHandler = mockRepository.CreateJsonMessageHandler(json);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AsxDataService(httpClient);

            var result = await dataService.GetNonTradingDays(2020, CancellationToken.None);

            result.Should().BeEmpty();
        }


        [Fact]
        public async Task GetNonTradingDaysHttpError()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var messageHandler = mockRepository.CreateHttpStatusMessageHandler(HttpStatusCode.NotFound);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AsxDataService(httpClient);

            var result = await dataService.GetNonTradingDays(2020, CancellationToken.None);

            result.Should().BeEmpty();
        }

    }
}
