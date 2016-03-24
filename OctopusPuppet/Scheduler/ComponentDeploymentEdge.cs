using System.Diagnostics;
using QuickGraph;

namespace OctopusPuppet.Scheduler
{
    [DebuggerDisplay("{Source.Name} {Source.Version} -> {Target.OctopusProject} {Target.Version}")]
    public class ComponentDeploymentEdge : Edge<ComponentDeploymentVertex>
    {
        public ComponentDeploymentEdge(ComponentDeploymentVertex source, ComponentDeploymentVertex target)
            : base(source, target)
        {
        }

        public override string ToString()
        {
            return string.Format("{0} {1} -> {2} {3}", Source.Name, Source.Version, Target.Name, Target);
        }
    }
}