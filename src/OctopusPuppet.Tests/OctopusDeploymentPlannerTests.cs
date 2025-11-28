
using System.Linq;
using FluentAssertions;
using OctopusPuppet.OctopusProvider;
using OctopusPuppet.Tests.TestHelpers;
using Xunit;

namespace OctopusPuppet.Tests
{
    public class OctopusDeploymentPlannerTests
    {

        [Theory]
        [InlineData("1.2.3456", "", true)] // master branch
        [InlineData("1.2.3456-a-branch", "a-branch", true)] // normal branch
        [InlineData("1.2.3456-release-1.5.1", "release-1.5.1", true)] // release branch
        [InlineData("an-invalid-version-number", "", false)] // invalid version
        [InlineData("1234", "", false)] // invalid version
        public void GetBranches_ShouldReturnCorrectBranchIdAndName(string version, string expectedBranch, bool shouldExist)
        {
            // Given: a planner for a specific version
            var planner = DeploymentPlannerTestFactory.GetSutForVersion(version);

            // When: retrieving branches
            var branches = planner.GetBranches();

            // Then: branches exist or not as expected
            if (shouldExist)
            {
                branches.Should().ContainSingle() 
                    .Which.ShouldBeEquivalentTo(
                        new { Id = expectedBranch, Name = expectedBranch },
                        options => options.ExcludingMissingMembers()
                    );
            }
            else
            {
                branches.Should().BeEmpty();
            }
        }
    }
}