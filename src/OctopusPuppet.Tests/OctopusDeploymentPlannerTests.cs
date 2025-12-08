
using System.Linq;
using FluentAssertions;
using OctopusPuppet.OctopusProvider;
using OctopusPuppet.Tests.TestHelpers;
using Xunit;

namespace OctopusPuppet.Tests
{
     
    public class OctopusDeploymentPlannerTests
    {
        private readonly TestComponent[] _components;
        private readonly OctopusDeploymentPlanner _planner;

        public OctopusDeploymentPlannerTests()
        { 
            // Given: several components with master, normal-branch, and release-branch versions
            _components = new[]
            {
                new TestComponent { ProjectName = "ArmSharedInfrastructure", Version = "1.2.3456" },                 
                new TestComponent { ProjectName = "FileBeat", Version = "1.2.3456-a-branch" },       
                new TestComponent { ProjectName = "TestProjectDummy", Version = "1.2.3456-release-1.5.1" },  
            };

            _planner = DeploymentPlannerTestFactory.GetSutForComponents(_components, "D1");
        }


        [Theory]
        [InlineData("1.2.3456" ,"", true)]                  // master
        [InlineData("1.2.3456-a-branch", "a-branch", true)]        // normal branch
        [InlineData("1.2.3456-release-1.5.1", "release-1.5.1", true)]   // release branch
        [InlineData("an-invalid-version-number", "", false)] // invalid
        [InlineData("1234", "", false)]                     // invalid
        public void GetBranches_ShouldExistBasedOnVersion(string version,string expectedVersion, bool shouldExist)
        {


            // When: retrieving branch list from the planner
            var branches = _planner.GetBranches();

            // Then: the expected branch should exist or not, based on validity of the version
            if (shouldExist)
            {
                branches.Should()
                        .Contain(b => b.Name == expectedVersion,
                            $"the version '{version}' should exist");
            }
            else
            {
                branches.Should()
                        .NotContain(version,
                            $"the version '{version}' is invalid and should not exist");
            }
        }
    }
}