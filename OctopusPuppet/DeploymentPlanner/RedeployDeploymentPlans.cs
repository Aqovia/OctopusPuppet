using Newtonsoft.Json;

namespace OctopusPuppet.DeploymentPlanner
{
    public class RedeployDeploymentPlans
    {
        [JsonProperty(Required = Required.AllowNull)]
        public string EnvironmentId { get; set; }
       
        [JsonProperty(Required = Required.AllowNull)]
        public EnvironmentDeploymentPlan EnvironmentDeploymentPlan { get; set; }
    }
}