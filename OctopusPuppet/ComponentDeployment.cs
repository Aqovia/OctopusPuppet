using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OctopusPuppet.Scheduler;

namespace OctopusPuppet
{
    public class ComponentDeployment
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Version { get; set; }

        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ComponentAction Action { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public TimeSpan? DeploymentDuration { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public List<string> Dependancies { get; set; }

        public ComponentDeployment()
        {
            Dependancies = new List<string>();
        }
    }
}