using System.Configuration;
using NUnit.Framework;
using OctopusPuppet.OctopusProvider;

namespace OctopusPuppet.IntegrationTests
{
    public class OctopusDeploymentPlannerTests
    {
        [Test]
        public void GetEnvironmentDeploymentPlans()
        {
            var octopusUrl = ConfigurationManager.AppSettings["OctopusUrl"];
            var octopusApiKey = ConfigurationManager.AppSettings["OctopusApiKey"];
            var deploymentPlanner = new OctopusDeploymentPlanner(octopusUrl, octopusApiKey);

            var environmentFrom = ConfigurationManager.AppSettings["EnvironmentFrom"];
            var environmentTo = ConfigurationManager.AppSettings["EnvironmentTo"];

            var dashboard = deploymentPlanner.GetEnvironmentDeploymentPlans(environmentFrom, environmentTo);

            Assert.AreEqual(2, dashboard.DeploymentPlans.Count);
        }

        [Test]
        public void GetBranchDeploymentPlans()
        {
            var octopusUrl = ConfigurationManager.AppSettings["OctopusUrl"];
            var octopusApiKey = ConfigurationManager.AppSettings["OctopusApiKey"];
            var deploymentPlanner = new OctopusDeploymentPlanner(octopusUrl, octopusApiKey);

            var environment = ConfigurationManager.AppSettings["EnvironmentFrom"];
            var branch = "Master";

            var dashboard = deploymentPlanner.GetBranchDeploymentPlans(environment, branch);

            Assert.AreEqual(2, dashboard.DeploymentPlans.Count);
        }

        [Test]
        public void GetRedeployDeploymentPlans()
        {
            var octopusUrl = ConfigurationManager.AppSettings["OctopusUrl"];
            var octopusApiKey = ConfigurationManager.AppSettings["OctopusApiKey"];
            var deploymentPlanner = new OctopusDeploymentPlanner(octopusUrl, octopusApiKey);

            var environment = ConfigurationManager.AppSettings["EnvironmentFrom"];

            var dashboard = deploymentPlanner.GetRedeployDeploymentPlans(environment);

            Assert.AreEqual(2, dashboard.DeploymentPlans.Count);
        }
    }
}
