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
using System.Threading.Tasks;

namespace Booth.PortfolioManager.DataServices.Test
{

    public class YahooDataServiceTests
    {

        [Fact]
        public async Task GetHistoricalPriceData()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var json = "{\r\n    \"chart\": {\r\n        \"result\": [\r\n            {\r\n                \"meta\": {\r\n                    \"currency\": \"AUD\",\r\n                    \"symbol\": \"BHP.AX\",\r\n                    \"exchangeName\": \"ASX\",\r\n                    \"fullExchangeName\": \"ASX\",\r\n                    \"instrumentType\": \"EQUITY\",\r\n                    \"firstTradeDate\": 570409200,\r\n                    \"regularMarketTime\": 1728969025,\r\n                    \"hasPrePostMarketData\": false,\r\n                    \"gmtoffset\": 39600,\r\n                    \"timezone\": \"AEDT\",\r\n                    \"exchangeTimezoneName\": \"Australia/Sydney\",\r\n                    \"regularMarketPrice\": 44.01,\r\n                    \"fiftyTwoWeekHigh\": 44.39,\r\n                    \"fiftyTwoWeekLow\": 44.01,\r\n                    \"regularMarketDayHigh\": 44.39,\r\n                    \"regularMarketDayLow\": 44.01,\r\n                    \"regularMarketVolume\": 4791422,\r\n                    \"longName\": \"BHP Group Limited\",\r\n                    \"shortName\": \"BHP GROUP FPO [BHP]\",\r\n                    \"chartPreviousClose\": 43.79,\r\n                    \"priceHint\": 2,\r\n                    \"currentTradingPeriod\": {\r\n                        \"pre\": {\r\n                            \"timezone\": \"AEDT\",\r\n                            \"start\": 1728936000,\r\n                            \"end\": 1728946800,\r\n                            \"gmtoffset\": 39600\r\n                        },\r\n                        \"regular\": {\r\n                            \"timezone\": \"AEDT\",\r\n                            \"start\": 1728946800,\r\n                            \"end\": 1728969120,\r\n                            \"gmtoffset\": 39600\r\n                        },\r\n                        \"post\": {\r\n                            \"timezone\": \"AEDT\",\r\n                            \"start\": 1728969120,\r\n                            \"end\": 1728969120,\r\n                            \"gmtoffset\": 39600\r\n                        }\r\n                    },\r\n                    \"dataGranularity\": \"1d\",\r\n                    \"range\": \"5d\",\r\n                    \"validRanges\": [\r\n                        \"1d\",\r\n                        \"5d\",\r\n                        \"1mo\",\r\n                        \"3mo\",\r\n                        \"6mo\",\r\n                        \"1y\",\r\n                        \"2y\",\r\n                        \"5y\",\r\n                        \"10y\",\r\n                        \"ytd\",\r\n                        \"max\"\r\n                    ]\r\n                },\r\n                \"timestamp\": [\r\n                    1728428400,\r\n                    1728514800,\r\n                    1728601200,\r\n                    1728860400,\r\n                    1728969025\r\n                ],\r\n                \"indicators\": {\r\n                    \"quote\": [\r\n                        {\r\n                            \"volume\": [\r\n                                8605304,\r\n                                8307883,\r\n                                6467361,\r\n                                6157139,\r\n                                4791422\r\n                            ],\r\n                            \"close\": [\r\n                                43.279998779296875,\r\n                                43.900001525878906,\r\n                                43.43000030517578,\r\n                                43.79999923706055,\r\n                                44.0099983215332\r\n                            ],\r\n                            \"high\": [\r\n                                43.59000015258789,\r\n                                44.060001373291016,\r\n                                43.81999969482422,\r\n                                44.41999816894531,\r\n                                44.38999938964844\r\n                            ],\r\n                            \"low\": [\r\n                                42.79999923706055,\r\n                                43.25,\r\n                                43.43000030517578,\r\n                                43.33000183105469,\r\n                                44.0099983215332\r\n                            ],\r\n                            \"open\": [\r\n                                43.310001373291016,\r\n                                43.279998779296875,\r\n                                43.75,\r\n                                43.459999084472656,\r\n                                44.369998931884766\r\n                            ]\r\n                        }\r\n                    ],\r\n                    \"adjclose\": [\r\n                        {\r\n                            \"adjclose\": [\r\n                                43.279998779296875,\r\n                                43.900001525878906,\r\n                                43.43000030517578,\r\n                                43.79999923706055,\r\n                                44.0099983215332\r\n                            ]\r\n                        }\r\n                    ]\r\n                }\r\n            }\r\n        ],\r\n        \"error\": null\r\n    }\r\n}";

            HttpRequestMessage requestMessage = null;
            var messageHandler = mockRepository.CreateJsonMessageHandler(json, x => requestMessage = x);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new YahooDataService(httpClient);

            var result = await dataService.GetHistoricalPriceData("BHP", new DateRange(new Date(2024, 10, 11), new Date(2024, 10, 15)), CancellationToken.None);

            requestMessage.Method.Should().Be(HttpMethod.Get);
            requestMessage.RequestUri.AbsoluteUri.Should().StartWith("https://query1.finance.yahoo.com/v8/finance/chart/BHP.AX?interval=1d&range=");
            result.Should().BeEquivalentTo(new StockPrice[] { new StockPrice("BHP", new Date(2024, 10, 11), 43.43m), new StockPrice("BHP", new Date(2024, 10, 14), 43.80m), new StockPrice("BHP", new Date(2024, 10, 15), 44.01m) });

        }

        [Fact]
        public async Task GetHistoricalPriceDataUnexpectedJson()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var json = "{\r\n    \"chart\": {\r\n        \"result\": null,\r\n        \"error\": {\r\n            \"code\": \"Not Found\",\r\n            \"description\": \"No data found, symbol may be delisted\"\r\n        }\r\n    }\r\n}";

            var messageHandler = mockRepository.CreateJsonMessageHandler(json);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new YahooDataService(httpClient);

            var result = await dataService.GetHistoricalPriceData("BHP", new DateRange(new Date(2024, 10, 11), new Date(2024, 10, 15)), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetHistoricalPriceDataInvalidJson()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var json = "xxxxx";

            var messageHandler = mockRepository.CreateJsonMessageHandler(json);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new YahooDataService(httpClient);

            var result = await dataService.GetHistoricalPriceData("BHP", new DateRange(new Date(2024, 10, 11), new Date(2024, 10, 15)), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetHistoricalPriceDataHttpError()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var messageHandler = mockRepository.CreateHttpStatusMessageHandler(HttpStatusCode.NotFound);

            var httpClient = new HttpClient(messageHandler.Object);
            var dataService = new YahooDataService(httpClient);

            var result = await dataService.GetHistoricalPriceData("BHP", new DateRange(new Date(2024, 10, 11), new Date(2024, 10, 15)), CancellationToken.None);

            result.Should().BeEmpty();
        }

    }
}
