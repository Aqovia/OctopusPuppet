using System.Configuration;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using OctopusPuppet.DeploymentPlanner;
using OctopusPuppet.OctopusProvider;
using OctopusPuppet.Scheduler;
using Xunit;

namespace OctopusPuppet.IntegrationTests
{
    public class OctopusDeploymentPlannerTests
    {
        [Fact]
        public void GetEnvironments()
        {
            var octopusUrl = ConfigurationManager.AppSettings["OctopusUrl"];
            var octopusApiKey = ConfigurationManager.AppSettings["OctopusApiKey"];
            var deploymentPlanner = new OctopusDeploymentPlanner(octopusUrl, octopusApiKey);

            var environments = deploymentPlanner.GetEnvironments();

            environments.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public void GetBranches()
        {
            var octopusUrl = ConfigurationManager.AppSettings["OctopusUrl"];
            var octopusApiKey = ConfigurationManager.AppSettings["OctopusApiKey"];
            var deploymentPlanner = new OctopusDeploymentPlanner(octopusUrl, octopusApiKey);

            var branches = deploymentPlanner.GetBranches();

            branches.Count.Should().BeGreaterThan(0);       
        }

        [Fact]
        public void GetEnvironmentMirrorDeploymentPlans()
        {
            var octopusUrl = ConfigurationManager.AppSettings["OctopusUrl"];
            var octopusApiKey = ConfigurationManager.AppSettings["OctopusApiKey"];
            var deploymentPlanner = new OctopusDeploymentPlanner(octopusUrl, octopusApiKey);

            var environmentFrom = ConfigurationManager.AppSettings["EnvironmentFrom"];
            var environmentTo = ConfigurationManager.AppSettings["EnvironmentTo"];

            var dashboard = deploymentPlanner.GetEnvironmentMirrorDeploymentPlans(environmentFrom, environmentTo, false);

            var deploymentScheduler = new DeploymentScheduler();
            var products = deploymentScheduler.GetComponentDeploymentGraph(dashboard.EnvironmentDeploymentPlan);

            var difference = JsonConvert.SerializeObject(dashboard.EnvironmentDeploymentPlan.DeploymentPlans.Where(x => x.Action != PlanAction.Skip));

            dashboard.EnvironmentDeploymentPlan.DeploymentPlans.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public void GetBranchDeploymentPlans()
        {
            var octopusUrl = ConfigurationManager.AppSettings["OctopusUrl"];
            var octopusApiKey = ConfigurationManager.AppSettings["OctopusApiKey"];
            var deploymentPlanner = new OctopusDeploymentPlanner(octopusUrl, octopusApiKey);

            var environment = ConfigurationManager.AppSettings["EnvironmentFrom"];
            var branch = "Master";

            var dashboard = deploymentPlanner.GetBranchDeploymentPlans(environment, branch, false);

            var difference = JsonConvert.SerializeObject(dashboard.EnvironmentDeploymentPlan.DeploymentPlans.Where(x => x.Action != PlanAction.Skip));

            dashboard.EnvironmentDeploymentPlan.DeploymentPlans.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public void GetRedeployDeploymentPlans()
        {
            var octopusUrl = ConfigurationManager.AppSettings["OctopusUrl"];
            var octopusApiKey = ConfigurationManager.AppSettings["OctopusApiKey"];
            var deploymentPlanner = new OctopusDeploymentPlanner(octopusUrl, octopusApiKey);

            var environment = ConfigurationManager.AppSettings["EnvironmentFrom"];

            var dashboard = deploymentPlanner.GetRedeployDeploymentPlans(environment);

            var difference = JsonConvert.SerializeObject(dashboard.EnvironmentDeploymentPlan.DeploymentPlans.Where(x => x.Action != PlanAction.Skip));

            dashboard.EnvironmentDeploymentPlan.DeploymentPlans.Count.Should().BeGreaterThan(0);
        }
    }
}
