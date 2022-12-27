using Epicweb.Optimizely.ContentDelivery.Sync.Models;
using System.Text.Json;
using System.Web;

namespace Epicweb.Optimizely.ContentDelivery.Sync
{
    public class SerializeService : ISerializeService
    {
        protected readonly IImportApiClient _importApiClient;

        public SerializeService(IImportApiClient importApiClient)
        {
            _importApiClient = importApiClient;
        }
        /// <summary>
        /// Redo the properties from ContentDelivery to a propertyList
        /// </summary>
        /// <param name="page"></param>
        /// <param name="propsToSync"></param>
        /// <returns></returns>
        public IEnumerable<ContentDeliveryProp> GetPropertyList(JsonElement page, string[]? propsToSync = null)
        {
            bool useFilter = propsToSync != null && propsToSync?.Length > 0;

            foreach (var prop in page.EnumerateObject())
            {
                if ((!useFilter || propsToSync.Contains(prop.Name.ToLower())))
                {
                    ContentDeliveryProp deliveryProp = null;
                    try
                    {
                        deliveryProp = JsonSerializer.Deserialize<ContentDeliveryProp>(prop.Value.ToString());
                    }
                    catch { } //will only work for Optimizely "properties"

                    if (deliveryProp != null && !string.IsNullOrEmpty(deliveryProp.propertyDataType))
                    {
                        deliveryProp.name = prop.Name;
                        yield return deliveryProp;
                    }
                }
            }
        }

        public GenericPageModel SerializeToPageModel(JsonElement page, string[]? propsToSync = null)
        {
            var model = page.Deserialize<GenericPageModel>();
            model.properties = GetPropertyList(page, propsToSync).ToList();
            return model;
        }
        /// <summary>
        /// Returns PageModelDtos from other site
        /// </summary>
        /// <param name="pagetypes"></param>
        /// <param name="propsToSync"></param>
        /// <returns></returns>
        public IEnumerable<GenericPageModel> GetPageModels(string[] pagetypes, string[]? propsToSync = null)
        {
            var jsonDoc = _importApiClient.GetAllPageTypesAsync(pagetypes).Result;
            var pagesJsonElement = jsonDoc.RootElement.GetProperty("results");

            foreach (var page in pagesJsonElement.EnumerateArray())
            {
                yield return SerializeToPageModel(page, propsToSync);
            }
        }
    }
}
