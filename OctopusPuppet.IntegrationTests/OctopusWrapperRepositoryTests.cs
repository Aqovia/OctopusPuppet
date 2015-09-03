using System.Configuration;
using NUnit.Framework;
using OctopusPuppet.DeploymentHistory;

namespace OctopusPuppet.Tests
{
    public class OctopusWrapperRepositoryTests
    {
        [Test]
        public void GetEnvironmentDeploymentPlans()
        {
            var octopusUrl = ConfigurationManager.AppSettings["OctopusUrl"];
            var octopusApiKey = ConfigurationManager.AppSettings["OctopusApiKey"];
            var repository = new OctopusWrapperRepository(octopusUrl, octopusApiKey);

            var environmentFrom = ConfigurationManager.AppSettings["EnvironmentFrom"];
            var environmentTo = ConfigurationManager.AppSettings["EnvironmentTo"];

            var dashboard = repository.GetEnvironmentDeploymentPlans(environmentFrom, environmentTo);

            Assert.AreEqual(2, dashboard.DeploymentPlans.Count);
        }

        [Test]
        public void GetBranchDeploymentPlans()
        {
            var octopusUrl = ConfigurationManager.AppSettings["OctopusUrl"];
            var octopusApiKey = ConfigurationManager.AppSettings["OctopusApiKey"];
            var repository = new OctopusWrapperRepository(octopusUrl, octopusApiKey);

            var environment = ConfigurationManager.AppSettings["EnvironmentFrom"];
            var branch = "Master";

            var dashboard = repository.GetBranchDeploymentPlans(environment, branch);

            Assert.AreEqual(2, dashboard.DeploymentPlans.Count);
        }

        [Test]
        public void GetRedeployDeploymentPlans()
        {
            var octopusUrl = ConfigurationManager.AppSettings["OctopusUrl"];
            var octopusApiKey = ConfigurationManager.AppSettings["OctopusApiKey"];
            var repository = new OctopusWrapperRepository(octopusUrl, octopusApiKey);

            var environment = ConfigurationManager.AppSettings["EnvironmentFrom"];

            var dashboard = repository.GetRedeployDeploymentPlans(environment);

            Assert.AreEqual(2, dashboard.DeploymentPlans.Count);
        }
    }
}
