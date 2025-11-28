using NSubstitute;
using Octopus.Client.Model;
using Octopus.Client.Repositories;
using Octopus.Client;
using OctopusPuppet.DeploymentPlanner;
using OctopusPuppet.OctopusProvider;
using System;
using System.Collections.Generic;

namespace OctopusPuppet.Tests.TestHelpers
{
    public static class DeploymentPlannerTestFactory
    {
        public static OctopusDeploymentPlanner GetSutForVersion(string versionNumber, string environmentName = "D1")
        {
            var repo = Substitute.For<IOctopusRepository>();

            // Projects
            var project1 = new ProjectResource { Id = "124", Name = "ArmSharedInfrastructure", IsDisabled = false };
            var project2 = new ProjectResource { Id = "125", Name = "Filebeat", IsDisabled = false };
            var project3 = new ProjectResource { Id = "126", Name = "NonRelevantProject", IsDisabled = false };

            repo.Projects.Get(Arg.Any<string>()).Returns(project1);
            repo.Projects.FindAll().Returns(new List<ProjectResource> { project1, project2, project3 });
            repo.Projects.GetAll().Returns(new List<ReferenceDataItem>
            {
                new ReferenceDataItem("124",""),
                new ReferenceDataItem("125",""),
                new ReferenceDataItem("126","")
            });

            // Releases
            repo.Projects.GetReleases(Arg.Any<ProjectResource>())
                .Returns(args =>
                {
                    var project = args.Arg<ProjectResource>();
                    return new ResourceCollection<ReleaseResource>(
                        new[] { new ReleaseResource(versionNumber, project.Id, "") { Id = versionNumber, Version = versionNumber } },
                        new LinkCollection()
                    );
                });

            // Environments
            var envRepo = Substitute.For<IEnvironmentRepository>();
            envRepo.GetAll().Returns(new List<ReferenceDataItem> { new ReferenceDataItem("124", environmentName) });
            repo.Environments.Returns(envRepo);

            // Dashboard
            var dashboardRepo = Substitute.For<IDashboardRepository>();
            dashboardRepo.GetDynamicDashboard(Arg.Any<string[]>(), Arg.Any<string[]>())
                .Returns(new DashboardResource
                {
                    Projects = new List<DashboardProjectResource>
                    {
                        new DashboardProjectResource { Id = "124", Name = "ArmSharedInfrastructure" },
                        new DashboardProjectResource { Id = "125", Name = "Filebeat" },
                        new DashboardProjectResource { Id = "126", Name = "NonRelevantProject" }
                    },
                    Items = new List<DashboardItemResource>
                    {
                        new DashboardItemResource { ProjectId = "124", EnvironmentId = environmentName, ReleaseVersion = versionNumber },
                        new DashboardItemResource { ProjectId = "125", EnvironmentId = environmentName, ReleaseVersion = versionNumber },
                        new DashboardItemResource { ProjectId = "126", EnvironmentId = environmentName, ReleaseVersion = versionNumber }
                    }
                });
            repo.Dashboards.Returns(dashboardRepo);

            // Variable Sets
            var variableSetRepo = Substitute.For<IVariableSetRepository>();
            variableSetRepo.Get(Arg.Any<string>()).Returns(new VariableSetResource
            {
                Variables = new List<VariableResource> { new VariableResource { Id = Guid.NewGuid().ToString(), Name = "test", Value = "Testing var" } }
            });
            repo.VariableSets.Returns(variableSetRepo);

            return new OctopusDeploymentPlanner(repo);
        }

        public static ComponentFilter ComponentFilterForTarget() =>
            new ComponentFilter
            {
                Include = true,
                Expressions = new List<string> { "(?i)^(ArmSharedInfrastructure|Filebeat)$" }
            };
    }
}
