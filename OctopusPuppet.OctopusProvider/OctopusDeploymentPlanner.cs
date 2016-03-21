using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Octopus.Client;
using Octopus.Client.Model;
using OctopusPuppet.DeploymentPlanner;

namespace OctopusPuppet.OctopusProvider
{
    public class OctopusDeploymentPlanner : IDeploymentPlanner
    {
        private const string ComponentDependancies = "ComponentDependencies";
        private readonly OctopusRepository _repository;

        public OctopusDeploymentPlanner(string url, string apiKey)
        {
            var octopusServerEndpoint = new OctopusServerEndpoint(url, apiKey);
            _repository = new OctopusRepository(octopusServerEndpoint);
        }

        private List<ReleaseResource> GetReleaseResources(string projectId)
        {
            var project = _repository.Projects.Get(projectId);

            var releases = new List<ReleaseResource>();
            var skip = 0;
            var shouldPage = false;
            do
            {
                var releasePages = _repository.Projects.GetReleases(project, skip);
                releases.AddRange(releasePages.Items);
                skip += releasePages.ItemsPerPage;
                shouldPage = releasePages.TotalResults > skip;
            } while (shouldPage);

            return releases;
        }

        private List<DeploymentResource> GetDeploymentResources(string projectId)
        {
            var projects = new[] { projectId };
            var environments = new string[] { };

            var deployments = new List<DeploymentResource>();
            var skip = 0;
            var shouldPage = false;
            do
            {
                var releasePages = _repository.Deployments.FindAll(projects, environments, skip);
                deployments.AddRange(releasePages.Items);
                skip += releasePages.ItemsPerPage;
                shouldPage = releasePages.TotalResults > skip;
            } while (shouldPage);

            return deployments;
        }

        private DashboardItemResource GetClosestMatchingDashboardItemResource(DashboardResource dashboard, string environmentId, string projectId, string branch)
        {
            var dashboardItemResources = dashboard.Items
                .Where(x => x.ProjectId == projectId);

            if (dashboardItemResources.Any(x => x.EnvironmentId == environmentId))
            {
                dashboardItemResources = dashboardItemResources
                    .Where(x => x.EnvironmentId == environmentId);
            }

            if (dashboardItemResources.Any(x => (new SemanticVersion(x.ReleaseVersion)).SpecialVersion == branch))
            {
                dashboardItemResources = dashboardItemResources
                    .Where(x => (new SemanticVersion(x.ReleaseVersion)).SpecialVersion == branch);
            }

            var dashboardItemResource = dashboardItemResources
                .FirstOrDefault();

            return dashboardItemResource;
        }

        /// <summary>
        /// Get branches for any release
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        private List<string> GetBranchesForProject(string projectId)
        {
            var branches = GetReleaseResources(projectId)
                .Select(x=>new SemanticVersion(x.Version).SpecialVersion)
                .Distinct()
                .ToList();

            return branches;
        }


        /// <summary>
        /// Get component for a branch. If release does not exist for branch then master branch is assumed.
        /// </summary>
        /// <param name="environmentId"></param>
        /// <param name="projectId"></param>
        /// <param name="branch"></param>
        /// <param name="dashboard"></param>
        /// <returns></returns>
        private Component GetComponentForBranch(DashboardResource dashboard, string environmentId, string projectId, string branch)
        {
            var releaseResources = GetReleaseResources(projectId);

            if (branch == null || branch.Equals("Master", StringComparison.InvariantCultureIgnoreCase))
            {
                branch = string.Empty;
            }

            //If we dont have any release in a branch assume master
            if (!string.IsNullOrEmpty(branch) && releaseResources.All(x => (new SemanticVersion(x.Version)).SpecialVersion != branch))
            {
                branch = string.Empty;
            }

            //Get release for branch
            var releaseResource = releaseResources.FirstOrDefault(x => (new SemanticVersion(x.Version)).SpecialVersion == branch);
            if (releaseResource == null) return null;

            // No other way to calculate duration otherwise :<
            var dashboardItemResource = GetClosestMatchingDashboardItemResource(dashboard, environmentId, projectId, branch);

            var componentDeployedOnEnvironmentFromDuration = dashboardItemResource == null 
                ? null 
                : dashboardItemResource.CompletedTime - dashboardItemResource.QueueTime;
            
            var projectVariables = _repository.VariableSets.Get(releaseResource.ProjectVariableSetSnapshotId);

            var componentDependancies = projectVariables.Variables
                .Where(x => x.Name == ComponentDependancies)
                .SelectMany(x => JsonConvert.DeserializeObject<string[]>(x.Value))
                .ToList();

            var component = new Component
            {
                Version = new SemVer(releaseResource.Version),
                DeploymentDuration = componentDeployedOnEnvironmentFromDuration,
                Dependancies = componentDependancies
            };

            return component;
        }

