using OctopusPuppet.DeploymentPlanner;

namespace OctopusPuppet.Scheduler
{
    public interface IDeploymentScheduler
    {
        ComponentDeploymentGraph GetComponentDeploymentGraph(EnvironmentDeploymentPlan environmentDeploymentPlan);
        EnvironmentDeployment GetEnvironmentDeployment(ComponentDeploymentGraph componentDeploymentDependanciesAdjacencyGraph);
    }
}