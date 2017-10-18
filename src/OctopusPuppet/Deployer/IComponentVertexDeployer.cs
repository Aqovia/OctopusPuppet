using System;
using System.Threading;
using OctopusPuppet.LogMessages;
using OctopusPuppet.Scheduler;

namespace OctopusPuppet.Deployer
{
    public interface IComponentVertexDeployer
    {
        ComponentVertexDeploymentResult Deploy(ComponentDeploymentVertex componentDeploymentVertex, CancellationToken cancellationToken, ILogMessages logMessages, IProgress<ComponentVertexDeploymentProgress> progress);
    }
}
