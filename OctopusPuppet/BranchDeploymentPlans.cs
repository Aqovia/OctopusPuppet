using System.Collections.Generic;
using Newtonsoft.Json;

namespace OctopusPuppet
{
    public class BranchDeploymentPlans
    {
        [JsonProperty(Required = Required.AllowNull)]
        public string EnvironmentId { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public string Branch { get; set; }        

        [JsonProperty(Required = Required.AllowNull)]
        public List<DeploymentPlan> DeploymentPlans { get; set; }

        public BranchDeploymentPlans()
        {
            DeploymentPlans = new List<DeploymentPlan>();
        }
    }
}