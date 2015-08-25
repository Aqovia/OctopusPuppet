using QuickGraph;

namespace OctopusPuppet.Scheduler
{
    public class ComponentEdge : Edge<ComponentVertex>
    {
        public ComponentEdge(ComponentVertex source, ComponentVertex target)
            : base(source, target)
        {
        }
    }
}
