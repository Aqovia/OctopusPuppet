using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using OctopusPuppet.DeploymentPlanner;
using OctopusPuppet.Scheduler;

namespace OctopusPuppet.Tests
{
    public class DeploymentSchedulerTests
    {
        private EnvironmentDeploymentPlan GetEnvironmentDeploymentPlan()
        {
            var componentDeploymentPlans = new List<ComponentDeploymentPlan>()
            {
                new ComponentDeploymentPlan() {Name = "a", Action = PlanAction.Skip, ComponentFrom = new Component(){Dependancies = {}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,100)}, ComponentTo = null},
                new ComponentDeploymentPlan() {Name = "b", Action = PlanAction.Change, ComponentFrom = new Component(){Dependancies = {"a", "c"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,20)}, ComponentTo = null},
                new ComponentDeploymentPlan() {Name = "c", Action = PlanAction.Skip, ComponentFrom = new Component(){Dependancies = {"b"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,20)}, ComponentTo = null},
                new ComponentDeploymentPlan() {Name = "d", Action = PlanAction.Change, ComponentFrom = new Component(){Dependancies = {"a"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,20)}, ComponentTo = null},
                new ComponentDeploymentPlan() {Name = "e", Action = PlanAction.Skip, ComponentFrom = new Component(){Dependancies = {"d"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,60)}, ComponentTo = null},
                new ComponentDeploymentPlan() {Name = "f", Action = PlanAction.Change, ComponentFrom = new Component(){Dependancies = {"e"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,20)}, ComponentTo = null},
                new ComponentDeploymentPlan() {Name = "g", Action = PlanAction.Remove, ComponentFrom = new Component(){Dependancies = {"d"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,20)}, ComponentTo = null},
                new ComponentDeploymentPlan() {Name = "h", Action = PlanAction.Change, ComponentFrom = new Component(){Dependancies = {"d", "e"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,10)}, ComponentTo = null},

                new ComponentDeploymentPlan() {Name = "x", Action = PlanAction.Skip, ComponentFrom = new Component(){Dependancies = {}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,20)}, ComponentTo = null},
                new ComponentDeploymentPlan() {Name = "y", Action = PlanAction.Change, ComponentFrom = new Component(){Dependancies = {"x"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,40)}, ComponentTo = null},
                new ComponentDeploymentPlan() {Name = "z", Action = PlanAction.Skip, ComponentFrom = new Component(){Dependancies = {"y"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,50)}, ComponentTo = null},
            };

            var environmentDeploymentPlan = new EnvironmentDeploymentPlan(componentDeploymentPlans);
            return environmentDeploymentPlan;
        }

        [Test]
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

            Assert.AreEqual(7, products0.DeploymentSteps.Count());
            Assert.AreEqual(3, products1.DeploymentSteps.Count());
        }

        [Test]
        public void GetDeploymentPlanForComponentDeploymentList()
        {
            var componentDependancies = GetEnvironmentDeploymentPlan();

            var deploymentScheduler = new DeploymentScheduler();
            var componentGraph = deploymentScheduler.GetComponentDeploymentGraph(componentDependancies);
            var environmentDeployment = deploymentScheduler.GetEnvironmentDeployment(componentGraph);

            var products0 = environmentDeployment.ProductDeployments[0];
            var products1 = environmentDeployment.ProductDeployments[1];

            Assert.AreEqual(7, products0.DeploymentSteps.Count());
            Assert.AreEqual(3, products1.DeploymentSteps.Count());
        }

        [Test]
        public void GetDeploymentPlanForComponentAdjacencyGraph()
        {
            var componentDeploymentGraph = new ComponentDeploymentGraph();

            //Add vertices

            var a = new ComponentDeploymentVertex("a", "1.0.0", PlanAction.Skip, null);
            var b = new ComponentDeploymentVertex("b", "1.0.0", PlanAction.Change, null);
            var c = new ComponentDeploymentVertex("c", "1.0.0", PlanAction.Skip, null);
            var d = new ComponentDeploymentVertex("d", "1.0.0", PlanAction.Change, null);
            var e = new ComponentDeploymentVertex("e", "1.0.0", PlanAction.Skip, null);
            var f = new ComponentDeploymentVertex("f", "1.0.0", PlanAction.Change, null);
            var g = new ComponentDeploymentVertex("g", "1.0.0", PlanAction.Remove, null);
            var h = new ComponentDeploymentVertex("h", "1.0.0", PlanAction.Change, null);

            var x = new ComponentDeploymentVertex("x", "1.0.0", PlanAction.Skip, null);
            var y = new ComponentDeploymentVertex("y", "1.0.0", PlanAction.Change, null);
            var z = new ComponentDeploymentVertex("z", "1.0.0", PlanAction.Skip, null);

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

            Assert.AreEqual(7, products0.DeploymentSteps.Count());
            Assert.AreEqual(3, products1.DeploymentSteps.Count());
        }   
    }
}
