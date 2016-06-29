using System;
using CommandLine.Text;
using JetBrains.TeamCity.ServiceMessages.Write;
using Newtonsoft.Json;
using OctopusPuppet.Deployer;
using OctopusPuppet.DeploymentPlanner;
using OctopusPuppet.Scheduler;
using Environment = System.Environment;

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

            Console.Out.WriteLine(message);
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

            Console.Out.WriteLine(message);
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

            Console.Out.WriteLine(message);
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
                flowId = name,
                timeStamp = timeStamp
            });

            var closeBlockMessage = _serviceMessageFormatter.FormatMessage("blockClosed", new
            {
                name = name,
                flowId = name,
                timeStamp = timeStamp
            });

            var message = openBlockMessage + Environment.NewLine + environmentDeploymentJson + Environment.NewLine + closeBlockMessage;

            Console.Out.WriteLine(message);
        }

        private void ComponentDeploymentNotStarted(ComponentVertexDeploymentProgress value)
        {
        }

        private void ComponentDeploymentStarted(ComponentVertexDeploymentProgress value)
        {
            var name = GetName(value);
            var flowId = GetFlowId(value);
            var timeStamp = GetJavaTimeStamp();

            var progressMessage = _serviceMessageFormatter.FormatMessage("progressMessage", string.Format("Deployment started for {0}", name));

            var testStartedMessage = _serviceMessageFormatter.FormatMessage("testStarted", new
            {
                name = name,
                flowId = flowId,
                timeStamp = timeStamp
            });

            Console.Out.WriteLine(progressMessage);
            Console.Out.WriteLine(testStartedMessage);
        }

        private void ComponentDeploymentInProgress(ComponentVertexDeploymentProgress value)
        {
        }

        private void ComponentDeploymentFailure(ComponentVertexDeploymentProgress value)
        {
            var name = GetName(value);
            var flowId = GetFlowId(value);
            var timeStamp = GetJavaTimeStamp();

            var progressMessage = _serviceMessageFormatter.FormatMessage("progressMessage", string.Format("Deployment failed for {0}", name));

            var buildProblemMessage = _serviceMessageFormatter.FormatMessage("buildProblem", new
            {
                description = string.Format("Deployment failed for {0}", name),
                identity = name,
                flowId = flowId,
                timeStamp = timeStamp
            });

            var testFailedMessage = _serviceMessageFormatter.FormatMessage("testFailed", new
            {
                name = name,
                message = "Deployment failed",
                details = value.Text,
                flowId = flowId,
                timeStamp = timeStamp
            });

            var failedDeploymentMessage = _serviceMessageFormatter.FormatMessage("message", new
            {
                text = value.Text,
                errorDetails = value.Text,
                status = "ERROR",
                flowId = flowId,
                timeStamp = timeStamp
            });

            var testFinishMessage = _serviceMessageFormatter.FormatMessage("testFinished", new
            {
                name = name,
                flowId = flowId,
                timeStamp = timeStamp
            });

            Console.Out.WriteLine(progressMessage);
            Console.Out.WriteLine(buildProblemMessage);
            Console.Out.WriteLine(testFailedMessage);
            Console.Out.WriteLine(failedDeploymentMessage);
            Console.Out.WriteLine(testFinishMessage);
        }

        private void ComponentDeploymentCancelled(ComponentVertexDeploymentProgress value)
        {
            var name = GetName(value);
            var flowId = GetFlowId(value);
            var timeStamp = GetJavaTimeStamp();

            var progressMessage = _serviceMessageFormatter.FormatMessage("progressMessage", string.Format("Deployment cancelled for {0}", name));

            var testFailedMessage = _serviceMessageFormatter.FormatMessage("testFailed", new
            {
                name = name,
                message = "Deployment cancelled",
                details = string.Format("Cancelled deployment for {0}", name),
                flowId = flowId,
                timeStamp = timeStamp
            });

            var cancelledDeploymentMessage = _serviceMessageFormatter.FormatMessage("message", new
            {
                text = string.Format("Cancelled deploy for {0}", name),
                errorDetails = string.Format("Cancelled deploy for {0}", name),
                status = "ERROR",
                flowId = flowId,
                timeStamp = timeStamp
            });

            var testFinishMessage = _serviceMessageFormatter.FormatMessage("testFinished", new
            {
                name = name,
                flowId = flowId,
                timeStamp = timeStamp
            });

            Console.Out.WriteLine(progressMessage);
            Console.Out.WriteLine(testFailedMessage);
            Console.Out.WriteLine(cancelledDeploymentMessage);
            Console.Out.WriteLine(testFinishMessage);
        }

        private void ComponentDeploymentSuccess(ComponentVertexDeploymentProgress value)
        {
            var name = GetName(value);
            var flowId = GetFlowId(value);
            var timeStamp = GetJavaTimeStamp();

            var progressMessage = _serviceMessageFormatter.FormatMessage("progressMessage", string.Format("Deployment succeeded for {0}", name));

            var testIgnoredMessage = _serviceMessageFormatter.FormatMessage("testIgnored", new
            {
                name = name,
                message = string.Format("Plan action was {0} for {1}", value.Vertex.Action, name),
                flowId = flowId,
                timeStamp = timeStamp
            });

            var testFinishMessage = _serviceMessageFormatter.FormatMessage("testFinished", new
            {
                name = name,
                flowId = flowId,
                timeStamp = timeStamp
            });

            if (value.Vertex.Action != PlanAction.Change)
            {
                Console.Out.WriteLine(testIgnoredMessage);
            }
            Console.Out.WriteLine(progressMessage);
            Console.Out.WriteLine(testFinishMessage);
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
