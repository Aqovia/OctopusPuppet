using System;
using System.Threading;
using OctopusPuppet.Scheduler;

namespace OctopusPuppet.Deployer
{
    public interface IComponentVertexDeployer
    {
        ComponentVertexDeploymentResult Deploy(ComponentDeploymentVertex componentDeploymentVertex, CancellationToken cancellationToken, IProgress<ComponentVertexDeploymentProgress> progress);
    }
}
