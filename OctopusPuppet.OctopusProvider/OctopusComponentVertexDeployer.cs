using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Octopus.Client;
using Octopus.Client.Model;
using OctopusPuppet.Deployer;
using OctopusPuppet.DeploymentPlanner;
using OctopusPuppet.Scheduler;

namespace OctopusPuppet.OctopusProvider
{
    public class OctopusComponentVertexDeployer : IComponentVertexDeployer
    {
        private readonly DeploymentPlanner.Environment _environmentToDeployTo;
        private readonly string _comments;
        private readonly bool _forcePackageDownload;
        private readonly bool _forcePackageRedeployment;
        private readonly int _pollIntervalSeconds;
        private readonly int _timeoutAfterMinutes;
        private readonly OctopusRepository _repository;

        public OctopusComponentVertexDeployer(string url, string apiKey, DeploymentPlanner.Environment environmentToDeployTo, 
            string comments = "",
            bool forcePackageDownload = false,
            bool forcePackageRedeployment = false,
            int pollIntervalSeconds = 4, 
            int timeoutAfterMinutes = 0)
        {
            _environmentToDeployTo = environmentToDeployTo;
            _comments = comments;
            _forcePackageDownload = forcePackageDownload;
            _forcePackageRedeployment = forcePackageRedeployment;
            _pollIntervalSeconds = pollIntervalSeconds;
            _timeoutAfterMinutes = timeoutAfterMinutes;
            var octopusServerEndpoint = new OctopusServerEndpoint(url, apiKey);
            _repository = new OctopusRepository(octopusServerEndpoint);
        }

        private DeploymentPlanner.Environment GetEnvironment(string environment)
        {
            var environments = _repository.Environments
                .GetAll()
                .Select(x => new DeploymentPlanner.Environment()
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .FirstOrDefault(x => x.Name == environment);

            return environments;
        }

        private ReferenceDataItem GetProject(string project)
        {
            var release = _repository.Projects
                .GetAll()
                .FirstOrDefault(x => x.Name == project);

            return release;
        }

        /// <summary>
        /// Find first release for project that matches version
        /// </summary>
        /// <param name="projectId">The project id to match on</param>
        /// <param name="version">Version for the project to match on</param>
        /// <returns>Matched release or null if there is no match</returns>
        private ReleaseResource GetReleaseResources(string projectId, SemVer version)
        {
            var project = _repository.Projects.Get(projectId);
            var skip = 0;
            var shouldPage = false;
            do
            {
                var releasePages = _repository.Projects.GetReleases(project, skip);

                var release = releasePages.Items.FirstOrDefault(x => new SemVer(x.Version) == version);

                if (release != null)
                {
                    return release;
                }

                skip += releasePages.ItemsPerPage;
                shouldPage = releasePages.TotalResults > skip;
            } while (shouldPage);

            return null;
        }

        public async Task Deploy(ComponentDeploymentVertex componentDeploymentVertex, CancellationToken cancellationToken)
        {
            await Task.Factory.StartNew(() =>
            {
                
                if (!componentDeploymentVertex.Exists || componentDeploymentVertex.Action == PlanAction.Skip)
                {
                    return;
                }

                if (componentDeploymentVertex.Version == null)
                {
                    throw new Exception("Version for release is null");
                }

                var environment = GetEnvironment(_environmentToDeployTo.Name);
                if (environment == null)
                {
                    throw new Exception(string.Format("Can't find environment name of {0}", _environmentToDeployTo.Name));
                }

                var project = GetProject(componentDeploymentVertex.Name);
                if (project == null)
                {
                    throw new Exception(string.Format("Can't find project with name of {0}", componentDeploymentVertex.Name));
                }

                var release = GetReleaseResources(project.Id, componentDeploymentVertex.Version);
                if (release == null)
                {
                    throw new Exception(string.Format("Can't find release with project id {0} and version of {1}", componentDeploymentVertex.Name, componentDeploymentVertex.Version));
                }

                var deployment = new DeploymentResource
                {
                    ReleaseId = release.Id,
                    EnvironmentId = environment.Id,
                    Comments = _comments,
                    ForcePackageDownload = _forcePackageDownload,
                    ForcePackageRedeployment = _forcePackageRedeployment,
                };

                var queuedDeployment = _repository.Deployments.Create(deployment);
                var deploymentTask = _repository.Tasks.Get(queuedDeployment.TaskId);

                Action<TaskResource[]> interval = tasks =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        foreach (var task in tasks)
                        {
                            _repository.Tasks.Cancel(task);
                        }
                    }
                };

                _repository.Tasks.WaitForCompletion(deploymentTask, _pollIntervalSeconds, _timeoutAfterMinutes, interval);
            }, cancellationToken);
        }
    }
}
