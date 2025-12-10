using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using OctopusPuppet.DeploymentPlanner;
using OctopusPuppet.OctopusProvider;
using OctopusPuppet.Tests.TestHelpers;
using Xunit;

namespace OctopusPuppet.Tests
{
    public class DeploymentPlanActionTests
    {
        private const string EnvironmentId = "D1";

        [Theory]
        [InlineData(true, PlanAction.Skip)]
        [InlineData(false, PlanAction.Change)]
        public void NoBranchSuffix_PlanActionDependsOnSkipNoBranchSuffix(bool skipNoBranchSuffix, PlanAction expectedAction)
        {
            // Given: a component with no branch suffix and a filter that includes it
            var components = new[]
            {
                new TestComponent { ProjectName = "ArmSharedInfrastructure", Version = "1.2.3456" }
            };

            var filter = new ComponentFilter
            {
                Include = true,
                Expressions = new List<string> { "(?i)^(ArmSharedInfrastructure)$" }
            };

            var planner = DeploymentPlannerTestFactory.GetSutForComponents(components, EnvironmentId);

            // When: generating the deployment plan with the given skipNoBranchSuffix value
            var result = planner.GetBranchDeploymentPlans(EnvironmentId, "1.2.3456", false, skipNoBranchSuffix, filter);
            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            // Then: the plan action matches the expected value
            plans.Should().NotBeEmpty();
            plans.Should().OnlyContain(p => string.IsNullOrWhiteSpace(p.ComponentFrom.Version.SpecialVersion));
            plans.Should().OnlyContain(p => p.Action == expectedAction);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SuffixedBranch_ShouldAlwaysHavePlanActionChange(bool skipNoBranchSuffix)
        {
            // Given: a component with no branch suffix and a filter that includes it
            var components = new[]
            {
                new TestComponent { ProjectName = "ArmSharedInfrastructure", Version = "1.2.34-release-1.6.0" }
            };

            var filter = new ComponentFilter
            {
                Include = true,
                Expressions = new List<string> { "(?i)^(ArmSharedInfrastructure)$" }
            };

            var planner = DeploymentPlannerTestFactory.GetSutForComponents(components, EnvironmentId);

            // When: generating the deployment plan with the given skipNoBranchSuffix
            var result = planner.GetBranchDeploymentPlans(EnvironmentId, "release-1.6.0", false, skipNoBranchSuffix, filter);
            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            // Then: the suffixed branch should always have PlanAction.Change
            var branchPlan = plans.Single();
            branchPlan.Action.Should().Be(PlanAction.Change);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SuffixedAndNonSuffixedBranches_ShouldApplyCorrectPlanActions_BasedOnSkipNoBranchSuffixFlag(bool skipNoBranchSuffix)
        {
            // Given: one suffixed and one non-suffixed component, both matching the filter
            var components = new[]
            {
                new TestComponent { ProjectName = "ArmSharedInfrastructure", Version = "1.2.34-release-1.6.0" },
                new TestComponent { ProjectName = "FileBeat", Version = "1.2.34" } // no suffix
            };

            var filter = new ComponentFilter
            {
                Include = true,
                Expressions = new List<string> { "(?i)^(ArmSharedInfrastructure|FileBeat)$" }
            };

            var planner = DeploymentPlannerTestFactory.GetSutForComponents(components, EnvironmentId);

            // When: generating the deployment plan for branch
            var result = planner.GetBranchDeploymentPlans(EnvironmentId, "release-1.6.0", false, skipNoBranchSuffix, filter);
            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            // Then: the suffixed component should always have its plan action as Change
            var suffixedPlan = plans.Single(p => p.ComponentFrom.Version.ToString() == "1.2.34-release-1.6.0");
            suffixedPlan.Action.Should().Be(PlanAction.Change);

            // And: non-suffixed component action depends on skip flag
            var noSuffixPlan = plans.Single(p => p.ComponentFrom.Version.ToString() == "1.2.34");
            noSuffixPlan.Action.Should().Be(skipNoBranchSuffix ? PlanAction.Skip : PlanAction.Change);
        }

        [Fact]
        public void DeploymentPlan_ShouldOnlyInclude_FilteredComponents()
        {
            // Given: multiple components and a filter that includes only ArmSharedInfrastructure and Filebeat
            var components = new[]
            {
                new TestComponent { ProjectName = "ArmSharedInfrastructure", Version = "1.2.3456" },
                new TestComponent { ProjectName = "Filebeat", Version = "2.3.456" },
                new TestComponent { ProjectName = "TestProjectDummy", Version = "1.0.0" }
            };

            var filter = new ComponentFilter
            {
                Include = true,
                Expressions = new List<string> { "(?i)^(ArmSharedInfrastructure|Filebeat)$" }
            };

            var planner = DeploymentPlannerTestFactory.GetSutForComponents(components, EnvironmentId);

            // When: generating the deployment plan
            var result = planner.GetBranchDeploymentPlans(EnvironmentId, "1.2.3456", false, false, filter);
            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            // Then: only the components matching the filter should appear in the deployment plan 
            plans.Should().NotBeNull();
            plans.Should().HaveCount(2);
            plans.Select(p => p.Name).Should().BeEquivalentTo("ArmSharedInfrastructure", "Filebeat");
            plans.Select(p => p.Name).Should().NotContain("TestProjectDummy");
        }
    }
}
