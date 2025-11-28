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

        // Provides a mocked OctopusDeploymentPlanner for unit tests.
        private OctopusDeploymentPlanner GetPlanner(string version) =>
            DeploymentPlannerTestFactory.GetSutForVersion(version, "D1");


        private ComponentFilter GetComponentFilter() =>
            DeploymentPlannerTestFactory.ComponentFilterForTarget();
     

        [Fact]
        public void DeploymentPlanner_ShouldSkipNoBranchSuffix_WhenFlagIsTrue()
        {
            var planner = GetPlanner("1.2.3456");
            var result = planner.GetBranchDeploymentPlans("D1", "1.2.3456", false, true, GetComponentFilter());

            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            plans.Should().NotBeEmpty();
            plans.Should().OnlyContain(p => p.Name == "ArmSharedInfrastructure" || p.Name == "Filebeat");
            plans.Should().OnlyContain(p => string.IsNullOrWhiteSpace(p.ComponentFrom.Version.SpecialVersion));
            plans.Should().OnlyContain(p => p.Action == PlanAction.Skip);
        }

        [Fact]
        public void DeploymentPlanner_ShouldDeployNoBranchSuffix_WhenFlagIsFalse()
        {
            var planner = GetPlanner("1.2.3456");
            var result = planner.GetBranchDeploymentPlans("D1", "1.2.3456", false, false, GetComponentFilter());

            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            plans.Should().NotBeEmpty();
            plans.Should().OnlyContain(p => p.Name == "ArmSharedInfrastructure" || p.Name == "Filebeat");
            plans.Should().OnlyContain(p => string.IsNullOrWhiteSpace(p.ComponentFrom.Version.SpecialVersion));
            plans.Should().OnlyContain(p => p.Action == PlanAction.Change);
        }

        [Theory]
        [InlineData("ArmSharedInfrastructure", true)]
        [InlineData("Filebeat", true)]
        [InlineData("NonRelevantProject", false)]
        public void DeploymentPlanner_ComponentFilter_ShouldOnlyIncludeMatchingComponents(string projectName, bool shouldBeIncluded)
        {
            var planner = GetPlanner("1.2.3456");
            var result = planner.GetBranchDeploymentPlans("D1", "1.2.3456", false, false, GetComponentFilter());
            var plans = result.EnvironmentDeploymentPlan.DeploymentPlans;

            if (shouldBeIncluded)
                plans.Should().ContainSingle(p => p.Name == projectName);
            else
                plans.Should().NotContain(p => p.Name == projectName);
        }
    }
}
