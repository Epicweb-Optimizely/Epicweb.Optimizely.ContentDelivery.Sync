using EPiServer.Core;
using Epicweb.Optimizely.ContentDelivery.Sync.Models;
using System.Reflection;
using System.Text.Json;

namespace Epicweb.Optimizely.ContentDelivery.Sync
{
    public class ContentDeliveryMapper
    {
        private readonly IImportApiClient _importApiClient;

        public ContentDeliveryMapper(IImportApiClient importApiClient)
        {
            _importApiClient = importApiClient;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageModel"></param>
        /// <param name="methodSpecialSerialization">for serialization of IList< CustomType >,</param>
        /// <param name=""></param>
        /// <returns></returns>
        public PageData Map(PageData page, GenericPageModel pageModel, Func<string, JsonElement, object> methodSpecialSerialization = null)
        {
            if (page.Name != pageModel.name)
                page.Name = pageModel.name;
            if (page.URLSegment != pageModel.routeSegment)
                page.URLSegment = pageModel.routeSegment;

            foreach (var prop in pageModel.properties)
            {
                var pageProp = MapProp(prop, methodSpecialSerialization);
                if (pageProp != null && page.Property.Keys.Contains(prop.name))
                {
                    object value = page.Property[prop.name].Value;

                    (bool hasOtherValue, string ReturnedValue) = IsPropContentIdAndShouldGetValue(page, prop.name);

                    if (hasOtherValue)
                    {
                        pageProp = ReturnedValue;
                    }

                    if (IsPropChanged(value, pageProp, prop.value.ValueKind))
                        page.Property[prop.name].Value = pageProp;
                }
            }
            return page;
        }

        public (bool hasOtherValue, string ReturnedValue) IsPropContentIdAndShouldGetValue(PageData page, string propname)
        {
            const BindingFlags memberInfoBinding = BindingFlags.Public | BindingFlags.Instance;
            bool hasOtherValue = false;
            string ReturnedValue = null;
            propname = propname.FirstCharToUpper();
            var member = page.GetType().GetMember(propname, memberInfoBinding).FirstOrDefault();
            if (member != null) {               
                var syncAttrib = member.GetCustomAttributes<SyncPageNameAttribute>();
                if (syncAttrib.Any())
                {
                    if (member.Name == propname)
                    {
                        string listOfIds = GetValue(page, member) as string;
                        string newVal = GetNewValue(listOfIds);
                        return (true, newVal);
                    }
                }
            }

            return (hasOtherValue, ReturnedValue);
        }

        private string GetNewValue(string listOfIds)
        {
            if (listOfIds == null)
                return null;
            string[] arr = listOfIds.Split(",".ToCharArray());
            arr = GetValues(arr).ToArray();
            return string.Join(",", arr);
        }

        private IEnumerable<string> GetValues(string[] arr)
        {
            foreach (var value in arr)
            {
                var returnValue = value;
                if (int.TryParse(value, out int PageId))
                {
                    try
                    {
                        var jsonDoc = _importApiClient.GetSinglePageByIdAsync(PageId + "").Result;
                        SerializeService serializeService = new SerializeService(_importApiClient);
                        GenericPageModel pageModel = serializeService.SerializeToPageModel(jsonDoc.RootElement);
                        returnValue = pageModel.name;
                    }
                    catch
                    {
                        //the id does not exists or site not accessible
                    }
                }

                yield return returnValue;
            }
        }

        private string GetValue(PageData page, MemberInfo memberInfo)
        {

            switch (memberInfo)
            {
                case FieldInfo fieldInfo:
                    return fieldInfo.GetValue(page) as string;
                case PropertyInfo propertyInfo:
                    return propertyInfo.GetValue(page) as string;
            }

            if (!(page[memberInfo.Name] is string value))
            {
                return null;
            }
            return value;
        }

        //this one is slow, the array string comparision has poor performance
        private bool IsPropChanged(object value, object pageProp, JsonValueKind valueKind)
        {
            if (value == null && pageProp == string.Empty)
                return false;

            if (value == null && value != pageProp)
                return true;

            switch (valueKind)
            {
                case JsonValueKind.String:
                    return value?.ToString().Equals(pageProp?.ToString()) == false;
                case JsonValueKind.Number:
                    return value?.ToString().Equals(pageProp?.ToString()) == false;
                case JsonValueKind.True:
                    return value?.ToString().Equals(pageProp?.ToString()) == false;
                case JsonValueKind.False:
                    return value?.ToString().Equals(pageProp?.ToString()) == false;
                case JsonValueKind.Undefined:
                    throw new NotSupportedException();
                case JsonValueKind.Object:
                    return value?.Equals(pageProp) == false;
                case JsonValueKind.Array:
                    return string.Join("|", value as IEnumerable<object>) == string.Join("|", pageProp as IEnumerable<object>) == false;
                case JsonValueKind.Null:
                    throw new NotSupportedException();
                default:
                    return value?.Equals(pageProp) == false;
            }
        }

        private object MapProp(ContentDeliveryProp prop, Func<string, JsonElement, object> methodSpecialSerialization)
        {

            var typevalue = prop.value.JsonElementToTypedValue();

            switch (prop.propertyDataType)
            {
                case "PropertyLongString":
                    return typevalue;
                case "PropertyString":
                    return typevalue;
                case "PropertyFloatNumber":
                    return typevalue;
                case "PropertyXhtmlString":
                    return typevalue;
                case "PropertyBoolean":
                    if (prop.value.ToString() == "true")
                        return true;
                    if (prop.value.ToString() == "false")
                        return false;
                    break;
                default:
                    break;
            }

            if (methodSpecialSerialization != null)
            {
                return methodSpecialSerialization(prop.propertyDataType, prop.value);
            }

            return null;
        }

        
       
    }

    public static class JsonElementExtensions
    {
        public static Type ValueKindToType(this JsonValueKind valueKind, string value)
        {
            switch (valueKind)
            {
                case JsonValueKind.String:
                    return typeof(System.String);
                case JsonValueKind.Number:
                    if (Int64.TryParse(value, out Int64 intValue))
                    {
                        return typeof(System.Int64);
                    }
                    else
                    {
                        return typeof(System.Double);
                    }
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return typeof(System.Boolean);
                case JsonValueKind.Undefined:
                    throw new NotSupportedException();
                case JsonValueKind.Object:
                    return typeof(System.Object);
                case JsonValueKind.Array:
                    return typeof(System.Array);
                case JsonValueKind.Null:
                    throw new NotSupportedException();
                default:
                    return typeof(System.Object);
            }
        }

        public static object JsonElementToTypedValue(this JsonElement jsonElement)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    return null;
                case JsonValueKind.String:
                    if (jsonElement.TryGetGuid(out Guid guidValue))
                    {
                        return guidValue;
                    }
                    else
                    {
                        if (jsonElement.TryGetDateTime(out DateTime datetime))
                        {
                            if (datetime.Kind == DateTimeKind.Local)
                            {
                                if (jsonElement.TryGetDateTimeOffset(out DateTimeOffset datetimeOffset))
                                {
                                    return datetimeOffset;
                                }
                            }
                            return datetime;
                        }
                        return jsonElement.ToString();
                    }
                case JsonValueKind.Number:
                    if (jsonElement.TryGetInt64(out long longValue))
                    {
                        return longValue;
                    }
                    else
                    {
                        return jsonElement.GetDouble();
                    }
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return jsonElement.GetBoolean();
                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                    return null;
                default:
                    return jsonElement.ToString();
            }
        }


    }

    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }
    }
}