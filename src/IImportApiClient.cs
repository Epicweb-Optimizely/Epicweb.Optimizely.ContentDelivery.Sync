using System.Text.Json;

namespace Epicweb.Optimizely.ContentDelivery.Sync
{
    public interface IImportApiClient
    {
        Task<JsonDocument> GetAllPageTypesAsync(string[] pagetypes);
        Task<JsonDocument> GetSinglePageByIdAsync(string id);
    }
}