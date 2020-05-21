using System;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

using Moq;
using Moq.Protected;
using System.IO;
using System.Net.Mime;

namespace Booth.PortfolioManager.DataServices.Test
{
    public static class MockHttpMessageHandlerExtension
    {

        public static Mock<HttpMessageHandler> CreateMessageHandler(this MockRepository mockRepository, HttpResponseMessage responseMessage)
        {
            return CreateMessageHandler(mockRepository, responseMessage, x => { });
        }

        public static Mock<HttpMessageHandler> CreateMessageHandler(this MockRepository mockRepository, HttpResponseMessage responseMessage, Action<HttpRequestMessage> requestMessageAction)
        {
            var httpHandler = mockRepository.Create<HttpMessageHandler>();
            httpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage)
                .Callback<HttpRequestMessage, CancellationToken>((m, c) => requestMessageAction(m));

            return httpHandler;
        }

        public static Mock<HttpMessageHandler> CreateJsonMessageHandler(this MockRepository mockRepository, string jsonContent)
        {
            return CreateJsonMessageHandler(mockRepository, jsonContent, x => { });
        }

        public static Mock<HttpMessageHandler> CreateJsonMessageHandler(this MockRepository mockRepository, string jsonContent, Action<HttpRequestMessage> requestMessageAction)
        {
            var content = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            return CreateMessageHandler(mockRepository, content, requestMessageAction);
        }

        public static Mock<HttpMessageHandler> CreateFileContentMessageHandler(this MockRepository mockRepository, string fileName, string mediaType, Action<HttpRequestMessage> requestMessageAction)
        {

            var content = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(File.ReadAllText(fileName), Encoding.UTF8, mediaType)
            };

            return CreateMessageHandler(mockRepository, content, requestMessageAction);
        }

        public static Mock<HttpMessageHandler> CreateHttpStatusMessageHandler(this MockRepository mockRepository, HttpStatusCode httpStatusCode)
        {
            return CreateHttpStatusMessageHandler(mockRepository, httpStatusCode, x => { });
        }

        public static Mock<HttpMessageHandler> CreateHttpStatusMessageHandler(this MockRepository mockRepository, HttpStatusCode httpStatusCode, Action<HttpRequestMessage> requestMessageAction)
        {
            var content = new HttpResponseMessage()
            {
                StatusCode = httpStatusCode,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            return CreateMessageHandler(mockRepository, content, requestMessageAction);
        }
    }
}
