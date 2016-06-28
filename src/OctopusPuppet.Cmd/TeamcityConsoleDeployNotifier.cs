using System;
using CommandLine.Text;
using JetBrains.TeamCity.ServiceMessages.Write;
using Newtonsoft.Json;
using OctopusPuppet.Deployer;
using OctopusPuppet.Scheduler;

namespace OctopusPuppet.Cmd
{
    public class TeamcityConsoleDeployNotifier : INotifier
    {
        private readonly string NoParent = "0";
        private readonly ServiceMessageFormatter _serviceMessageFormatter = new ServiceMessageFormatter();

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
            var timeStamp = GetJavaTimeStamp();

            var message = _serviceMessageFormatter.FormatMessage("message", new
            {
                text = helpText,
                flowId = NoParent,
                timeStamp = timeStamp
            });

            Console.WriteLine(message);
        }

        public void PrintVersion(string version)
        {
            var timeStamp = GetJavaTimeStamp();

            var message = _serviceMessageFormatter.FormatMessage("message", new
            {
                text = version,
                flowId = NoParent,
                timeStamp = timeStamp
            });

            Console.WriteLine(message);
        }

        public void PrintActionMessage(string messageToSend)
        {
            var timeStamp = GetJavaTimeStamp();

            var message = _serviceMessageFormatter.FormatMessage("message", new
            {
                text = messageToSend,
                flowId = NoParent,
                timeStamp = timeStamp
            });

            Console.WriteLine(message);
        }

        public void PrintEnvironmentDeploy(EnvironmentDeployment environmentDeployment)
        {
            var name = "EnvironmentDeployment";
            var description = "Environment Deployment";
            var timeStamp = GetJavaTimeStamp();

            var environmentDeploymentJson = JsonConvert.SerializeObject(environmentDeployment, new JsonSerializerSettings { Formatting = Formatting.Indented });

            var openBlockMessage = _serviceMessageFormatter.FormatMessage("blockOpened", new
            {
                name = name,
                description = description,
                flowId = NoParent,
                timeStamp = timeStamp
            });

            var closeBlockMessage = _serviceMessageFormatter.FormatMessage("blockClosed", new
            {
                name = name,
                flowId = NoParent,
                timeStamp = timeStamp
            });

            var message = openBlockMessage + Environment.NewLine + environmentDeploymentJson + Environment.NewLine + closeBlockMessage;

            Console.WriteLine(message);
        }

        private void ComponentDeploymentNotStarted(ComponentVertexDeploymentProgress value)
        {
        }

        private void ComponentDeploymentStarted(ComponentVertexDeploymentProgress value)
        {
            var name = GetName(value);
            var flowId = GetFlowId(value);
            var timeStamp = GetJavaTimeStamp();

            var progressStartMessage = _serviceMessageFormatter.FormatMessage("progressStart", new
            {
                name = name,
                parent = NoParent,
                flowId = flowId,
                timeStamp = timeStamp
            });

            var message = _serviceMessageFormatter.FormatMessage("message", new
            {
                text = string.Format("Starting deployment for {0} - expected deployment duration {1}", name, value.Vertex.DeploymentDuration),
                parent = NoParent,
                flowId = flowId,
                timeStamp = timeStamp
            });

            var testStartedMessage = _serviceMessageFormatter.FormatMessage("testStarted", new
            {
                name = name,
                parent = NoParent,
                flowId = flowId,
                timeStamp = timeStamp
            });

            Console.WriteLine(progressStartMessage);
            Console.WriteLine(message);
            Console.WriteLine(testStartedMessage);
        }

        private void ComponentDeploymentInProgress(ComponentVertexDeploymentProgress value)
        {
            var name = GetName(value);
            var flowId = GetFlowId(value);
            var timeStamp = GetJavaTimeStamp();

            var progressMessage = _serviceMessageFormatter.FormatMessage("progressMessage", new
            {
                name = name,
                parent = NoParent,
                flowId = flowId,
                timeStamp = timeStamp
            });

            Console.WriteLine(progressMessage);
        }

