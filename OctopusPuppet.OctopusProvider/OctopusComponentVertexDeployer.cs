using System;
using System.Threading;
using System.Threading.Tasks;
using Octopus.Client;
using Octopus.Client.Model;
using OctopusPuppet.Deployer;
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

        public async Task Deploy(ComponentDeploymentVertex componentDeploymentVertex, CancellationToken cancellationToken)
        {
            await Task.Factory.StartNew(() =>
            {
                var releaseId = ""; 
                var environmentId = "";// _environmentToDeployTo;

                var deployment = new DeploymentResource
                {
                    ReleaseId = releaseId,
                    EnvironmentId = environmentId,
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
