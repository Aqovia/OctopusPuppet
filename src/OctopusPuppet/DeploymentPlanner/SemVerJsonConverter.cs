using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OctopusPuppet.DeploymentPlanner
{
    public class SemVerJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var version = token.ToString();
            if (string.IsNullOrEmpty(version))
                return null;
            return new SemVer(version);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(SemVer).IsAssignableFrom(objectType);
        }
    }
}
