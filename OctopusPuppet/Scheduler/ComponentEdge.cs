using System.Diagnostics;
using QuickGraph;

namespace OctopusPuppet.Scheduler
{
    [DebuggerDisplay("{Source.OctopusProject} {Source.Version} -> {Target.OctopusProject} {Target.Version}")]
    public class ComponentEdge : Edge<ComponentVertex>
    {
        public ComponentEdge(ComponentVertex source, ComponentVertex target)
            : base(source, target)
        {
        }

        public override string ToString()
        {
            return string.Format("{0} {1} -> {2} {3}", Source.OctopusProject, Source.Version, Target.OctopusProject, Target);
        }
    }
}