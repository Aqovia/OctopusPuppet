using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Octopus.Client.Model;

namespace OctopusPuppet
{
    public class Component
    {
        [JsonProperty(Required = Required.Always)]
        public SemanticVersion Version { get; set; }

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