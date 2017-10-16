using System;
using OctopusPuppet.LogMessager;
using OctopusPuppet.Scheduler;

namespace OctopusPuppet.OctopusProvider
{
    public class OctopusLogMessager : ILogMessager
    {
        private readonly string _url;

        public OctopusLogMessager(string url)
        {
            _url = url;
        }

        private string GetName(ComponentDeploymentVertex componentDeploymentVertex)
        {
            return string.IsNullOrEmpty(componentDeploymentVertex.Name) ? componentDeploymentVertex.Id : componentDeploymentVertex.Name;
        }

        private string GetOctopusDeploymentUrl(ComponentDeploymentVertex componentDeploymentVertex)
        {
            if (componentDeploymentVertex == null || string.IsNullOrEmpty(componentDeploymentVertex.DeploymentId))
            {
                return null;
            }
            
            var deploymentUri = string.Format("{0}/app#/deployments/{1}", _url, componentDeploymentVertex.DeploymentId);
            return deploymentUri;
        }

        public string DeploymentSkipped(ComponentDeploymentVertex componentDeploymentVertex)
        {
            var name = GetName(componentDeploymentVertex);
            return string.Format("Deployment skipped for {0}", name);
        }

        public string DeploymentStarted(ComponentDeploymentVertex componentDeploymentVertex)
        {
            var name = GetName(componentDeploymentVertex);
            return string.Format("Deployment started for {0} - expected deployment duration {1}", name, componentDeploymentVertex.DeploymentDuration);
        }

        public string DeploymentFailed(ComponentDeploymentVertex componentDeploymentVertex, string errorMessage)
        {
            var name = GetName(componentDeploymentVertex);
            var deploymentUri = GetOctopusDeploymentUrl(componentDeploymentVertex);
            if (deploymentUri == null)
            {
                return string.Format("Deployment failed for {0}{1}{2}", name, Environment.NewLine, errorMessage);
            }
            return string.Format("Deployment failed for {0} - {1}{2}{3}", name, deploymentUri, Environment.NewLine, errorMessage);
        }

        public string DeploymentProgress(ComponentDeploymentVertex componentDeploymentVertex, string processingMessage)
        {
            var name = GetName(componentDeploymentVertex);
            var deploymentUri = GetOctopusDeploymentUrl(componentDeploymentVertex);
            if (deploymentUri == null)
            {
                return string.Format("Deploying {0}{1}{2}", name, Environment.NewLine, processingMessage);
            }
            return string.Format("Deploying {0} - {1}{2}{3}", name, deploymentUri, Environment.NewLine, processingMessage);
        }

        public string DeploymentCancelled(ComponentDeploymentVertex componentDeploymentVertex)
        {
            var name = GetName(componentDeploymentVertex);
            var deploymentUri = GetOctopusDeploymentUrl(componentDeploymentVertex);
            if (deploymentUri == null)
            {
                return string.Format("Deployment cancelled for {0}", name);
            }
            return string.Format("Deployment cancelled for {0} - {1}", name, deploymentUri);
        }

        public string DeploymentSuccess(ComponentDeploymentVertex componentDeploymentVertex)
        {
            var name = GetName(componentDeploymentVertex);
            var deploymentUri = GetOctopusDeploymentUrl(componentDeploymentVertex);
            if (deploymentUri == null)
            {
                return string.Format("Deployment succeeded for {0}", name);
            }
            return string.Format("Deployment succeeded for {0} - {1}", name, deploymentUri);
        }
    }
}