        private void ComponentDeploymentFailure(ComponentVertexDeploymentProgress value)
        {
            var name = GetName(value);
            var flowId = GetFlowId(value);
            var timeStamp = GetJavaTimeStamp();

            var testFailedMessage = _serviceMessageFormatter.FormatMessage("testFailed", new
            {
                name = name,
                message = "Deployment failed",
                details = value.Text,
                parent = NoParent,
                flowId = flowId,
                timeStamp = timeStamp
            });

            var progressFinishMessage = _serviceMessageFormatter.FormatMessage("progressFinish", new
            {
                name = name,
                parent = NoParent,
                flowId = flowId,
                timeStamp = timeStamp
            });

            var message = _serviceMessageFormatter.FormatMessage("message", new
            {
                text = value.Text,
                errorDetails = value.Text,
                status = "ERROR",
                parent = NoParent,
                flowId = flowId,
                timeStamp = timeStamp
            });

            Console.WriteLine(testFailedMessage);
            Console.WriteLine(message);
            Console.WriteLine(progressFinishMessage);
        }

        private void ComponentDeploymentCancelled(ComponentVertexDeploymentProgress value)
        {
            var name = GetName(value);
            var flowId = GetFlowId(value);
            var timeStamp = GetJavaTimeStamp();

            var testFailedMessage = _serviceMessageFormatter.FormatMessage("testFailed", new
            {
                name = name,
                message = "Deployment cancelled",
                details = string.Format("Cancelled deployment for {0}", name),
                parent = NoParent,
                flowId = flowId,
                timeStamp = timeStamp
            });

            var progressFinishMessage = _serviceMessageFormatter.FormatMessage("progressFinish", new
            {
                name = name,
                parent = NoParent,
                flowId = flowId,
                timeStamp = timeStamp
            });

            var message = _serviceMessageFormatter.FormatMessage("message", new
            {
                text = string.Format("Cancelled deploy for {0}", name),
                errorDetails = string.Format("Cancelled deploy for {0}", name),
                status = "ERROR",
                parent = NoParent,
                flowId = flowId,
                timeStamp = timeStamp
            });

            Console.WriteLine(testFailedMessage);
            Console.WriteLine(message);
            Console.WriteLine(progressFinishMessage);
        }

        private void ComponentDeploymentSuccess(ComponentVertexDeploymentProgress value)
        {
            var name = GetName(value);
            var duration = value.Vertex.DeploymentDuration.HasValue
                ? value.Vertex.DeploymentDuration.Value.Milliseconds
                : 0;
            var flowId = GetFlowId(value);
            var timeStamp = GetJavaTimeStamp();

            var testFinishMessage = _serviceMessageFormatter.FormatMessage("testFinished", new
            {
                name = name,
                duration = duration,
                parent = NoParent,
                flowId = flowId,
                timeStamp = timeStamp
            });

            var progressFinishMessage = _serviceMessageFormatter.FormatMessage("progressFinish", new
            {
                name = name,
                parent = NoParent,
                flowId = flowId,
                timeStamp = timeStamp
            });

            var message = _serviceMessageFormatter.FormatMessage("message", new
            {
                text = string.Format("Successfully deploy for {0}", name),
                parent = "",
                flowId = flowId,
                timeStamp = timeStamp
            });

            Console.WriteLine(testFinishMessage);
            Console.WriteLine(message);
            Console.WriteLine(progressFinishMessage);
        }

        private string GetJavaTimeStamp()
        {
            var now = DateTime.UtcNow;

            var result = now.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff") + now.ToString("zzz").Replace(":", "");

            return result;
        }

        private string GetFlowId(ComponentVertexDeploymentProgress value)
        {
            return string.IsNullOrEmpty(value.Vertex.Id) ? value.Vertex.Name : value.Vertex.Id;
        }

        private string GetName(ComponentVertexDeploymentProgress value)
        {
            return string.IsNullOrEmpty(value.Vertex.Name) ? value.Vertex.Id : value.Vertex.Name;
        }
    }
}
