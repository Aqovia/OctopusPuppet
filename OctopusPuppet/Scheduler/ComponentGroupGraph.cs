using QuickGraph;

namespace OctopusPuppet.Scheduler
{
    public class ComponentGroupGraph : AdjacencyGraph<ComponentGroupVertex, ComponentGroupEdge>
    {
        public ComponentGroupGraph() : base(true)
        {
            
        }
    }
}
