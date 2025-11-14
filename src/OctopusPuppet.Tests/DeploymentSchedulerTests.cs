using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using OctopusPuppet.DeploymentPlanner;
using OctopusPuppet.Scheduler;
using Xunit;

namespace OctopusPuppet.Tests
{
    public class DeploymentSchedulerTests
    {
        private const int ExpectedNumberOfDeploymentStepsForProductDeployment0 = 4;
        private const int ExpectedNumberOfComponentDeploymentsForProductDeployment0 = 8;

        private const int ExpectedNumberOfDeploymentStepsForProductDeployment1 = 3;
        private const int ExpectedNumberOfComponentDeploymentsForProductDeployment1 = 3;

        private EnvironmentDeploymentPlan GetEnvironmentDeploymentPlan()
        {
            var componentDeploymentPlans = new List<ComponentDeploymentPlan>()
            {
                new ComponentDeploymentPlan() {Id = "a", Name = "a", Action = PlanAction.Skip, ComponentFrom = new Component(){Dependancies = {}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,100)}, ComponentTo = null},
                new ComponentDeploymentPlan() {Id = "b", Name = "b", Action = PlanAction.Change, ComponentFrom = new Component(){Dependancies = {"a", "c"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,20)}, ComponentTo = null},
                new ComponentDeploymentPlan() {Id = "c", Name = "c", Action = PlanAction.Skip, ComponentFrom = new Component(){Dependancies = {"b"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,20)}, ComponentTo = null},
                new ComponentDeploymentPlan() {Id = "d", Name = "d", Action = PlanAction.Change, ComponentFrom = new Component(){Dependancies = {"a"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,20)}, ComponentTo = null},
                new ComponentDeploymentPlan() {Id = "e", Name = "e", Action = PlanAction.Skip, ComponentFrom = new Component(){Dependancies = {"d"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,60)}, ComponentTo = null},
                new ComponentDeploymentPlan() {Id = "f", Name = "f", Action = PlanAction.Change, ComponentFrom = new Component(){Dependancies = {"e"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,20)}, ComponentTo = null},
                new ComponentDeploymentPlan() {Id = "g", Name = "g", Action = PlanAction.Remove, ComponentFrom = new Component(){Dependancies = {"d"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,20)}, ComponentTo = null},
                new ComponentDeploymentPlan() {Id = "h", Name = "h", Action = PlanAction.Change, ComponentFrom = new Component(){Dependancies = {"d", "e"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,10)}, ComponentTo = null},

                new ComponentDeploymentPlan() {Id = "x", Name = "x", Action = PlanAction.Skip, ComponentFrom = new Component(){Dependancies = {}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,20)}, ComponentTo = null},
                new ComponentDeploymentPlan() {Id = "y", Name = "y", Action = PlanAction.Change, ComponentFrom = new Component(){Dependancies = {"x"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,40)}, ComponentTo = null},
                new ComponentDeploymentPlan() {Id = "z", Name = "z", Action = PlanAction.Skip, ComponentFrom = new Component(){Dependancies = {"y"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,50)}, ComponentTo = null},
            };

