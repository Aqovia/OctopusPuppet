using OctopusPuppet.Scheduler;

namespace OctopusPuppet.LogMessager
{
    public interface ILogMessager
    {
        string DeploymentSkipped(ComponentDeploymentVertex componentDeploymentVertex);

        string DeploymentStarted(ComponentDeploymentVertex componentDeploymentVertex);

        string DeploymentFailed(ComponentDeploymentVertex componentDeploymentVertex, string errorMessage);

        string DeploymentProgress(ComponentDeploymentVertex componentDeploymentVertex, string processingMessage);

        string DeploymentCancelled(ComponentDeploymentVertex componentDeploymentVertex);

        string DeploymentSuccess(ComponentDeploymentVertex componentDeploymentVertex);
    }
}
