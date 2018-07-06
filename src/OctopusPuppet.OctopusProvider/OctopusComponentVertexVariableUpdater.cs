using System;
using System.Threading;
using Octopus.Client;
using OctopusPuppet.Deployer;
using OctopusPuppet.DeploymentPlanner;
using OctopusPuppet.LogMessages;
using OctopusPuppet.Scheduler;

namespace OctopusPuppet.OctopusProvider
{
    public class OctopusComponentVertexVariableUpdater : IComponentVertexDeployer
    {
        private readonly OctopusRepository _repository;

        public OctopusComponentVertexVariableUpdater(OctopusApiSettings apiSettings)
        {
            _repository = new OctopusRepository(new OctopusServerEndpoint(apiSettings.Url, apiSettings.ApiKey));
        }

        public ComponentVertexDeploymentResult Deploy(ComponentDeploymentVertex vertex, CancellationToken cancellationToken, ILogMessages logMessages, IProgress<ComponentVertexDeploymentProgress> progress)
        {
            if (!vertex.Exists || vertex.VariableAction == VariableAction.Leave)
            {
                return new ComponentVertexDeploymentResult
                {
                    Status = ComponentVertexDeploymentStatus.Success
                };
            }

            var project = _repository.Projects.GetProjectByName(vertex.Name);
            var release = _repository.Projects.GetRelease(project.Id, vertex.Version);

            _repository.Releases.SnapshotVariables(release);

            return new ComponentVertexDeploymentResult
            {
                Status = ComponentVertexDeploymentStatus.Success,
                Description = "Updated"
            };
        }
    }
}