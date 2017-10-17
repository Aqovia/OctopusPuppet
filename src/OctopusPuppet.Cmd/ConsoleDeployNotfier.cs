using System;
using CommandLine.Text;
using Newtonsoft.Json;
using OctopusPuppet.Deployer;
using OctopusPuppet.LogMessages;
using OctopusPuppet.Scheduler;

namespace OctopusPuppet.Cmd
{
    public class ConsoleDeployNotfier : INotifier
    {
        private readonly ILogMessages _logMessages;

        public ConsoleDeployNotfier(ILogMessages logMessages)
        {
            _logMessages = logMessages;
        }

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

        public void PrintHelp(HelpText helpText)
        {
            Console.Out.WriteLine(helpText);
        }

        public void PrintVersion(string version)
        {
            Console.Out.WriteLine(version);
        }

        public void PrintActionMessage(string message)
        {
            Console.Out.WriteLine(message);
        }

        public void PrintEnvironmentDeploy(EnvironmentDeployment environmentDeployment)
        {
            var environmentDeploymentJson = JsonConvert.SerializeObject(environmentDeployment, new JsonSerializerSettings { Formatting = Formatting.Indented });
            Console.Out.WriteLine(environmentDeploymentJson);
        }

        private void ComponentDeploymentNotStarted(ComponentVertexDeploymentProgress value)
        {
        }

        private void ComponentDeploymentStarted(ComponentVertexDeploymentProgress value)
        {
            Console.Out.WriteLine(_logMessages.DeploymentStarted(value.Vertex));
        }

        private void ComponentDeploymentInProgress(ComponentVertexDeploymentProgress value)
        {
        }

        private void ComponentDeploymentFailure(ComponentVertexDeploymentProgress value)
        {
            Console.Out.WriteLine(_logMessages.DeploymentFailed(value.Vertex, value.Text));
        }

        private void ComponentDeploymentCancelled(ComponentVertexDeploymentProgress value)
        {
            Console.Out.WriteLine(_logMessages.DeploymentCancelled(value.Vertex));
        }

        private void ComponentDeploymentSuccess(ComponentVertexDeploymentProgress value)
        {
            Console.Out.WriteLine(_logMessages.DeploymentSuccess(value.Vertex));
        }
    }
}
