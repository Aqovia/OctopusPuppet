using OctopusPuppet.Scheduler;
using QuickGraph;

namespace OctopusPuppet.Gui.Views
{
    public class ComponentGraphLayout : GraphSharp.Controls.GraphLayout<ComponentDeploymentVertex, ComponentDeploymentEdge, IBidirectionalGraph<ComponentDeploymentVertex, ComponentDeploymentEdge>> { }
}
