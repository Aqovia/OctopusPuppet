using System.Threading;
using System.Threading.Tasks;
using OctopusPuppet.Scheduler;

namespace OctopusPuppet.Deployer
{
    public interface IComponentVertexDeployer
    {
        Task Deploy(ComponentDeploymentVertex componentDeploymentVertex, CancellationToken cancellationToken);
    }
}
