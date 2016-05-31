using System.Collections.Generic;

namespace OctopusPuppet.DeploymentPlanner
{
    public interface IDeploymentPlanner
    {
        List<Environment> GetEnvironments();
        List<Branch> GetBranches();
        EnvironmentDeploymentPlans GetEnvironmentMirrorDeploymentPlans(string environmentFrom, string environmentTo);
        BranchDeploymentPlans GetBranchDeploymentPlans(string environment, string branch);
        RedeployDeploymentPlans GetRedeployDeploymentPlans(string environment);
    }
}