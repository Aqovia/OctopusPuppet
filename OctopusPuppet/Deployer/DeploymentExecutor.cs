using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OctopusPuppet.Scheduler;

namespace OctopusPuppet.Deployer
{
    public class DeploymentExecutor
    {
        private readonly IComponentVertexDeployer _componentVertexDeployer;
        private readonly EnvironmentDeployment _environmentDeployment;
        private readonly CancellationToken _cancellationToken;
        private readonly IProgress<ComponentVertexDeploymentProgress> _progress;

        public DeploymentExecutor(IComponentVertexDeployer componentVertexDeployer, EnvironmentDeployment environmentDeployment, CancellationToken cancellationToken, IProgress<ComponentVertexDeploymentProgress> progress)
        {
            _componentVertexDeployer = componentVertexDeployer;
            _environmentDeployment = environmentDeployment;
            _cancellationToken = cancellationToken;
            _progress = progress;
        }

        public async Task Execute()
        {
            await DeployEnvironment(_environmentDeployment, _cancellationToken);
        }

        private async Task DeployEnvironment(EnvironmentDeployment environmentDeployment, CancellationToken cancellationToken)
        {
            var productTasks = environmentDeployment.ProductDeployments
                .Select(productDeployment => DeployProduct(productDeployment, cancellationToken));

            await RunInParallel(productTasks, cancellationToken);
        }

        private async Task DeployProduct(ProductDeployment productDeployment, CancellationToken cancellationToken)
        {
            var productStepTasks = productDeployment.DeploymentSteps
                .OrderBy(x => x.ExecutionOrder)
                .Select(x => DeployProductStep(x, cancellationToken));

            await RunInParallel(productStepTasks, cancellationToken);
        }

        private async Task DeployProductStep(ProductDeploymentStep productDeploymentStep, CancellationToken cancellationToken)
        {
            var componentDeploymentTasks = productDeploymentStep.ComponentDeployments.Select(x=>x.Vertex)
                .OrderBy(x => x.ExecutionOrder)
                .Select(x => DeployComponent(x, cancellationToken));

            await RunInSeries(componentDeploymentTasks, cancellationToken);
        }

        private async Task DeployComponent(ComponentDeploymentVertex componentDeploymentVertex, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            //Start progress
            if (_progress != null)
            {
                _progress.Report(new ComponentVertexDeploymentProgress
                {
                    Vertex = componentDeploymentVertex,
                    Status = ComponentVertexDeploymentStatus.Started,
                    MinimumValue = 0,
                    MaximumValue =
                        componentDeploymentVertex.DeploymentDuration.HasValue
                            ? componentDeploymentVertex.DeploymentDuration.Value.Ticks
                            : 0,
                    Value = 0,
                    Text = "Started"
                });
            }

            var status = await _componentVertexDeployer.Deploy(componentDeploymentVertex, cancellationToken, _progress);

            //Finish progress    
            if (_progress != null)
            {
                _progress.Report(new ComponentVertexDeploymentProgress
                {
                    Vertex = componentDeploymentVertex,
                    Status = status,
                    MinimumValue = 0,
                    MaximumValue =
                        componentDeploymentVertex.DeploymentDuration.HasValue
                            ? componentDeploymentVertex.DeploymentDuration.Value.Ticks
                            : 0,
                    Value =
                        componentDeploymentVertex.DeploymentDuration.HasValue
                            ? componentDeploymentVertex.DeploymentDuration.Value.Ticks
                            : 0,
                    Text = status.ToString()
                });
            }
        }

        private static async Task RunInParallel(IEnumerable<Task> tasks, CancellationToken cancellationToken)
        {
            await Task.WhenAny(Task.WhenAll(tasks), cancellationToken.AsTask());
        }

        private static async Task RunInSeries(IEnumerable<Task> tasks, CancellationToken cancellationToken)
        {
            foreach (var task in tasks)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await task;
            }
        }
    }
}
