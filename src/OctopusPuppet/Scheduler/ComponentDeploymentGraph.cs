using QuickGraph;

namespace OctopusPuppet.Scheduler
{
    public class ComponentDeploymentGraph : AdjacencyGraph<ComponentDeploymentVertex, ComponentDeploymentEdge>
    {
        public ComponentDeploymentGraph() : base(true)
        {   
        }
    }
}
