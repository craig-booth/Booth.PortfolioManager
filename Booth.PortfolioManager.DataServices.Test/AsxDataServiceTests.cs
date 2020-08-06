using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Http;
using System.Linq;

using Xunit;
using FluentAssertions;
using Moq;
using Moq.Protected;

using Booth.Common;
using Booth.PortfolioManager.Domain.TradingCalendars;

namespace Booth.PortfolioManager.DataServices.Test
{
    public class AsxDataServiceTests
    {

        [Fact]
        public async void GetSinglePrice()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var json = "{\"code\":\"BHP\",\"isin_code\":\"AU000000BHP4\",\"desc_full\":\"Ordinary Fully Paid\",\"last_price\":34.72,\"open_price\":34.38,\"day_high_price\":34.88,\"day_low_price\":34.25,\"change_price\":-0.33,\"change_in_percent\":\" - 0.942 % \",\"volume\":6214544,\"bid_price\":34.72,\"offer_price\":34.74,\"previous_close_price\":35.05,\"previous_day_percentage_change\":\"5.891 % \",\"year_high_price\":42.33,\"last_trade_date\":\"2020-05-20T00:00:00+1000\",\"year_high_date\":\"2019-07-03T00:00:00+1000\",\"year_low_price\":24.05,\"year_low_date\":\"2020-03-13T00:00:00+1100\",\"year_open_price\":39.1,\"year_open_date\":\"2014-02-25T11:00:00+1100\",\"year_change_price\":-4.38,\"year_change_in_percentage\":\"-11.202 % \",\"pe\":13.2,\"eps\":2.6547,\"average_daily_volume\":11644628,\"annual_dividend_yield\":6.08,\"market_cap\":103252091430,\"number_of_shares\":2945851396,\"deprecated_market_cap\":102279960000,\"deprecated_number_of_shares\":2945851394,\"suspended\":false}";

            HttpRequestMessage requestMessage = null;
            var messageHandler = mockRepository.CreateJsonMessageHandler(json, x => requestMessage = x);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AsxDataService(httpClient);

            var result = await dataService.GetSinglePrice("BHP", CancellationToken.None);

            requestMessage.Method.Should().Be(HttpMethod.Get);
            requestMessage.RequestUri.AbsoluteUri.Should().Be("https://www.asx.com.au/asx/1/share/BHP");
            result.Should().BeEquivalentTo(new StockPrice("BHP", new Date(2020, 05, 20), 34.72m));
            
        }

        [Fact]
        public async void GetSinglePriceUnexpectedJson()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var json = "{\"field\": \"value\" }";

            var messageHandler = mockRepository.CreateJsonMessageHandler(json);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AsxDataService(httpClient);

            var result = await dataService.GetSinglePrice("IBM", CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async void GetSinglePriceInvalidJson()
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
        public async void GetSinglePriceHttpError()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var messageHandler = mockRepository.CreateHttpStatusMessageHandler(HttpStatusCode.NotFound);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AsxDataService(httpClient);

            var result = await dataService.GetSinglePrice("IBM", CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async void GetMultiplePrices()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var json = "{\"code\":\"BHP\",\"isin_code\":\"AU000000BHP4\",\"desc_full\":\"Ordinary Fully Paid\",\"last_price\":34.72,\"open_price\":34.38,\"day_high_price\":34.88,\"day_low_price\":34.25,\"change_price\":-0.33,\"change_in_percent\":\" - 0.942 % \",\"volume\":6214544,\"bid_price\":34.72,\"offer_price\":34.74,\"previous_close_price\":35.05,\"previous_day_percentage_change\":\"5.891 % \",\"year_high_price\":42.33,\"last_trade_date\":\"2020-05-20T00:00:00+1000\",\"year_high_date\":\"2019-07-03T00:00:00+1000\",\"year_low_price\":24.05,\"year_low_date\":\"2020-03-13T00:00:00+1100\",\"year_open_price\":39.1,\"year_open_date\":\"2014-02-25T11:00:00+1100\",\"year_change_price\":-4.38,\"year_change_in_percentage\":\"-11.202 % \",\"pe\":13.2,\"eps\":2.6547,\"average_daily_volume\":11644628,\"annual_dividend_yield\":6.08,\"market_cap\":103252091430,\"number_of_shares\":2945851396,\"deprecated_market_cap\":102279960000,\"deprecated_number_of_shares\":2945851394,\"suspended\":false}";

            var requestMessage = new List<HttpRequestMessage>(); ;
            var messageHandler = mockRepository.CreateJsonMessageHandler(json, x => requestMessage.Add(x));

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AsxDataService(httpClient);

            var result = await dataService.GetMultiplePrices(new string[] { "BHP", "COH" }, CancellationToken.None);

            requestMessage[0].Method.Should().Be(HttpMethod.Get);
            requestMessage[0].RequestUri.AbsoluteUri.Should().StartWith("https://www.asx.com.au/asx/1/share/BHP");

            requestMessage[1].Method.Should().Be(HttpMethod.Get);
            requestMessage[1].RequestUri.AbsoluteUri.Should().StartWith("https://www.asx.com.au/asx/1/share/COH");
        }

        [Fact]
        public async void GetHistoricalPriceData()
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
        public async void GetHistoricalPriceDataUnexpectedJson()
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
        public async void GetHistoricalPriceDataInvalidJson()
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
        public async void GetHistoricalPriceDataHttpError()
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
        public async void GetNonTradingDays(int year, int expectedCount)
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            HttpRequestMessage requestMessage = null;
            var fileName = String.Format("asx-trading-calendar-{0}.htm", year);
            var messageHandler = mockRepository.CreateFileContentMessageHandler(fileName, "text/html", x => requestMessage = x);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AsxDataService(httpClient);

            var result = await dataService.GetNonTradingDays(year, CancellationToken.None);

            requestMessage.Method.Should().Be(HttpMethod.Get);
            requestMessage.RequestUri.AbsoluteUri.Should().Be("http://www.asx.com.au/about/" + fileName);
            result.Should().HaveCount(expectedCount);

            var newYearsDay = new Date(year, 01, 01);
            while (!newYearsDay.WeekDay())
                newYearsDay = newYearsDay.AddDays(1);

            result.First().Should().BeEquivalentTo(new NonTradingDay(newYearsDay, "New Year's Day"));

        }

        [Fact]
        public async void GetNonTradingDaysUnexpectedResponse()
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
        public async void GetNonTradingDaysHttpError()
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
