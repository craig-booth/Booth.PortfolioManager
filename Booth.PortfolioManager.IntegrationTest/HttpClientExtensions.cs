using Booth.PortfolioManager.Web.Models.User;
using Booth.PortfolioManager.Web.Serialization;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Booth.PortfolioManager.IntegrationTest
{

    internal class RestApiException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }
        public RestApiException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }


    static internal class HttpClientExtensions
    {

        static public async Task<bool> AuthenticateAsync(this HttpClient httpClient, string user, string password, CancellationToken cancellationToken)
        {
            var authenticationRquest = new AuthenticationRequest
            {
                UserName = user,
                Password = password
            };
            var httpResponse = await httpClient.PostAsJsonAsync<AuthenticationRequest>("https://integrationtest.com/api/authenticate", authenticationRquest, cancellationToken);
            if (!httpResponse.IsSuccessStatusCode)
                return false;

            var contentStream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
            using (var streamReader = new StreamReader(contentStream))
            {
                var serializer = new ModelSerializer();
                var authenticationResponse = serializer.Deserialize<AuthenticationResponse>(streamReader);

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResponse.Token);   
            }
            return true;
        }

        static public async Task<T> GetAsync<T>(this HttpClient httpClient, string url, CancellationToken cancellationToken)
        {
            var httpResponse = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            if (!httpResponse.IsSuccessStatusCode)
                throw new RestApiException(httpResponse.StatusCode, httpResponse.ReasonPhrase);

            if (!((httpResponse.Content is object) && (httpResponse.Content.Headers.ContentType.MediaType == "application/json")))
                throw new RestApiException(HttpStatusCode.UnsupportedMediaType, "Content Type is not application/json");

            var contentStream = await httpResponse.Content.ReadAsStreamAsync();

            using (var streamReader = new StreamReader(contentStream))
            {
                var serializer = new ModelSerializer();
                var result = serializer.Deserialize<T>(streamReader);

                return result;
            }
        }

        static public async Task PostAsync<D>(this HttpClient httpClient, string url, D data, CancellationToken cancellationToken)
        {
            var contentStream = new MemoryStream();

            using (var streamWriter = new StreamWriter(contentStream))
            {
                var serializer = new ModelSerializer();

                serializer.Serialize<D>(streamWriter, data);
                streamWriter.Flush();
                contentStream.Position = 0;

                var content = new StreamContent(contentStream);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var httpResponse = await httpClient.PostAsync(url, content);
                if (!httpResponse.IsSuccessStatusCode)
                    throw new RestApiException(httpResponse.StatusCode, httpResponse.ReasonPhrase);
            }
        }

        static public async Task<T> PostAsync<T, D>(this HttpClient httpClient, string url, D data, CancellationToken cancellationToken)
        {
            var contentStream = new MemoryStream();
            var content = new StreamContent(contentStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using (var streamWriter = new StreamWriter(contentStream))
            {
                var serializer = new ModelSerializer();

                serializer.Serialize<D>(streamWriter, data);
                streamWriter.Flush();
                contentStream.Position = 0;

                var httpResponse = await httpClient.PostAsync(url, content);
                if (!httpResponse.IsSuccessStatusCode)
                    throw new RestApiException(httpResponse.StatusCode, httpResponse.ReasonPhrase);

                if (!((httpResponse.Content is object) && (httpResponse.Content.Headers.ContentType.MediaType == "application/json")))
                    throw new RestApiException(HttpStatusCode.UnsupportedMediaType, "Content Type is not application/json");

                var responseStream = await httpResponse.Content.ReadAsStreamAsync();
                using (var streamReader = new StreamReader(responseStream))
                {
                    var result = serializer.Deserialize<T>(streamReader);
                    return result;
                }
            }
        }

        static public async Task DeleteAsync(this HttpClient httpClient, string url, CancellationToken cancellationToken)
        {
            var httpResponse = await httpClient.DeleteAsync(url, cancellationToken);
            if (!httpResponse.IsSuccessStatusCode)
                throw new RestApiException(httpResponse.StatusCode, httpResponse.ReasonPhrase);

        }

    }
}
