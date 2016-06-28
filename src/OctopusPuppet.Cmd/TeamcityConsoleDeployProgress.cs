using System;
using System.Text.RegularExpressions;
using OctopusPuppet.Deployer;

namespace OctopusPuppet.Cmd
{
    public class TeamcityConsoleDeployProgress : IProgress<ComponentVertexDeploymentProgress>
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
            var name = GetName(value);
            var flowId = GetFlowId(value);
            var timeStamp = GetJavaTimeStamp();
            Console.WriteLine("##teamcity[progressMessage name='{0}' flowId='{1}' timestamp='{2}'] ", name, flowId, timeStamp);
        }

        private void ComponentDeploymentStarted(ComponentVertexDeploymentProgress value)
        {
            var name = GetName(value);
            var flowId = GetFlowId(value);
            var timeStamp = GetJavaTimeStamp();
            Console.WriteLine("##teamcity[deploymentStarted name='{0}' flowId='{1}' timestamp='{2}'] ", name, flowId, timeStamp);
            Console.WriteLine("##teamcity[progressStart name='{0}' flowId='{1}' timestamp='{2}']", name, flowId, timeStamp);
            Console.WriteLine("##teamcity[message text='Starting deployment for {0} - expected deployment duration {1}' flowId='{2}' timestamp='{3}']", name, value.Vertex.DeploymentDuration, flowId, timeStamp);
        }

        private void ComponentDeploymentInProgress(ComponentVertexDeploymentProgress value)
        {
            var name = GetName(value);
            var flowId = GetFlowId(value);
            var timeStamp = GetJavaTimeStamp();
            Console.WriteLine("##teamcity[progressFinish name='{0}' flowId='{1}' timestamp='{2}']", name, flowId, timeStamp);
        }

        private void ComponentDeploymentFailure(ComponentVertexDeploymentProgress value)
        {
            var name = GetName(value);
            var flowId = GetFlowId(value);
            var timeStamp = GetJavaTimeStamp();
            Console.WriteLine("##teamcity[progressFinish name='{0}' flowId='{1}' timestamp='{2}']", name, flowId, timeStamp);
            Console.WriteLine("##teamcity[deploymentFailed name='{0}' flowId='{1}' timestamp='{2}']", name, flowId, timeStamp);

            var message = @"
##teamcity[message text='Failed deploy for {0}' flowId='{1}' timestamp='{2}']
##teamcity[blockOpened name='{0}' description='Error message' flowId='{1}' timestamp='{2}']
{3}
##teamcity[blockClosed name='{0}' flowId='{1}' timestamp='{2}']
";

            Console.WriteLine(message, name, flowId, timeStamp, value.Text);
            Console.WriteLine("##teamcity[buildProblem description='Failed deploy for {0}' identity='{1}' name='' timestamp='{2}']", name, flowId, timeStamp);
        }

        private void ComponentDeploymentCancelled(ComponentVertexDeploymentProgress value)
        {
            var name = GetName(value);
            var flowId = GetFlowId(value);
            var timeStamp = GetJavaTimeStamp();
            Console.WriteLine("##teamcity[progressFinish name='{0}' flowId='{1}' timestamp='{2}']", name, flowId, timeStamp);
            Console.WriteLine("##teamcity[deploymentCancelled name='{0}' flowId='{1}' timestamp='{2}'] ", name, flowId, timeStamp);
            Console.WriteLine("##teamcity[message text='Cancelled deploy for {0}' flowId='{1}' timestamp='{2}']", name, flowId, timeStamp);
        }

        private void ComponentDeploymentSuccess(ComponentVertexDeploymentProgress value)
        {
            var name = GetName(value);
            var flowId = GetFlowId(value);
            var timeStamp = GetJavaTimeStamp();
            Console.WriteLine("##teamcity[progressFinish name='{0}' flowId='{1}' timestamp='{2}']", name, flowId, timeStamp);
            Console.WriteLine("##teamcity[deploymentSucceeded name='{0}' flowId='{1}' timestamp='{2}'] ", name, flowId, timeStamp);
            Console.WriteLine("##teamcity[message text='Successfully deploy for {0}' flowId='{1}' timestamp='{2}']", name, flowId, timeStamp);
        }

        private string GetJavaTimeStamp()
        {
            var now = DateTime.UtcNow;

            var result = now.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff") + now.ToString("zzz").Replace(":", "");

            return result;
        }

        private string TeamcityEscape(string value)
        {
            var stringReplace = value
                .Replace("|", "||'")
                .Replace("'", "|'")
                .Replace("\n", "|n'")
                .Replace("\r", "|r")
                .Replace("[", "|['")
                .Replace("]", "|]'");

            //Ignore the unicode escape problem for now

            return stringReplace;
        }

        private string GetFlowId(ComponentVertexDeploymentProgress value)
        {
            return TeamcityEscape(string.IsNullOrEmpty(value.Vertex.Id) ? value.Vertex.Name : value.Vertex.Id);
        }

        private string GetName(ComponentVertexDeploymentProgress value)
        {
            return TeamcityEscape(string.IsNullOrEmpty(value.Vertex.Name) ? value.Vertex.Id : value.Vertex.Name);
        }
    }
}