        /// <summary>
        /// Get a component for an environment
        /// </summary>
        /// <param name="dashboard"></param>
        /// <param name="environmentId"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        private Component GetComponentForEnvironment(DashboardResource dashboard, string environmentId, string projectId)
        {
            var dashboardItemResources = dashboard.Items
                .Where(x => x.EnvironmentId == environmentId && x.ProjectId == projectId);

            var dashboardItemResource = dashboardItemResources.FirstOrDefault();

            if (dashboardItemResource == null) return null;

            var componentDeployedOnEnvironmentFromDuration = dashboardItemResource.CompletedTime -
                                                             dashboardItemResource.QueueTime;

            var release = _repository.Releases.Get(dashboardItemResource.ReleaseId);
            var projectVariables = _repository.VariableSets.Get(release.ProjectVariableSetSnapshotId);

            var componentDependancies = projectVariables.Variables
                .Where(x => x.Name == ComponentDependancies)
                .SelectMany(x => JsonConvert.DeserializeObject<string[]>(x.Value))
                .ToList();

            var component = new Component
            {
                Version = new SemVer(dashboardItemResource.ReleaseVersion),
                DeploymentDuration = componentDeployedOnEnvironmentFromDuration,
                Dependancies = componentDependancies
            };

            return component;
        }

        private DeploymentPlan GetEnvironmentDeploymentPlan(string projectName, Component componentFrom, Component componentTo)
        {
            var deploymentPlan = new DeploymentPlan
            {
                Name = projectName,
                ComponentFrom = componentFrom,
                ComponentTo = componentTo
            };

            if (componentFrom == null && componentTo == null)
            {
                deploymentPlan.Action = PlanAction.Skip;
            }
            else if (componentFrom == null)
            {
                deploymentPlan.Action = PlanAction.Remove;
            }
            else if (componentTo == null)
            {
                deploymentPlan.Action = PlanAction.Change;
            }
            else if (componentFrom.Version == componentTo.Version)
            {
                deploymentPlan.Action = PlanAction.Skip;
            }
            else
            {
                deploymentPlan.Action = PlanAction.Change;
            }

            return deploymentPlan;
        }

        public List<string> GetEnvironments()
        {
            var environments = _repository.Environments
                .GetAll()
                .Select(x => x.Id)
                .ToList();

            return environments;
        }

        public List<string> GetBranches()
        {
            var branches = new List<string>();

            var projectIds = _repository.Projects.GetAll().Select(x=>x.Id);

            foreach (var projectId in projectIds)
            {
                branches.AddRange(GetBranchesForProject(projectId));
            }

            branches = branches.Distinct().ToList();

            return branches;
        }

