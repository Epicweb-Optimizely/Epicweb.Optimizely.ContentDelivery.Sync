using Epicweb.Optimizely.ContentDelivery.Sync.Models;
using System.Text.Json;

namespace Epicweb.Optimizely.ContentDelivery.Sync
{
    public interface ISerializeService
    {
        IEnumerable<GenericPageModel> GetPageModels(string[] pagetypes, string[]? propsToSync = null);
        IEnumerable<ContentDeliveryProp> GetPropertyList(JsonElement page, string[]? propsToSync = null);
        GenericPageModel SerializeToPageModel(JsonElement page, string[]? propsToSync = null);
    }
}