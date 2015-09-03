namespace OctopusPuppet.DeploymentPlanner
{
    public interface IDeploymentPlanner
    {
        EnvironmentDeploymentPlans GetEnvironmentDeploymentPlans(string environmentFrom, string environmentTo);
        BranchDeploymentPlans GetBranchDeploymentPlans(string environment, string branch);
        RedeployDeploymentPlans GetRedeployDeploymentPlans(string environment);
    }
}