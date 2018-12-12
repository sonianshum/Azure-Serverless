namespace EventReportingService.Utilities
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    public static class Json
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters =
            {
                new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ" },
            },
        };

        public static string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value, Settings);
        }

        public static object DeserializeObject(string value)
        {
            return JsonConvert.DeserializeObject(value, Settings);
        }

        public static T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, Settings);
        }

        public static JObject ConvertToJObject<T>(T data)
        {
            return data == null ? null : JObject.FromObject(data, JsonSerializer.Create(Settings));
        }
    }
}
