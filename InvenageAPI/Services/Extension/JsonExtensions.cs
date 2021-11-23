using Newtonsoft.Json;

namespace InvenageAPI.Services.Extension
{
    public static class JsonExtensions
    {
        public static string ToJson<T>(this T data)
            => JsonConvert.SerializeObject(data, DefaultJsonSerializerSettings());
        public static T FromJson<T>(this string data)
            => data == null ? default : JsonConvert.DeserializeObject<T>(data, DefaultJsonSerializerSettings());

        public static JsonSerializerSettings DefaultJsonSerializerSettings()
            => new()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
    }
}
