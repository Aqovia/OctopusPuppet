
using System.Linq;
using FluentAssertions;
using OctopusPuppet.OctopusProvider;
using OctopusPuppet.Tests.TestHelpers;
using Xunit;

namespace OctopusPuppet.Tests
{
    [Collection("NonParallelCollection")] 
    public class OctopusDeploymentPlannerTests
    {
        private readonly TestComponent[] _components = new[]
        {
            new TestComponent { ProjectName = "ArmSharedInfrastructure", Version = "1.2.3456" },                  // master
            new TestComponent { ProjectName = "ArmSharedInfrastructure", Version = "1.2.3456-a-branch" },         // normal branch
            new TestComponent { ProjectName = "ArmSharedInfrastructure", Version = "1.2.3456-release-1.5.1" },   // release branch
        };

        [Theory]
        [InlineData("1.2.3456", true)]                  // master
        [InlineData("1.2.3456-a-branch", true)]        // normal branch
        [InlineData("1.2.3456-release-1.5.1", true)]   // release branch
        [InlineData("an-invalid-version-number", false)] // invalid
        [InlineData("1234", false)]                     // invalid
        public void GetBranches_ShouldExistBasedOnVersion(string version, bool shouldExist)
        {

            // given: a planner with components that only include valid versions
             
            var components = _components
               .Where(c => c.Version == version && shouldExist)
               .ToArray();

            var planner = DeploymentPlannerTestFactory.GetSutForComponents(components, "D1");

            // when: retrieving branches for the given version
            var branches = planner.GetBranches();

            // then: branches exist or not as expected
            if (shouldExist)
            {
                branches.Should().NotBeEmpty($"the version '{version}' should exist");
            }
            else
            {
                branches.Should().BeEmpty($"the version '{version}' is invalid and should not exist");
            }
        }
    }
}