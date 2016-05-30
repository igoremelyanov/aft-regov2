using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FakeUGS.Core.Providers
{
    public interface IJsonSerializationProvider
    {
        T DeserializeFromString<T>(string value);
        dynamic DeserializeAsDynamic(string value);
        string SerializeToString<T>(T obj);
    }
    public sealed class JsonSerializationProvider : IJsonSerializationProvider
    {
        T IJsonSerializationProvider.DeserializeFromString<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
        dynamic IJsonSerializationProvider.DeserializeAsDynamic(string value)
        {
            return JObject.Parse(value);
        }
        string IJsonSerializationProvider.SerializeToString<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}