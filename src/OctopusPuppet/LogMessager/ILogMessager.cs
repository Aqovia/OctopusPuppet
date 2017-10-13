using OctopusPuppet.Deployer;

namespace OctopusPuppet.LogMessager
{
    public interface ILogMessager
    {
        string DeploymentStarted(ComponentVertexDeploymentProgress componentVertexDeploymentProgress);
        string DeploymentFailed(ComponentVertexDeploymentProgress componentVertexDeploymentProgress);
        string DeploymentCancelled(ComponentVertexDeploymentProgress componentVertexDeploymentProgress);
        string DeploymentSuccess(ComponentVertexDeploymentProgress componentVertexDeploymentProgress);
    }
}
