using System.Text.Json;

namespace Epicweb.Optimizely.ContentDelivery.Sync.Models
{

    public class SearchResultModel
    {
        public int totalMatching { get; set; }
        public GenericPageModel[] results { get; set; }
    }

    //"changed": "2020-04-09T09:06:02Z",
    //"created": "2014-07-14T11:24:44Z",
    //"startPublish": "2014-07-14T11:24:44Z",
    //"stopPublish": null,
    //"saved": "2020-04-09T09:06:02Z",
    //"status": "Published",

    public class GenericPageModel
    {
        public IEnumerable<ContentDeliveryProp> properties;

        public Contentlink contentLink { get; set; }
        public string name { get; set; }
        public Language language { get; set; }
        public Language masterLanguage { get; set; }
        public Language[] existingLanguages { get; set; }
        /// <summary>
        /// list of inherited models
        /// </summary>
        public string[] contentType { get; set; }
        public Parentlink parentLink { get; set; }
        public string url { get; set; }
        public string routeSegment { get; set; }
        /// <summary>
        /// Published
        /// </summary>
        public string status { get; set; }
        public DateTime? saved { get; set; }
        public DateTime? stopPublish { get; set; }
        public DateTime? startPublish { get; set; }
        public DateTime? created { get; set; }
        public DateTime? changed { get; set; }
    }

   
    public class Contentlink
    {
        public int id { get; set; }
        public int workId { get; set; }
        public string guidValue { get; set; }
        public object providerName { get; set; }
        public string url { get; set; }
        public object expanded { get; set; }
    }

    public class Language
    {
        public string link { get; set; }
        public string displayName { get; set; }
        public string name { get; set; }
    }

    public class Masterlanguage
    {
        public string link { get; set; }
        public string displayName { get; set; }
        public string name { get; set; }
    }

    public class Parentlink
    {
        public int id { get; set; }
        public int workId { get; set; }
        public string guidValue { get; set; }
        public string providerName { get; set; }
        public string url { get; set; }
        public object expanded { get; set; }
    } 
    
    public class ContentDeliveryProp
    {
        public string name { get; set; }
        public JsonElement value { get; set; }
        public string propertyDataType { get; set; }
    }

    public class StringProp
    {
        public string value { get; set; }
        public string propertyDataType { get; set; }
    }

    public class ContentareaProp
    {
        public Value[] value { get; set; }
        public string propertyDataType { get; set; }
    }

    public class Value
    {
        public string displayOption { get; set; }
        public Contentlink contentLink { get; set; }
        public object tag { get; set; }
    }



    public class FloatProp
    {
        public float? value { get; set; }
        public string propertyDataType { get; set; }
    }

    public class BoolProp
    {
        public bool? value { get; set; }
        public string propertyDataType { get; set; }
    }

    public class Category
    {
        public int[] value { get; set; }
        public string propertyDataType { get; set; }
    }


    public class Existinglanguage
    {
        public string link { get; set; }
        public string displayName { get; set; }
        public string name { get; set; }
    }

}