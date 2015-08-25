using QuickGraph;

namespace OctopusPuppet.Scheduler
{
    public class ComponentGroupEdge : Edge<ComponentGroupVertex>
    {
        public ComponentGroupEdge(ComponentGroupVertex source, ComponentGroupVertex target)
            : base(source, target)
        {
        }
    }
}
