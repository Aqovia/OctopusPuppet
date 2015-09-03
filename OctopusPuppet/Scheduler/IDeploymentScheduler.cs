using System.Collections.Generic;
using OctopusPuppet.DeploymentPlanner;
using QuickGraph;

namespace OctopusPuppet.Scheduler
{
    public interface IDeploymentScheduler
    {
        List<List<ComponentGroupVertex>> GetDeploymentSchedule(List<DeploymentPlan> componentDependancies);
        List<List<ComponentGroupVertex>> GetDeploymentSchedule(AdjacencyGraph<ComponentVertex, ComponentEdge> componentDependanciesAdjacencyGraph);
    }
}