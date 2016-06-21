using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OctopusPuppet.DeploymentPlanner
{
    public class Component
    {
        [JsonProperty(Required = Required.Always)]
        public bool Healthy { get; set; }

        [JsonProperty(Required = Required.Always)]
        public SemVer Version { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public TimeSpan? DeploymentDuration { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public List<string> Dependancies { get; set; }

        public Component()
        {
            Dependancies = new List<string>();
        }
    }
}