using System.Configuration;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using OctopusPuppet.DeploymentPlanner;
using OctopusPuppet.OctopusProvider;

namespace OctopusPuppet.IntegrationTests
{
    public class OctopusDeploymentPlannerTests
    {
        [Test]
        public void GetEnvironments()
        {
            var octopusUrl = ConfigurationManager.AppSettings["OctopusUrl"];
            var octopusApiKey = ConfigurationManager.AppSettings["OctopusApiKey"];
            var deploymentPlanner = new OctopusDeploymentPlanner(octopusUrl, octopusApiKey);

            var environments = deploymentPlanner.GetEnvironments();

            Assert.Greater(0, environments.Count);
        }

        [Test]
        public void GetBranches()
        {
            var octopusUrl = ConfigurationManager.AppSettings["OctopusUrl"];
            var octopusApiKey = ConfigurationManager.AppSettings["OctopusApiKey"];
            var deploymentPlanner = new OctopusDeploymentPlanner(octopusUrl, octopusApiKey);

            var branches = deploymentPlanner.GetBranches();

            Assert.Greater(0, branches.Count);        
        }

        [Test]
        public void GetEnvironmentDeploymentPlans()
        {
            var octopusUrl = ConfigurationManager.AppSettings["OctopusUrl"];
            var octopusApiKey = ConfigurationManager.AppSettings["OctopusApiKey"];
            var deploymentPlanner = new OctopusDeploymentPlanner(octopusUrl, octopusApiKey);

            var environmentFrom = ConfigurationManager.AppSettings["EnvironmentFrom"];
            var environmentTo = ConfigurationManager.AppSettings["EnvironmentTo"];

            var dashboard = deploymentPlanner.GetEnvironmentDeploymentPlans(environmentFrom, environmentTo);

            var difference = JsonConvert.SerializeObject(dashboard.DeploymentPlans.Where(x => x.Action != PlanAction.Skip));

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
