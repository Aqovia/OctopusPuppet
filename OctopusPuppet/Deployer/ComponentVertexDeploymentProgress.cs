using OctopusPuppet.Scheduler;

namespace OctopusPuppet.Deployer
{
    public class ComponentVertexDeploymentProgress
    {
        public ComponentDeploymentVertex Vertex { get; set; }
        public ComponentVertexDeploymentStatus Status { get; set; }
        public long MinimumValue { get; set; }
        public long MaximumValue { get; set; }
        public long Value { get; set; }
        public string Text { get; set; }
    }
}
