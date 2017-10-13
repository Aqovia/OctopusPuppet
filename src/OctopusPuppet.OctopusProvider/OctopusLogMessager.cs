using System;
using Octopus.Client;
using OctopusPuppet.Deployer;
using OctopusPuppet.LogMessager;

namespace OctopusPuppet.OctopusProvider
{
    public class OctopusLogMessager : ILogMessager
    {
        private readonly string _url;
        private readonly IOctopusRepository _repository;

        public OctopusLogMessager(string url, string apiKey) : this(url, new OctopusRepository(new OctopusServerEndpoint(url, apiKey))) { }

        private OctopusLogMessager(string url, IOctopusRepository repository)
        {
            _url = url;
            _repository = repository;
        }

        private string GetName(ComponentVertexDeploymentProgress value)
        {
            return string.IsNullOrEmpty(value.Vertex.Name) ? value.Vertex.Id : value.Vertex.Name;
        }

        private Uri GetOctopusDeploymentUrl(ComponentVertexDeploymentProgress value)
        {
            if (string.IsNullOrEmpty(value.Vertex.DeploymentId))
            {
                return null;
            }

            var deployment = _repository.Deployments.Get(value.Vertex.DeploymentId);
            if (deployment == null)
            {
                return null;
            }
            var deploymentUri = new Uri(new Uri(_url), new Uri(deployment.Links["Web"].AsString()));
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
