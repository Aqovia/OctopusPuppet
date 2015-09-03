using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OctopusPuppet
{
    public class DeploymentSchedule
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Version { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public TimeSpan? DeploymentDuration { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public List<string> Dependancies { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public string ReleaseNotes { get; set; }

        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public PlanAction Action { get; set; }

        public DeploymentSchedule()
        {
            Dependancies = new List<string>();
        }
    }
}