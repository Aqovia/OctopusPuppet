using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using Octopus.Client;
using Octopus.Client.Model;
using OctopusPuppet.DeploymentPlanner;
using OctopusPuppet.OctopusProvider;
using Xunit;
using static NSubstitute.Arg;

namespace OctopusPuppet.Tests
{
    public class OctopusDeploymentPlannerTests
    {
        private static OctopusDeploymentPlanner GetSutForVersion(string versionNumber)
        {
            var repo = Substitute.For<IOctopusRepository>();

            var project2 = new ProjectResource("124", "", "Projects-124");

            repo.Projects.GetAll().Returns(new List<ReferenceDataItem> { new ReferenceDataItem("123", "") });
            repo.Projects.FindAll().Returns(new List<ProjectResource> { project2 });

            var project1 = new ProjectResource() { Id = "123" };
            repo.Projects.Get("123").Returns(project1);
            repo.Projects.GetReleases(project1)
                .Returns(new ResourceCollection<ReleaseResource>(new[] { new ReleaseResource(versionNumber, "123", "") },
                    new LinkCollection()));

            repo.Projects.Get("124").Returns(project2);
            repo.Projects.GetReleases(project2)
                .Returns(new ResourceCollection<ReleaseResource>(new[] { new ReleaseResource(versionNumber, "124", "") },
                    new LinkCollection()));

            var sut = new OctopusDeploymentPlanner(repo);
            return sut;
        }

        [Fact]
        public void GetBranches_should_include_branches_with_valid_version_numbers_containing_special_names()
        {
            GetSutForVersion("1.2.3456-a-branch").GetBranches().Select(x => new { x.Id, x.Name })
                .Should().Equal(new { Id = "a-branch", Name = "a-branch" });
        }

        [Fact]
        public void GetBranches_should_include_branches_in_versions_without_special_names()
        {
            GetSutForVersion("1.2.3456").GetBranches().Select(x => new { x.Id, x.Name })
                .Should().Equal(new { Id = "", Name = "" });
        }

        [Fact]
        public void GetBranches_should_not_include_branches_with_invalid_version_numbers()
        {
            GetSutForVersion("an-invalid-version-number").GetBranches().Should().BeEmpty();
        }
    }
}