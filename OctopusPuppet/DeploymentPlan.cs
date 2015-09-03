using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OctopusPuppet
{
    public class DeploymentPlan
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public string ReleaseNotes { get; set; }

        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public PlanAction Action { get; set; }

        public Component ComponentFrom { get; set; }
        public Component ComponentTo { get; set; }
    }
}