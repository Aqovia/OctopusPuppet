using System;
using System.Threading;
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

        

        public ComponentVertexDeploymentResult Deploy(ComponentDeploymentVertex componentDeploymentVertex, CancellationToken cancellationToken, IProgress<ComponentVertexDeploymentProgress> progress)
        {
            if (!componentDeploymentVertex.Exists || componentDeploymentVertex.DeploymentAction == PlanAction.Skip)
            {
                return new ComponentVertexDeploymentResult
                {
                    Status = ComponentVertexDeploymentStatus.Success,
                    Description = "Skipped"
                };
            }

            if (componentDeploymentVertex.Version == null)
            {
                throw new Exception("Version for release is null");
            }

            var environment = _repository.Environments.GetEnvironment(_environmentToDeployTo.Name);
            var project = _repository.Projects.GetProjectByName(componentDeploymentVertex.Name);
            var release = _repository.Projects.GetRelease(project.Id, componentDeploymentVertex.Version);
           
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
                foreach (var task in tasks)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _repository.Tasks.Cancel(task);
                    }

                    var duration = new TimeSpan(0);

                    if (task.StartTime.HasValue)
                    {
                        var now = new DateTimeOffset(DateTime.UtcNow);
                        duration = now.Subtract(task.StartTime.Value);
                    }

                    if (progress != null)
                    {
                        progress.Report(new ComponentVertexDeploymentProgress
                        {
                            Vertex = componentDeploymentVertex,
                            Status = ComponentVertexDeploymentStatus.InProgress,
                            MinimumValue = 0,
                            MaximumValue =
                                componentDeploymentVertex.DeploymentDuration.HasValue
                                    ? componentDeploymentVertex.DeploymentDuration.Value.Ticks
                                    : 0,
                            Value = duration.Ticks,
                            Text = task.Description
                        });
                    }
                }
            };

            _repository.Tasks.WaitForCompletion(deploymentTask, _pollIntervalSeconds, _timeoutAfterMinutes, interval);

            deploymentTask = _repository.Tasks.Get(queuedDeployment.TaskId);

            var result = new ComponentVertexDeploymentResult();

            switch (deploymentTask.State)
            {
                case TaskState.Success:
                    result.Status = ComponentVertexDeploymentStatus.Success;
                    result.Description = "Deployed";
                    break;

                case TaskState.Canceled:
                case TaskState.Cancelling:
                    result.Status = ComponentVertexDeploymentStatus.Cancelled;
                    result.Description = "Cancelled";
                    break;

                case TaskState.Failed:
                case TaskState.TimedOut:
                    result.Status = ComponentVertexDeploymentStatus.Failure;
                    result.Description = deploymentTask.ErrorMessage;
                    break;

                default:
                    result.Status = ComponentVertexDeploymentStatus.Failure;
                    result.Description = deploymentTask.ErrorMessage;
                    break;
            }

            return result;
        }
    }
}