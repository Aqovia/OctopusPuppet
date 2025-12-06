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

        [Fact]
        public void NoBranchSuffix_PlanActionShouldBeSkipped_WhenSkipNoBranchSuffixIsTrue()
        {
            // Given: a component with no branch suffix 
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

            // When: skipNoBranchSuffix is true
            var result = planner.GetBranchDeploymentPlans(EnvironmentId, "1.2.3456", false, true, filter);
            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            // Then: the deployment plan should skip builds with no branch suffix
            plans.Should().OnlyContain(p => string.IsNullOrWhiteSpace(p.ComponentFrom.Version.SpecialVersion));
            plans.Should().OnlyContain(p => p.Action == PlanAction.Skip);
        }

        [Fact]
        public void NoBranchSuffix_PlanActionShouldBeChange_WhenSkipNoBranchSuffixIsFalse()
        {
            // Given: a component with no branch suffix
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

            // When: skipNoBranchSuffix is false
            var result = planner.GetBranchDeploymentPlans(EnvironmentId, "1.2.3456", false, false, filter);
            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            // Then: the deployment plan should deploy master builds
            plans.Should().NotBeEmpty();
            plans.Should().OnlyContain(p => string.IsNullOrWhiteSpace(p.ComponentFrom.Version.SpecialVersion));
            plans.Should().OnlyContain(p => p.Action == PlanAction.Change);
        }

        [Fact]
        public void BranchWithSuffix_PlanActionShouldAlwaysBeChange_WhenSkipNoBranchSuffixIsTrue()
        {
            // Given: a component with a branch suffix
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

            // When: skipNoBranchSuffix is true
            var result = planner.GetBranchDeploymentPlans(EnvironmentId, "release-1.6.0", false, true, filter);
            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            // Then: the deployment plan should deploy the suffixed branch 
            var branchPlan = plans.SingleOrDefault(p => p.ComponentFrom.Version.ToString() == "1.2.34-release-1.6.0");
            branchPlan.Should().NotBeNull();
            branchPlan.Action.Should().Be(PlanAction.Change);
        }

        [Fact]
        public void BranchWithSuffix_PlanActionShouldAlwaysBeChange_WhenSkipNoBranchSuffixIsFalse()
        {
            // Given: a component with a branch suffix  
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

            // When: skipNoBranchSuffix is false
            var result = planner.GetBranchDeploymentPlans(EnvironmentId, "release-1.6.0", false, false, filter);
            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            // Then: the deployment plan should deploy the suffixed branch 
            var branchPlan = plans.SingleOrDefault(p => p.ComponentFrom.Version.ToString() == "1.2.34-release-1.6.0");
            branchPlan.Should().NotBeNull();
            branchPlan.Action.Should().Be(PlanAction.Change);
        }

        [Fact]
        public void DeploymentPlan_ShouldOnlyInclude_FilteredComponents()
        {
            // given: components with a filter for ArmSharedInfrastructure and Filebeat
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

            // When: retrieving the deployment plan
            var result = planner.GetBranchDeploymentPlans(EnvironmentId, "1.2.3456", false, false, filter);
            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            // Then: only the components matching the filter appear
            plans.Should().NotBeNull();
            plans.Should().HaveCount(2);
            plans.Select(p => p.Name).Should().BeEquivalentTo("ArmSharedInfrastructure", "Filebeat");
            plans.Select(p => p.Name).Should().NotContain("TestProjectDummy");
        }
    }
}
