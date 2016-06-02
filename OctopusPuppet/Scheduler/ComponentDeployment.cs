using System.Collections.Generic;

namespace OctopusPuppet.Scheduler
{
    public class ComponentDeployment
    {
        public ComponentDeploymentVertex Vertex { get; set; }
        public List<ComponentDeploymentEdge> Edges { get; set; }
    }
}
