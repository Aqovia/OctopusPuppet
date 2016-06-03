using System;
using System.Threading;
using System.Threading.Tasks;
using OctopusPuppet.Scheduler;

namespace OctopusPuppet.Deployer
{
    public interface IComponentVertexDeployer
    {
        Task<ComponentVertexDeploymentStatus> Deploy(ComponentDeploymentVertex componentDeploymentVertex, CancellationToken cancellationToken, IProgress<ComponentVertexDeploymentProgress> progress);
    }
}
