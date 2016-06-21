using Newtonsoft.Json;

namespace OctopusPuppet.DeploymentPlanner
{
    public class BranchDeploymentPlans
    {
        [JsonProperty(Required = Required.AllowNull)]
        public string EnvironmentId { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public string Branch { get; set; }        

        [JsonProperty(Required = Required.AllowNull)]
        public EnvironmentDeploymentPlan EnvironmentDeploymentPlan { get; set; }
    }
}