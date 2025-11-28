using System.Collections.Generic;

namespace OctopusPuppet.DeploymentPlanner
{
    public interface IDeploymentPlanner
    {
        List<Environment> GetEnvironments();
        List<Branch> GetBranches();
        EnvironmentDeploymentPlans GetEnvironmentMirrorDeploymentPlans(string environmentFrom, string environmentTo, bool doNotUseDifferentialDeployment, bool skipNoBranchSuffix, ComponentFilter componentFilter = null);
        BranchDeploymentPlans GetBranchDeploymentPlans(string environment, string branch, bool doNotUseDifferentialDeployment, bool skipNoBranchSuffix, ComponentFilter componentFilter = null);
        RedeployDeploymentPlans GetRedeployDeploymentPlans(string environment, ComponentFilter componentFilter = null);
    }
}