using System;

namespace Epicweb.Optimizely.ContentDelivery.Sync
{
    /// <summary>
    /// Will be printed on the product page belowe the "Spesifikasjoner" paragraph
    /// </summary>
    public class SyncPageNameAttribute : Attribute
    {
        public SyncPageNameAttribute()
        {
            SortOrder = 100;
        }

        /// <summary>
        /// Default sortorder is 100
        /// </summary>
        public int SortOrder { get; set; }
    }
}