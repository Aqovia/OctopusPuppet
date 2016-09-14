using System;
using System.Threading;
using OctopusPuppet.Scheduler;

namespace OctopusPuppet.Deployer
{
    public interface IComponentVertexDeployer
    {
        IComponentVertextDeploymentResult Deploy(ComponentDeploymentVertex componentDeploymentVertex, CancellationToken cancellationToken, IProgress<ComponentVertexDeploymentProgress> progress);
    }
}