        public EnvironmentDeploymentPlans GetEnvironmentDeploymentPlans(string environmentFrom, string environmentTo)
        {
            var environments = environmentFrom == environmentTo ? 
                new[] {environmentFrom} : 
                new[] {environmentFrom, environmentTo};

            var environmentIds = _repository.Environments
                .GetAll()
                .Where(x=>environments.Contains(x.Name))
                .Select(x=>x.Id)
                .ToArray();

            var environmentFromId = environmentIds[0];
            var environmentToId = environmentFromId;

            if (environmentIds.Length > 1)
            {
                environmentToId = environmentIds[1];
            }

            if (environmentIds.Length != environments.Length)
            {
                throw new Exception("Unable to find environment");
            }

            var dashboard = _repository.Dashboards.GetDynamicDashboard(null, environmentIds);

            var environmentDeploymentPlan = new EnvironmentDeploymentPlans
            {
                EnvironmentFromId = environmentFromId,
                EnvironmentToId = environmentToId
            };

            foreach (var dashboardProjectResource in dashboard.Projects)
            {
                var projectId = dashboardProjectResource.Id;
                var projectName = dashboardProjectResource.Name;

                var componentFrom = GetComponentForEnvironment(dashboard, environmentFromId, projectId);                

                var componentTo = environmentFromId == environmentToId 
                    ? componentFrom 
                    : GetComponentForEnvironment(dashboard, environmentToId, projectId);

                var deploymentPlan = GetEnvironmentDeploymentPlan(projectName, componentFrom, componentTo);

                environmentDeploymentPlan.DeploymentPlans.Add(deploymentPlan);
            }

            return environmentDeploymentPlan;
        }


        public BranchDeploymentPlans GetBranchDeploymentPlans(string environment, string branch)
        {
            var environments = new[] { environment };

            var environmentIds = _repository.Environments
                .GetAll()
                .Where(x => environments.Contains(x.Name))
                .Select(x => x.Id)
                .ToArray();

            if (environmentIds.Length != 1)
            {
                throw new Exception("Unable to find environment");
            }

            var environmentId = environmentIds[0];
            
            var dashboard = _repository.Dashboards.GetDynamicDashboard(null, environmentIds);

            var branchDeploymentPlan = new BranchDeploymentPlans
            {
                EnvironmentId = environmentId,
                Branch = branch
            };

            foreach (var dashboardProjectResource in dashboard.Projects)
            {
                var projectId = dashboardProjectResource.Id;
                var projectName = dashboardProjectResource.Name;

                var componentFrom = GetComponentForBranch(dashboard, environmentId, projectId, branch);
                var componentTo = GetComponentForEnvironment(dashboard, environmentId, projectId);
                    
                var deploymentPlan = GetEnvironmentDeploymentPlan(projectName, componentFrom, componentTo);

                branchDeploymentPlan.DeploymentPlans.Add(deploymentPlan);
            }

            return branchDeploymentPlan;
        }

        public RedeployDeploymentPlans GetRedeployDeploymentPlans(string environment)
        {
            var environments = new[] { environment };

            var environmentIds = _repository.Environments
                .GetAll()
                .Where(x => environments.Contains(x.Name))
                .Select(x => x.Id)
                .ToArray();

            if (environmentIds.Length != 1)
            {
                throw new Exception("Unable to find environment");
            }

            var environmentId = environmentIds[0];

            var dashboard = _repository.Dashboards.GetDynamicDashboard(null, environmentIds);

            var redeployDeploymentPlans = new RedeployDeploymentPlans
            {
                EnvironmentId = environmentId
            };

            foreach (var dashboardProjectResource in dashboard.Projects)
            {
                var projectId = dashboardProjectResource.Id;
                var projectName = dashboardProjectResource.Name;

                var componentFrom = GetComponentForEnvironment(dashboard, environmentId, projectId);
                var componentTo = componentFrom;

                var deploymentPlan = GetEnvironmentDeploymentPlan(projectName, componentFrom, componentTo);

                //If the component is installed. Then re-install it
                if (componentFrom != null)
                {
                    deploymentPlan.Action = PlanAction.Change;
                }

                redeployDeploymentPlans.DeploymentPlans.Add(deploymentPlan);
            }

            return redeployDeploymentPlans;
        }
    }
}
