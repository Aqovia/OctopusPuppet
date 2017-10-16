using System;
using System.Threading;
using OctopusPuppet.LogMessager;
using OctopusPuppet.Scheduler;

namespace OctopusPuppet.Deployer
{
    public interface IComponentVertexDeployer
    {
        ComponentVertexDeploymentResult Deploy(ComponentDeploymentVertex componentDeploymentVertex, CancellationToken cancellationToken, ILogMessager logMessager, IProgress<ComponentVertexDeploymentProgress> progress);
    }
}
