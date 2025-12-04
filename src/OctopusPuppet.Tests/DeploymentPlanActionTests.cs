using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using OctopusPuppet.DeploymentPlanner;
using OctopusPuppet.OctopusProvider;
using OctopusPuppet.Tests.TestHelpers;
using Xunit;

namespace OctopusPuppet.Tests
{
    [Collection("NonParallelCollection")]
    public class DeploymentPlanActionTests
    {
        private const string EnvironmentId = "D1";

        private static TestComponent[] GetComponents() => new[]
        {
            new TestComponent { ProjectName = "ArmSharedInfrastructure", Version = "1.2.3456" },
            new TestComponent { ProjectName = "Filebeat", Version = "2.3.456" },
        };



        [Theory]
        [InlineData("1.2.3456")]
        [InlineData("2.3.456")]
        public void GetBranchDeploymentPlan_Should_Skip_MasterBuilds_When_SkipNoBranchSuffix_IsTrue(string branch)
        {
            // GIVEN: master builds only and a filter restricting to relevant projects
            var components = GetComponents();
            var filter = new ComponentFilter
            {
                Include = true,
                Expressions = new List<string> { "(?i)^(ArmSharedInfrastructure|Filebeat)$" }
            };

            var planner = DeploymentPlannerTestFactory.GetSutForComponents(components, EnvironmentId);

            // WHEN: skipNoBranchSuffix = true
            var result = planner.GetBranchDeploymentPlans(EnvironmentId, branch, false, true, filter);
            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            // THEN: only master builds appear, and all are skipped
            plans.Should().OnlyContain(p => string.IsNullOrWhiteSpace(p.ComponentFrom.Version.SpecialVersion),
                "versions with no special suffix are master builds");

            plans.Should().OnlyContain(p => p.Action == PlanAction.Skip,
                "skipNoBranchSuffix=true means master builds are skipped");
        }


        [Theory]
        [InlineData("1.2.3456")]
        [InlineData("2.3.456")]
        public void DeploymentPlanner_ShouldDeploy_MasterBuilds_When_SkipNoBranchSuffix_IsFalse(string branch)
        {
            // GIVEN: master builds only and a filter restricting to relevant projects
            var components = GetComponents();
            var filter = new ComponentFilter
            {
                Include = true,
                Expressions = new List<string> { "(?i)^(ArmSharedInfrastructure|Filebeat)$" }
            };
            var planner = DeploymentPlannerTestFactory.GetSutForComponents(components, EnvironmentId);

            // WHEN: skipNoBranchSuffix = false
            var result = planner.GetBranchDeploymentPlans(EnvironmentId, branch, false, false, filter);
            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            // THEN: master builds appear, and all are deployed
            plans.Should().NotBeEmpty();

            plans.Should().OnlyContain(p => string.IsNullOrWhiteSpace(p.ComponentFrom.Version.SpecialVersion),
                "versions with no special suffix are master builds");

            plans.Should().OnlyContain(p => p.Action == PlanAction.Change,
                "skipNoBranchSuffix=false means master builds are deployed");
        }


        [Theory]
        [InlineData("ArmSharedInfrastructure", "1.2.3456", true)]
        [InlineData("Filebeat", "2.3.456", true)]
        [InlineData("NonRelevantProject", "1.0.0", false)]
        public void DeploymentPlanner_ComponentFilter_ShouldOnlyIncludeMatchingComponents(
            string projectName, string branch, bool shouldBeIncluded)
        {
            // GIVEN: mixed components (some matching, some not) and a filter
            var components = GetComponents();
            var filter = new ComponentFilter
            {
                Include = true,
                Expressions = new List<string> { "(?i)^(ArmSharedInfrastructure|Filebeat)$" }
            };
            var planner = DeploymentPlannerTestFactory.GetSutForComponents(components, EnvironmentId);

            // WHEN: retrieving deployment plans for a given branch
            var result = planner.GetBranchDeploymentPlans(EnvironmentId, branch, false, false, filter);
            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            // THEN: all returned plans must match the filter
            var expectedProjects = components
                .Where(c => filter.Expressions.Any(pattern =>
                    System.Text.RegularExpressions.Regex.IsMatch(c.ProjectName, pattern)))
                .Select(c => c.ProjectName);

            plans.Select(p => p.Name)
                 .Should()
                 .BeSubsetOf(expectedProjects);

            // THEN: project-specific expectation
            if (shouldBeIncluded)
                plans.Should().Contain(p => p.Name == projectName);
            else
                plans.Should().NotContain(p => p.Name == projectName);
        }
    }
}
