using OctopusPuppet.Deployer;
using OctopusPuppet.LogMessager;

namespace OctopusPuppet.OctopusProvider
{
    public class OctopusLogMessager : ILogMessager
    {
        private readonly string _url;

        public OctopusLogMessager(string url)
        {
            _url = url;
        }

        private string GetName(ComponentVertexDeploymentProgress value)
        {
            return string.IsNullOrEmpty(value.Vertex.Name) ? value.Vertex.Id : value.Vertex.Name;
        }

        private string GetOctopusDeploymentUrl(ComponentVertexDeploymentProgress value)
        {
            if (value.Vertex == null || string.IsNullOrEmpty(value.Vertex.DeploymentId))
            {
                return null;
            }
            
            var deploymentUri = string.Format("{0}/app#/deployments/{1}", _url, value.Vertex.DeploymentId);
            return deploymentUri;
        }

        public string DeploymentStarted(ComponentVertexDeploymentProgress componentVertexDeploymentProgress)
        {
            var name = GetName(componentVertexDeploymentProgress);
            return string.Format("Deployment started for {0}", name);
        }

        public string DeploymentFailed(ComponentVertexDeploymentProgress componentVertexDeploymentProgress)
        {
            var name = GetName(componentVertexDeploymentProgress);
            var deploymentUri = GetOctopusDeploymentUrl(componentVertexDeploymentProgress);
            if (deploymentUri == null)
            {
                return string.Format("Deployment failed for {0}", name);
            }
            return string.Format("Deployment failed for {0} - {1}", name, deploymentUri);
        }

        public string DeploymentCancelled(ComponentVertexDeploymentProgress componentVertexDeploymentProgress)
        {
            var name = GetName(componentVertexDeploymentProgress);
            var deploymentUri = GetOctopusDeploymentUrl(componentVertexDeploymentProgress);
            if (deploymentUri == null)
            {
                return string.Format("Deployment cancelled for {0}", name);
            }
            return string.Format("Deployment cancelled for {0} - {1}", name, deploymentUri);
        }

        public string DeploymentSuccess(ComponentVertexDeploymentProgress componentVertexDeploymentProgress)
        {
            var name = GetName(componentVertexDeploymentProgress);
            var deploymentUri = GetOctopusDeploymentUrl(componentVertexDeploymentProgress);
            if (deploymentUri == null)
            {
                return string.Format("Deployment succeeded for {0}", name);
            }
            return string.Format("Deployment succeeded for {0} - {1}", name, deploymentUri);
        }
    }
}
