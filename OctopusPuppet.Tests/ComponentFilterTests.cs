using System.Collections.Generic;
using NUnit.Framework;
using OctopusPuppet.DeploymentPlanner;

namespace OctopusPuppet.Tests
{
    public class ComponentFilterTests
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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
