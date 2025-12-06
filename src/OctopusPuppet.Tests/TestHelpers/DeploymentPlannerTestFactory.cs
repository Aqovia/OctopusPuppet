using NSubstitute;
using Octopus.Client.Model;
using Octopus.Client.Repositories;
using Octopus.Client;
using OctopusPuppet.DeploymentPlanner;
using OctopusPuppet.OctopusProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OctopusPuppet.Tests.TestHelpers
{
    public class TestComponent
    {
        public string ProjectName { get; set; }
        public string Version { get; set; }
    }

    public static class DeploymentPlannerTestFactory
    {
        public static OctopusDeploymentPlanner GetSutForComponents(TestComponent[] components, string environmentName = "D1")
        {
            var repo = Substitute.For<IOctopusRepository>();

            // Projects
            var projectMap = new Dictionary<string, ProjectResource>();
            int idCounter = 100;
            foreach (var c in components)
            {
                if (!projectMap.ContainsKey(c.ProjectName))
                    projectMap[c.ProjectName] = new ProjectResource { Id = (idCounter++).ToString(), Name = c.ProjectName, IsDisabled= false };
            }

            var projects = projectMap.Values.ToList();
            repo.Projects.FindAll().Returns(projects);
            repo.Projects.GetAll().Returns(projects.Select(p => new ReferenceDataItem(p.Id, p.Name)).ToList());
            foreach (var p in projects)
            {
                repo.Projects.Get(p.Id).Returns(p);
            }



            // Releases
            repo.Projects.GetReleases(Arg.Any<ProjectResource>())
                .Returns(call =>
                {
                    var project = call.Arg<ProjectResource>();
                    var releases = components
                        .Where(c => c.ProjectName == project.Name)
                        .Select(c => new ReleaseResource(c.Version, project.Id, "") { Id = c.Version, Version = c.Version })
                        .ToArray();

                    return new ResourceCollection<ReleaseResource>(releases, new LinkCollection());
                });

            // Environments
            var envRepo = Substitute.For<IEnvironmentRepository>();
            envRepo.GetAll().Returns(new List<ReferenceDataItem> { new ReferenceDataItem("env1", environmentName) });
            repo.Environments.Returns(envRepo);

            // Dashboard
            var dashboardRepo = Substitute.For<IDashboardRepository>();
            dashboardRepo.GetDynamicDashboard(Arg.Any<string[]>(), Arg.Any<string[]>())
                .Returns(new DashboardResource
                {
                    Projects = projects.Select(p => new DashboardProjectResource { Id = p.Id, Name = p.Name }).ToList(),
                    Items = components.Select(c =>
                    {
                        var project = projectMap[c.ProjectName];
                        return new DashboardItemResource
                        {
                            ProjectId = project.Id,
                            EnvironmentId = environmentName,
                            ReleaseVersion = c.Version
                        };
                    }).ToList()
                });
            repo.Dashboards.Returns(dashboardRepo);

            // Variable Sets
            var variableSetRepo = Substitute.For<IVariableSetRepository>();
            variableSetRepo.Get(Arg.Any<string>()).Returns(new VariableSetResource
            {
                Variables = new List<VariableResource>        {
                new VariableResource { Id = Guid.NewGuid().ToString(), Name = "test", Value = "Testing var" }
        }
            });
            repo.VariableSets.Returns(variableSetRepo);

            return new OctopusDeploymentPlanner(repo);
        }
    }
}