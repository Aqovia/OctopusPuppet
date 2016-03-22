using QuickGraph;

namespace OctopusPuppet.Scheduler
{
    public class ComponentGraph : AdjacencyGraph<ComponentVertex, ComponentEdge>
    {
        public ComponentGraph() : base(true)
        {   
        }
    }
}
