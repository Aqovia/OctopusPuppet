using System.Collections.Generic;
using Newtonsoft.Json;

namespace OctopusPuppet.DeploymentPlanner
{
    public class RedeployDeploymentPlans
    {
        [JsonProperty(Required = Required.AllowNull)]
        public string EnvironmentId { get; set; }
       
        [JsonProperty(Required = Required.AllowNull)]
        public List<DeploymentPlan> DeploymentPlans { get; set; }

        public RedeployDeploymentPlans()
        {
            DeploymentPlans = new List<DeploymentPlan>();
        }
    }
}