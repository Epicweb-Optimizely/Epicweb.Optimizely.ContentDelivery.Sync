using System.Text.Json;
using System.Web;

namespace Epicweb.Optimizely.ContentDelivery.Sync
{
    public class ImportApiClient : BearerApiClient, IImportApiClient
    {
        public readonly string SearchApiPath = "/api/episerver/v2.0/search/content/";
        public readonly string ContentApiPath = "/api/episerver/v2.0/content/";

        public ImportApiClient(string accesstoken, string apiurl): base(accesstoken, apiurl)
        {
        }

        public async Task<JsonDocument> GetAllPageTypesAsync(string[] pagetypes)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            string filter = null;
            //?query=&filter=contentType eq 'PrisonPage' or contentType eq 'CustodyPage'
            foreach (var type in pagetypes)
            {
                if (!string.IsNullOrEmpty(filter))
                    filter += " or ";
                filter += $"contentType eq '{type}'";
            }
            if (filter != null)
                queryString["filter"] = $"({filter}) and hideFromSearch/value ne true";

            var response = await _httpClient.GetAsync($"{SearchApiPath}?{queryString}");
            await ThrowIfError(response);
            using (var content = response.Content)
            {
                return JsonDocument.Parse(content.ReadAsStringAsync().Result);
            }
        }

        public async Task<JsonDocument> GetSinglePageByIdAsync(string id)
        {
            var response = await _httpClient.GetAsync($"{ContentApiPath}{id}");
            await ThrowIfError(response);
            using (var content = response.Content)
            {
                return JsonDocument.Parse(content.ReadAsStringAsync().Result);
            }
        }
    }
}
