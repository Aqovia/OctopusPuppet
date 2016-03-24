using System.Collections.Generic;

namespace OctopusPuppet.DeploymentPlanner
{
    public class EnvironmentDeploymentPlan
    {
        public List<ComponentDeploymentPlan> DeploymentPlans { get; set; }

        public EnvironmentDeploymentPlan(List<ComponentDeploymentPlan> deploymentPlans)
        {
            DeploymentPlans = deploymentPlans;
        }
    }
}
