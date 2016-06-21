using System.Collections.Generic;
using OctopusPuppet.DeploymentPlanner;
using Xunit;

namespace OctopusPuppet.Tests
{
    public class ComponentFilterTests
    {
        [Fact]
        public void IncludeMatch()
        {
            var componentFilter = new ComponentFilter()
            {
                Include = true,
                Expressions = new List<string>() { "^project.*$"}
            };

            var match = componentFilter.Match("project name");

            Assert.True(match);
        }

        [Fact]
        public void IncludeDoesNotMatch()
        {
            var componentFilter = new ComponentFilter()
            {
                Include = true,
                Expressions = new List<string>() { "^project.*$" }
            };

            var match = componentFilter.Match("not matched project name");

            Assert.False(match);
        }

        [Fact]
        public void IncludeDoesNotMatchWhenEmpty()
        {
            var componentFilter = new ComponentFilter()
            {
                Include = true,
                Expressions = new List<string>()
            };

            var match = componentFilter.Match("not matched project name");

            Assert.False(match);
        }

        [Fact]
        public void ExcludeMatch()
        {
            var componentFilter = new ComponentFilter()
            {
                Include = false,
                Expressions = new List<string>() { "^project.*$" }
            };

            var match = componentFilter.Match("project name");

            Assert.False(match);
        }

        [Fact]
        public void ExcludeDoesNotMatch()
        {
            var componentFilter = new ComponentFilter()
            {
                Include = false,
                Expressions = new List<string>() { "^project.*$" }
            };

            var match = componentFilter.Match("not matched project name");

            Assert.True(match);
        }

        [Fact]
        public void ExcludeDoesNotMatchhWhenEmpty()
        {
            var componentFilter = new ComponentFilter()
            {
                Include = false,
                Expressions = new List<string>()
            };

            var match = componentFilter.Match("not matched project name");

            Assert.True(match);
        }
    }
}
