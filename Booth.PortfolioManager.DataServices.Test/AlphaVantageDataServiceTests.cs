using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Http;

using Xunit;
using FluentAssertions;
using Moq;
using System.Threading.Tasks;

using Booth.Common;


namespace Booth.PortfolioManager.DataServices.Test
{
    public class AlphaVantageDataServiceTests
    {

        [Fact]
        public async Task GetSinglePrice()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var json = "{ \"Meta Data\": " +
                       "   { \"1. Information\": \"Daily Prices (open, high, low, close) and Volumes\"," +
                       "     \"2. Symbol\": \"IBM\"," +
                       "     \"3. Last Refreshed\": \"2020-05-15\"," +
                       "     \"4. Output Size\": \"Compact\"," +
                       "     \"5. Time Zone\": \"US/Eastern\"" +
                       "   }," +
                       " \"Time Series (Daily)\": " +
                       "   { \"2020-05-15\": " +
                       "       { \"1. open\": \"115.9300\"," +
                       "         \"2. high\": \"117.3900\"," +
                       "         \"3. low\": \"115.2500\"," +
                       "         \"4. close\": \"116.9800\"," +
                       "         \"5. volume\": \"4785773\"" +
                       "       }," +
                       "     \"2020-05-14\": " +
                       "       { \"1. open\": \"114.5700\"," +
                       "         \"2. high\": \"117.0900\"," +
                       "         \"3. low\": \"111.8100\"," +
                       "         \"4. close\": \"116.9500\"," +
                       "         \"5. volume\": \"5255607\"" +
                       "       }" +
                       "   }" +
                       "}";

            HttpRequestMessage requestMessage = null;
            var messageHandler = mockRepository.CreateJsonMessageHandler(json, x => requestMessage = x);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AlphaVantageDataService(httpClient);

            var result = await dataService.GetSinglePrice("IBM", CancellationToken.None);

            requestMessage.Method.Should().Be(HttpMethod.Get);
            requestMessage.RequestUri.AbsoluteUri.Should().StartWith("https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol=IBM.AX&apikey=");
            result.Should().BeEquivalentTo(new StockPrice("IBM", new Date(2020, 05, 15), 116.98m));
            
        }

        [Fact]
        public async Task GetSinglePriceUnexpectedJson()
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
            var dataService = new AlphaVantageDataService(httpClient);

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
            var dataService = new AlphaVantageDataService(httpClient);

            var result = await dataService.GetSinglePrice("IBM", CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetSinglePriceHttpError()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var messageHandler = mockRepository.CreateHttpStatusMessageHandler(HttpStatusCode.NotFound);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AlphaVantageDataService(httpClient);

            var result = await dataService.GetSinglePrice("IBM", CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetMultiplePrices()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var json = "{ \"Meta Data\": " +
                       "   { \"1. Information\": \"Daily Prices (open, high, low, close) and Volumes\"," +
                       "     \"2. Symbol\": \"IBM\"," +
                       "     \"3. Last Refreshed\": \"2020-05-15\"," +
                       "     \"4. Output Size\": \"Compact\"," +
                       "     \"5. Time Zone\": \"US/Eastern\"" +
                       "   }," +
                       " \"Time Series (Daily)\": " +
                       "   { \"2020-05-15\": " +
                       "       { \"1. open\": \"115.9300\"," +
                       "         \"2. high\": \"117.3900\"," +
                       "         \"3. low\": \"115.2500\"," +
                       "         \"4. close\": \"116.9800\"," +
                       "         \"5. volume\": \"4785773\"" +
                       "       }," +
                       "     \"2020-05-14\": " +
                       "       { \"1. open\": \"114.5700\"," +
                       "         \"2. high\": \"117.0900\"," +
                       "         \"3. low\": \"111.8100\"," +
                       "         \"4. close\": \"116.9500\"," +
                       "         \"5. volume\": \"5255607\"" +
                       "       }" +
                       "   }" +
                       "}";

            var requestMessage = new List<HttpRequestMessage>(); ;
            var messageHandler = mockRepository.CreateJsonMessageHandler(json, x => requestMessage.Add(x));

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AlphaVantageDataService(httpClient);

            var result = await dataService.GetMultiplePrices(new string[] { "IBM", "ABC" }, CancellationToken.None);

            requestMessage[0].Method.Should().Be(HttpMethod.Get);
            requestMessage[0].RequestUri.AbsoluteUri.Should().StartWith("https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol=IBM.AX&apikey=");

            requestMessage[1].Method.Should().Be(HttpMethod.Get);
            requestMessage[1].RequestUri.AbsoluteUri.Should().StartWith("https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol=ABC.AX&apikey=");
        }

        [Fact]
        public async Task GetHistoricalPriceData()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var json = "{ \"Meta Data\": " +
                       "   { \"1. Information\": \"Daily Prices (open, high, low, close) and Volumes\"," +
                       "     \"2. Symbol\": \"IBM\"," +
                       "     \"3. Last Refreshed\": \"2020-05-15\"," +
                       "     \"4. Output Size\": \"Compact\"," +
                       "     \"5. Time Zone\": \"US/Eastern\"" +
                       "   }," +
                       " \"Time Series (Daily)\": " +
                       "   { \"2020-05-15\": " +
                       "       { \"1. open\": \"115.9300\"," +
                       "         \"2. high\": \"117.3900\"," +
                       "         \"3. low\": \"115.2500\"," +
                       "         \"4. close\": \"116.9800\"," +
                       "         \"5. volume\": \"4785773\"" +
                       "       }," +
                       "     \"2020-05-14\": " +
                       "       { \"1. open\": \"114.5700\"," +
                       "         \"2. high\": \"117.0900\"," +
                       "         \"3. low\": \"111.8100\"," +
                       "         \"4. close\": \"116.9500\"," +
                       "         \"5. volume\": \"5255607\"" +
                       "       }" +
                       "   }" +
                       "}";

            HttpRequestMessage requestMessage = null;
            var messageHandler = mockRepository.CreateJsonMessageHandler(json, x => requestMessage = x);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AlphaVantageDataService(httpClient);

            var result = await dataService.GetHistoricalPriceData("IBM", new DateRange(new Date(2020, 05, 14), new Date(2020, 05, 15)), CancellationToken.None);

            requestMessage.Method.Should().Be(HttpMethod.Get);
            requestMessage.RequestUri.AbsoluteUri.Should().StartWith("https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol=IBM.AX&apikey=");
            result.Should().BeEquivalentTo(new StockPrice[] { new StockPrice("IBM", new Date(2020, 05, 14), 116.95m), new StockPrice("IBM", new Date(2020, 05, 15), 116.98m) });

        }

        [Fact]
        public async Task GetHistoricalPriceDataUnexpectedJson()
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
            var dataService = new AlphaVantageDataService(httpClient);

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
            var dataService = new AlphaVantageDataService(httpClient);

            var result = await dataService.GetHistoricalPriceData("IBM", new DateRange(new Date(2020, 05, 14), new Date(2020, 05, 15)), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetHistoricalPriceDataHttpError()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var messageHandler = mockRepository.CreateHttpStatusMessageHandler(HttpStatusCode.NotFound);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new AlphaVantageDataService(httpClient);

            var result = await dataService.GetHistoricalPriceData("IBM", new DateRange(new Date(2020, 05, 14), new Date(2020, 05, 15)), CancellationToken.None);

            result.Should().BeEmpty();
        }


    }
}
