using System.Collections.Generic;
using Newtonsoft.Json;

namespace OctopusPuppet.Scheduler
{
    public class EnvironmentDeploymentSchedule
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public List<DeploymentSchedule> DeploymentSchedules { get; set; }

        public EnvironmentDeploymentSchedule()
        {
            DeploymentSchedules = new List<DeploymentSchedule>();
        }
    }
}