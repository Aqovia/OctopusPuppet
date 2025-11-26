using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NSubstitute;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Client.Repositories;
using OctopusPuppet.DeploymentPlanner;
using OctopusPuppet.OctopusProvider;
using Xunit;
namespace OctopusPuppet.Tests
{

    public class DeploymentPlanActionTests
    {

        private static OctopusDeploymentPlanner GetSutForVersionTest(string versionNumber, string environmentName)
        {
            var repo = Substitute.For<IOctopusRepository>();

            //  Projects 
            var project = new ProjectResource { Id = "124", Name = "Test", IsDisabled = false };
            repo.Projects.Get(Arg.Any<string>()).Returns(project);
            repo.Projects.FindAll().Returns(_ => new List<ProjectResource> { project });
            repo.Projects.GetAll().Returns(_ => new List<ReferenceDataItem> { new ReferenceDataItem("124", "") });

            //  Releases  
            repo.Projects.GetReleases(Arg.Any<ProjectResource>()).Returns(_ => new ResourceCollection<ReleaseResource>(
                new[]
                {
                    new ReleaseResource(versionNumber, "124", "")
                    {
                        Id = versionNumber,
                        Version = versionNumber,
                        ProjectVariableSetSnapshotId = "fake-variable-set-id",
                        ProjectDeploymentProcessSnapshotId = Guid.NewGuid().ToString()
                    }
                },
                new LinkCollection()
            ));

            //  Environments 
            var envRepo = Substitute.For<IEnvironmentRepository>();
            envRepo.GetAll().Returns(_ => new List<ReferenceDataItem>
            {
                new ReferenceDataItem("124", environmentName)
            });
            repo.Environments.Returns(envRepo);

            // Dashboard 
            var dashboardRepo = Substitute.For<IDashboardRepository>();
            dashboardRepo.GetDynamicDashboard(Arg.Any<string[]>(), Arg.Any<string[]>())
                .Returns(_ => new DashboardResource
                {
                    Projects = new List<DashboardProjectResource>
                    {
                        new DashboardProjectResource { Id = "124", Name = "arminfra" }
                    },
                    Items = new List<DashboardItemResource>
                    {
                        new DashboardItemResource
                        {
                            ProjectId = "124",
                            EnvironmentId = environmentName,
                            ReleaseVersion = versionNumber,
                            State = TaskState.Success,
                            QueueTime = DateTime.UtcNow.AddMinutes(-5),
                            CompletedTime = DateTime.UtcNow
                        }
                    }
                });
            repo.Dashboards.Returns(dashboardRepo);

            // Variable Sets
            var variableSetRepo = Substitute.For<IVariableSetRepository>();
            variableSetRepo.Get(Arg.Any<string>()).Returns(_ => new VariableSetResource
            {
                Variables = new List<VariableResource>
                {
                    new VariableResource { Id = Guid.NewGuid().ToString(), Name = "test", Value = "Testing var" }
                }
            });
            repo.VariableSets.Returns(variableSetRepo);

            return new OctopusDeploymentPlanner(repo);
        }

        [Fact]

        public void DeploymentPlanner_Should_SkipMaster_WhenFlagIsTrue()
        {
            var planner = GetSutForVersionTest("1.2.3456", "D1");

            var result = planner.GetBranchDeploymentPlans("D1", "1.2.3456", false, true,
                new ComponentFilter
                {
                    Expressions = new List<string> { "(?i)^(ArmSharedInfrastructure|Filebeat)$" }
                });

            var masterPlan = result.EnvironmentDeploymentPlan.DeploymentPlans.First();
            masterPlan.Action.Should().Be(PlanAction.Skip);
        }

        [Fact]
        public void DeploymentPlanner_Should_UseMaster_WhenFlagIsFalse()
        {
            var planner = GetSutForVersionTest("1.2.3456", "D1");

            var result = planner.GetBranchDeploymentPlans("D1", "1.2.3456", false, false,
                new ComponentFilter
                {
                    Expressions = new List<string> { "(?i)^(ArmSharedInfrastructure|Filebeat)$" }
                });

            var masterPlan = result.EnvironmentDeploymentPlan.DeploymentPlans.First();
            masterPlan.Action.Should().Be(PlanAction.Change);

        }
    }
}