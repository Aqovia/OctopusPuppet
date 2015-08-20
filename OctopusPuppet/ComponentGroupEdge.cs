using QuickGraph;

namespace OctopusPuppet
{
    public class ComponentGroupEdge : Edge<ComponentGroupVertex>
    {
        public ComponentGroupEdge(ComponentGroupVertex source, ComponentGroupVertex target)
            : base(source, target)
        {
        }
    }
}
