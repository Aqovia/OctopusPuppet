 
using System.Linq;
using FluentAssertions; 
using OctopusPuppet.OctopusProvider;
using OctopusPuppet.Tests.TestHelpers;
using Xunit; 

namespace OctopusPuppet.Tests
{
    public class OctopusDeploymentPlannerTests
    {
        // Provides a mocked OctopusDeploymentPlanner for unit tests.
        private OctopusDeploymentPlanner GetPlanner(string version) => 
            DeploymentPlannerTestFactory.GetSutForVersion(version);

        [Fact]
        public void GetBranches_should_include_branches_with_valid_version_numbers_containing_special_names()
        {
            GetPlanner("1.2.3456-a-branch").GetBranches().Select(x => new { x.Id, x.Name })
                .Should().Equal(new { Id = "a-branch", Name = "a-branch" });
        }

        [Fact]
        public void GetBranches_should_include_branches_in_versions_without_special_names()
        {
            GetPlanner("1.2.3456").GetBranches().Select(x => new { x.Id, x.Name })
                .Should().Equal(new { Id = "", Name = "" });
        }

        [Fact]
        public void GetBranches_should_not_include_branches_with_invalid_version_numbers()
        {
            GetPlanner("an-invalid-version-number").GetBranches().Should().BeEmpty();
        }

        [Fact]
        public void GetBranches_should_include_branches_with_release_prefix()
        {
            DeploymentPlannerTestFactory.GetSutForVersion("1.2.3456-release-1.5.1").GetBranches().Select(x => new { x.Id, x.Name })
                .Should().Equal(new { Id = "release-1.5.1", Name = "release-1.5.1" });
        }

        [Theory]
        [InlineData("1.2.3456-release-1.5.1", true)]
        [InlineData("release1.5.1", false)]
        [InlineData("release.1", false)]
        [InlineData("1.2.3456-a-branch", true)]
        [InlineData("1.2.3456-DO-2059-disable-waf-rule", true)]
        [InlineData("1234", false)]
        public void GetBranches_ShouldValidateReleaseFormat(string version, bool shouldExist)
        {
            var branches = GetPlanner(version).GetBranches();

            if (shouldExist)
            {
                branches.Should().NotBeEmpty();
            }
            else
            {
                branches.Should().BeEmpty();
            }
        }
    }

}