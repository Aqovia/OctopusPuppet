namespace OctopusPuppet.Deployer
{
    public interface IComponentVertextDeploymentResult
    {
        ComponentVertexDeploymentStatus Status { get; set; }
        string Description { get; set; }
    }
}