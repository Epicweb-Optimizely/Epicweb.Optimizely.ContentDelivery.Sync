using Epicweb.Optimizely.ContentDelivery.Sync.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;

namespace Epicweb.Optimizely.ContentDelivery.Sync
{
    public class BearerApiClient 
    {

        private readonly string _accessToken;
        private readonly string _environmentUrl;
        protected HttpClient _httpClient;

        public BearerApiClient(string accesstoken, string apiurl)
        {
            _accessToken = accesstoken;
            _environmentUrl = apiurl;
            _httpClient = GetClient();
        }


        /// <summary>
        /// Checks the HttpResponseMessage status code and throw an ApiException in case of non 2xx response.
        /// </summary>
        /// <param name="result">HttpResponseMessage instance</param>
        /// <returns></returns>
        /// <exception cref="ApiException">Throws in case of non 2xx response</exception>
        protected static async Task ThrowIfError(HttpResponseMessage result)
        {
            if (!result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                var errorMessage = new ErrorMessage();

                try
                {
                    errorMessage = JsonSerializer.Deserialize<ErrorMessage>(content);
                }
                catch (Exception)
                {
                    //errorMessage.error = new[] { content };
                }

                var logError = $"Error '{errorMessage.error.code}' when calling {result.RequestMessage.Method.ToString().ToUpperInvariant()} " +
                    $"{result.RequestMessage.RequestUri}";

                var error = new ApiException(logError,
                    result.StatusCode,
                    errorMessage,
                    null);

                throw error;
            }
        }

        private HttpClient GetClient()
        {
            if (_httpClient == null)
            {
                var handler = new HttpClientHandler();
                if (handler.SupportsAutomaticDecompression)
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }

                handler.UseCookies = false;

                _httpClient = new HttpClient(handler, true) { Timeout = TimeSpan.FromSeconds(600) };
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "ApiClient .net SDK");
                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _accessToken);
                _httpClient.DefaultRequestHeaders.ExpectContinue = false;
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.BaseAddress = new Uri(_environmentUrl);
            }
            return _httpClient;
        }
    }
}
