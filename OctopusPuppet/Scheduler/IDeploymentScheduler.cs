using System.Collections.Generic;
using OctopusPuppet.DeploymentPlanner;

namespace OctopusPuppet.Scheduler
{
    public interface IDeploymentScheduler
    {
        ComponentGraph GetDeploymentComponentGraph(List<DeploymentPlan> componentDependancies);
        List<List<ComponentGroupVertex>> GetDeploymentSchedule(ComponentGraph componentDependanciesAdjacencyGraph);
    }
}