            var environmentDeploymentPlan = new EnvironmentDeploymentPlan(componentDeploymentPlans);
            return environmentDeploymentPlan;
        }



        private EnvironmentDeploymentPlan GetEnvironmentDeploymentPlanWithReleaseBranch()
        {
            var componentDeploymentPlans = new List<ComponentDeploymentPlan>()
            {
                new ComponentDeploymentPlan() {Id ="release-1.5.1", Name = "release-1.5.1", Action = PlanAction.Change, ComponentFrom = new Component(){Dependancies = {}, Version = new SemVer("1.2.345-release-1.5.1"), DeploymentDuration = new TimeSpan(0,0,100)}, ComponentTo = null},
            };

            return new EnvironmentDeploymentPlan(componentDeploymentPlans);
        }

        [Fact]
        public void GetDeploymentPlan_Should_Use_InputBranch_As_DeploymentBranch()
        {
            var environmentDeploymentPlan = GetEnvironmentDeploymentPlanWithReleaseBranch();
            var deploymentScheduler = new DeploymentScheduler();
            var componentGraph = deploymentScheduler.GetComponentDeploymentGraph(environmentDeploymentPlan);
            var environmentDeployment = deploymentScheduler.GetEnvironmentDeployment(componentGraph);

            var product = environmentDeployment.ProductDeployments.Single();
            var deployedComponent = product.DeploymentSteps.SelectMany(x => x.ComponentDeployments).Single();

            deployedComponent.Vertex.Id.Should().Be("release-1.5.1");
            deployedComponent.Vertex.Name.Should().Be("release-1.5.1");
            deployedComponent.Vertex.Version.SpecialVersion.Should().Be("release-1.5.1");
        }


        [Fact]
        public void GetDeploymentPlanForComponentJson()
        {
            var environmentDeploymentPlan = GetEnvironmentDeploymentPlan();

            var componentDependanciesJson = JsonConvert.SerializeObject(environmentDeploymentPlan);
            var componentDependancies = JsonConvert.DeserializeObject<EnvironmentDeploymentPlan>(componentDependanciesJson);

            var deploymentScheduler = new DeploymentScheduler();
            var componentGraph = deploymentScheduler.GetComponentDeploymentGraph(componentDependancies);
            var environmentDeployment = deploymentScheduler.GetEnvironmentDeployment(componentGraph);

            var products0 = environmentDeployment.ProductDeployments[0];
            var products1 = environmentDeployment.ProductDeployments[1];

            products0.DeploymentSteps.Count().Should().Be(ExpectedNumberOfDeploymentStepsForProductDeployment0);
            products0.DeploymentSteps.SelectMany(productDeploymentStep => productDeploymentStep.ComponentDeployments).Count().Should().Be(ExpectedNumberOfComponentDeploymentsForProductDeployment0);

            products1.DeploymentSteps.Count().Should().Be(ExpectedNumberOfDeploymentStepsForProductDeployment1);
            products1.DeploymentSteps.SelectMany(productDeploymentStep => productDeploymentStep.ComponentDeployments).Count().Should().Be(ExpectedNumberOfComponentDeploymentsForProductDeployment1);
        } 

        [Fact]
        public void GetDeploymentPlanForComponentDeploymentList()
        {
            var componentDependancies = GetEnvironmentDeploymentPlan();

            var deploymentScheduler = new DeploymentScheduler();
            var componentGraph = deploymentScheduler.GetComponentDeploymentGraph(componentDependancies);
            var environmentDeployment = deploymentScheduler.GetEnvironmentDeployment(componentGraph);

            var products0 = environmentDeployment.ProductDeployments[0];
            var products1 = environmentDeployment.ProductDeployments[1];

            products0.DeploymentSteps.Count().Should().Be(ExpectedNumberOfDeploymentStepsForProductDeployment0);
            products0.DeploymentSteps.SelectMany(productDeploymentStep => productDeploymentStep.ComponentDeployments).Count().Should().Be(ExpectedNumberOfComponentDeploymentsForProductDeployment0);

            products1.DeploymentSteps.Count().Should().Be(ExpectedNumberOfDeploymentStepsForProductDeployment1);
            products1.DeploymentSteps.SelectMany(productDeploymentStep => productDeploymentStep.ComponentDeployments).Count().Should().Be(ExpectedNumberOfComponentDeploymentsForProductDeployment1);
        }

        [Fact]
        public void GetDeploymentPlanForComponentAdjacencyGraph()
        {
            var componentDeploymentGraph = new ComponentDeploymentGraph();

            //Add vertices

            var a = new ComponentDeploymentVertex("a", "a", new SemVer("1.0.0"), PlanAction.Skip, null);
            var b = new ComponentDeploymentVertex("b", "b", new SemVer("1.0.0"), PlanAction.Change, null);
            var c = new ComponentDeploymentVertex("c", "c", new SemVer("1.0.0"), PlanAction.Skip, null);
            var d = new ComponentDeploymentVertex("d", "d", new SemVer("1.0.0"), PlanAction.Change, null);
            var e = new ComponentDeploymentVertex("e", "e", new SemVer("1.0.0"), PlanAction.Skip, null);
            var f = new ComponentDeploymentVertex("f", "f", new SemVer("1.0.0"), PlanAction.Change, null);
            var g = new ComponentDeploymentVertex("g", "g", new SemVer("1.0.0"), PlanAction.Remove, null);
            var h = new ComponentDeploymentVertex("h", "h", new SemVer("1.0.0"), PlanAction.Change, null);

            var x = new ComponentDeploymentVertex("x", "x", new SemVer("1.0.0"), PlanAction.Skip, null);
            var y = new ComponentDeploymentVertex("y", "y", new SemVer("1.0.0"), PlanAction.Change, null);
            var z = new ComponentDeploymentVertex("z", "z", new SemVer("1.0.0"), PlanAction.Skip, null);

            componentDeploymentGraph.AddVertexRange(new ComponentDeploymentVertex[]
            {
                a, b, c, d, e, f, g, h, x, y, z
            });

            //Create edges

            var b_a = new ComponentDeploymentEdge(b, a);
            var b_c = new ComponentDeploymentEdge(b, c);
            var c_b = new ComponentDeploymentEdge(c, b);
            var d_a = new ComponentDeploymentEdge(d, a);
            var e_d = new ComponentDeploymentEdge(e, d);
            var f_e = new ComponentDeploymentEdge(f, e);
            var g_d = new ComponentDeploymentEdge(g, d);
            var h_e = new ComponentDeploymentEdge(h, e);
            var h_d = new ComponentDeploymentEdge(h, d);

            var y_x = new ComponentDeploymentEdge(y, x);
            var z_y = new ComponentDeploymentEdge(z, y);

            componentDeploymentGraph.AddEdgeRange(new ComponentDeploymentEdge[]
            {
                b_a, b_c, c_b, d_a, e_d, f_e, g_d, h_e, h_d, y_x, z_y
            });

            var deploymentScheduler = new DeploymentScheduler();
            var environmentDeployment = deploymentScheduler.GetEnvironmentDeployment(componentDeploymentGraph);

            var products0 = environmentDeployment.ProductDeployments[0];
            var products1 = environmentDeployment.ProductDeployments[1];

            products0.DeploymentSteps.Count().Should().Be(ExpectedNumberOfDeploymentStepsForProductDeployment0);
            products0.DeploymentSteps.SelectMany(productDeploymentStep => productDeploymentStep.ComponentDeployments).Count().Should().Be(ExpectedNumberOfComponentDeploymentsForProductDeployment0);

            products1.DeploymentSteps.Count().Should().Be(ExpectedNumberOfDeploymentStepsForProductDeployment1);
            products1.DeploymentSteps.SelectMany(productDeploymentStep => productDeploymentStep.ComponentDeployments).Count().Should().Be(ExpectedNumberOfComponentDeploymentsForProductDeployment1);
        }
    }
}
