using System.Collections.Generic;
using Newtonsoft.Json;

namespace OctopusPuppet.DeploymentPlanner
{
    public class EnvironmentDeploymentPlans
    {
        [JsonProperty(Required = Required.AllowNull)]
        public string EnvironmentFromId { get; set; }
        
        [JsonProperty(Required = Required.AllowNull)]
        public string EnvironmentToId { get; set; }
        
        [JsonProperty(Required = Required.AllowNull)]
        public List<DeploymentPlan> DeploymentPlans { get; set; }

        public EnvironmentDeploymentPlans()
        {
            DeploymentPlans = new List<DeploymentPlan>();
        }
    }
}