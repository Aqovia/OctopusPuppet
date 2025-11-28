using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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

        private static OctopusDeploymentPlanner GetSutForVersion(string versionNumber, string environmentName)
        {
            var repo = Substitute.For<IOctopusRepository>();

            //  Projects 
            var project = new ProjectResource { Id = "124", Name = "ArmSharedInfrastructure", IsDisabled = false };

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
                        new DashboardProjectResource { Id = "124", Name = "ArmSharedInfrastructure" }
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

        private static ComponentFilter ComponentFilterForTarget() =>
            new ComponentFilter
            {
                Include = true,
                Expressions = new List<string> { "(?i)^(ArmSharedInfrastructure)$" }
            };

        [Fact]
        public void DeploymentPlanner_ShouldSkipNoBranchSuffix_WhenFlagIsTrue()
        {
            var version = "1.2.3456";
            var env = "D1";

            var planner = GetSutForVersion(version, env);
            var result = planner.GetBranchDeploymentPlans(env, version, false, true, ComponentFilterForTarget());

            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            plans.Should().NotBeEmpty();
            plans.Should().OnlyContain(p => p.Name == "ArmSharedInfrastructure");
            plans.Should().OnlyContain(p => string.IsNullOrWhiteSpace(p.ComponentFrom.Version.SpecialVersion));
            plans.Should().OnlyContain(p => p.Action == PlanAction.Skip);
        }

        [Fact]
        public void DeploymentPlanner_ShouldDeployNoBranchSuffix_WhenFlagIsFalse()
        {
            var version = "1.2.3456";
            var env = "D1";

            var planner = GetSutForVersion(version, env);
            var result = planner.GetBranchDeploymentPlans(env, version, false, false, ComponentFilterForTarget());

            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            plans.Should().NotBeEmpty();
            plans.Should().OnlyContain(p => p.Name == "ArmSharedInfrastructure");
            plans.Should().OnlyContain(p => string.IsNullOrWhiteSpace(p.ComponentFrom.Version.SpecialVersion));
            plans.Should().OnlyContain(p => p.Action == PlanAction.Change);
        }

        [Theory]
        [InlineData("ArmSharedInfrastructure", true)]
        [InlineData("Filebeat", false)]
        [InlineData("NonExistingComponent", false)]
        public void DeploymentPlanner_ComponentFilter_ShouldOnlyIncludeMatchingComponents(string projectName, bool shouldBeIncluded)
        {
            var version = "1.2.3456";
            var env = "D1";

            var planner = GetSutForVersion(version, env);

            var result = planner.GetBranchDeploymentPlans(env, version, false, false, ComponentFilterForTarget());
            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            if (shouldBeIncluded)
            {
                plans.Should().ContainSingle(p => p.Name == projectName);
            }
            else
            {
                plans.Should().NotContain(p => p.Name == projectName);
            }
        } 
    }
}