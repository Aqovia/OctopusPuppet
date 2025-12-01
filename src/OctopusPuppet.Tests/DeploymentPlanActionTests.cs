using System.Collections.Generic;
using FluentAssertions;
using OctopusPuppet.DeploymentPlanner;
using OctopusPuppet.OctopusProvider;
using OctopusPuppet.Tests.TestHelpers;
using Xunit;

namespace OctopusPuppet.Tests
{
    public class DeploymentPlanActionTests
    {
        private readonly OctopusDeploymentPlanner _planner;
        private readonly string _version = "1.2.3456";
        private readonly string _environment = "D1";
        private readonly ComponentFilter _filter;

        public DeploymentPlanActionTests()
        {
            // Arrange: initialise planner and component filter for all tests
            _planner = DeploymentPlannerTestFactory.GetSutForVersion(_version, _environment);
            _filter = DeploymentPlannerTestFactory.ComponentFilterForTarget();
        }

        [Fact]
        public void DeploymentPlanner_ShouldSkipNoBranchSuffix_WhenFlagIsTrue()
        {
            // Given: a planner with a component filter and skipNoBranchSuffix = true
            var result = _planner.GetBranchDeploymentPlans(_environment, _version, false, true, _filter);

            // When: retrieving the deployment plans
            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            // Then: all relevant components are returned as master builds and skipped
            plans.Should().NotBeEmpty();
            plans.Should().OnlyContain(p => p.Name == "ArmSharedInfrastructure" || p.Name == "Filebeat");
            plans.Should().OnlyContain(p => string.IsNullOrWhiteSpace(p.ComponentFrom.Version.SpecialVersion),
                "versions with no special suffix are master builds");
            plans.Should().OnlyContain(p => p.Action == PlanAction.Skip,
                "master builds should be skipped when skipNoBranchSuffix=true");
        }

        [Fact]
        public void DeploymentPlanner_ShouldDeployNoBranchSuffix_WhenFlagIsFalse()
        {
            // Given: a planner with a component filter and skipNoBranchSuffix = false
            var result = _planner.GetBranchDeploymentPlans(_environment, _version, false, false, _filter);

            // When: retrieving the deployment plans
            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            // Then: all relevant components are returned as master builds and deployed
            plans.Should().NotBeEmpty();
            plans.Should().OnlyContain(p => p.Name == "ArmSharedInfrastructure" || p.Name == "Filebeat");
            plans.Should().OnlyContain(p => string.IsNullOrWhiteSpace(p.ComponentFrom.Version.SpecialVersion),
                "versions with no special suffix are master builds");
            plans.Should().OnlyContain(p => p.Action == PlanAction.Change,
                "master builds should be deployed when skipNoBranchSuffix=false");
        }

        [Theory]
        [InlineData("ArmSharedInfrastructure", true)]
        [InlineData("Filebeat", true)]
        [InlineData("NonRelevantProject", false)]
        public void DeploymentPlanner_ComponentFilter_ShouldOnlyIncludeMatchingComponents(string projectName, bool shouldBeIncluded)
        {
            // Given: a planner with a component filter
            var result = _planner.GetBranchDeploymentPlans(_environment, _version, false, false, _filter);

            // When: retrieving the deployment plans
            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            // Then: only projects matching the filter are included
            if (shouldBeIncluded)
                plans.Should().ContainSingle(p => p.Name == projectName);
            else
                plans.Should().NotContain(p => p.Name == projectName);
        }
    }
}