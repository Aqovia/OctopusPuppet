using QuickGraph;

namespace OctopusPuppet
{
    public class ComponentEdge : Edge<ComponentVertex>
    {
        public ComponentEdge(ComponentVertex source, ComponentVertex target)
            : base(source, target)
        {
        }
    }
}
