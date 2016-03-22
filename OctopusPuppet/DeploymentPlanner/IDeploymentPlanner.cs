using System.Collections.Generic;

namespace OctopusPuppet.DeploymentPlanner
{
    public interface IDeploymentPlanner
    {
        List<string> GetEnvironments();
        List<string> GetBranches();
        EnvironmentDeploymentPlans GetEnvironmentMirrorDeploymentPlans(string environmentFrom, string environmentTo);
        BranchDeploymentPlans GetBranchDeploymentPlans(string environment, string branch);
        RedeployDeploymentPlans GetRedeployDeploymentPlans(string environment);
    }
}