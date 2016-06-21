using System;
using OctopusPuppet.Deployer;

namespace OctopusPuppet.Cmd
{
    public class ConsoleDeployProgress : IProgress<ComponentVertexDeploymentProgress>
    {
        public void Report(ComponentVertexDeploymentProgress value)
        {
            if (value != null)
            {
                switch (value.Status)
                {
                    case ComponentVertexDeploymentStatus.NotStarted:
                        ComponentDeploymentNotStarted(value);
                        break;
                    case ComponentVertexDeploymentStatus.Started:
                        ComponentDeploymentStarted(value);
                        break;
                    case ComponentVertexDeploymentStatus.InProgress:
                        ComponentDeploymentInProgress(value);
                        break;
                    case ComponentVertexDeploymentStatus.Failure:
                        ComponentDeploymentFailure(value);
                        break;
                    case ComponentVertexDeploymentStatus.Cancelled:
                        ComponentDeploymentCancelled(value);
                        break;
                    case ComponentVertexDeploymentStatus.Success:
                        ComponentDeploymentSuccess(value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ComponentDeploymentNotStarted(ComponentVertexDeploymentProgress value)
        {
        }

        private void ComponentDeploymentStarted(ComponentVertexDeploymentProgress value)
        {
            Console.WriteLine("Starting deployment for {0} - expected deployment duration {1}", value.Vertex.Name, value.Vertex.DeploymentDuration);
        }

        private void ComponentDeploymentInProgress(ComponentVertexDeploymentProgress value)
        {
        }

        private void ComponentDeploymentFailure(ComponentVertexDeploymentProgress value)
        {
            Console.WriteLine("Failed deploy for {0}\r\n{1}", value.Vertex.Name, value.Text);
        }

        private void ComponentDeploymentCancelled(ComponentVertexDeploymentProgress value)
        {
            Console.WriteLine("Cancelled deploy for {0}", value.Vertex.Name);
        }

        private void ComponentDeploymentSuccess(ComponentVertexDeploymentProgress value)
        {
            Console.WriteLine("Successfully deploy for {0}", value.Vertex.Name);
        }
    }
}
