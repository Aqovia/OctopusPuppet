namespace OctopusPuppet.Deployer
{
    public class ComponentVertextDeploymentResult : IComponentVertextDeploymentResult
    {
        public ComponentVertexDeploymentStatus Status { get; set; }
        public string Description { get; set; }
    }
}
