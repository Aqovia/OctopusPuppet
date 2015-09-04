using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using OctopusPuppet.DeploymentPlanner;
using OctopusPuppet.Scheduler;
using QuickGraph;

namespace OctopusPuppet.Tests
{
    public class DeploymentSchedulerTests
    {
        private List<DeploymentPlan> GetDeploymentPlans()
        {
            var componentDependanciesToSerialize = new List<DeploymentPlan>()
            {
                new DeploymentPlan() {Name = "a", Action = PlanAction.Skip, ComponentFrom = new Component(){Dependancies = {}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,100)}, ComponentTo = null},
                new DeploymentPlan() {Name = "b", Action = PlanAction.Change, ComponentFrom = new Component(){Dependancies = {"a", "c"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,20)}, ComponentTo = null},
                new DeploymentPlan() {Name = "c", Action = PlanAction.Skip, ComponentFrom = new Component(){Dependancies = {"b"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,20)}, ComponentTo = null},
                new DeploymentPlan() {Name = "d", Action = PlanAction.Change, ComponentFrom = new Component(){Dependancies = {"a"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,20)}, ComponentTo = null},
                new DeploymentPlan() {Name = "e", Action = PlanAction.Skip, ComponentFrom = new Component(){Dependancies = {"d"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,60)}, ComponentTo = null},
                new DeploymentPlan() {Name = "f", Action = PlanAction.Change, ComponentFrom = new Component(){Dependancies = {"e"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,20)}, ComponentTo = null},
                new DeploymentPlan() {Name = "g", Action = PlanAction.Remove, ComponentFrom = new Component(){Dependancies = {"d"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,20)}, ComponentTo = null},
                new DeploymentPlan() {Name = "h", Action = PlanAction.Change, ComponentFrom = new Component(){Dependancies = {"d", "e"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,10)}, ComponentTo = null},

                new DeploymentPlan() {Name = "x", Action = PlanAction.Skip, ComponentFrom = new Component(){Dependancies = {}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,20)}, ComponentTo = null},
                new DeploymentPlan() {Name = "y", Action = PlanAction.Change, ComponentFrom = new Component(){Dependancies = {"x"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,40)}, ComponentTo = null},
                new DeploymentPlan() {Name = "z", Action = PlanAction.Skip, ComponentFrom = new Component(){Dependancies = {"y"}, Version = new SemVer("1.0.0.0"), DeploymentDuration = new TimeSpan(0,0,50)}, ComponentTo = null},
            };

            return componentDependanciesToSerialize;
        }

        [Test]
        public void GetDeploymentPlanForComponentJson()
        {
            var componentDependanciesToSerialize = GetDeploymentPlans();

            var componentDependanciesJson = JsonConvert.SerializeObject(componentDependanciesToSerialize);
            var componentDependancies = JsonConvert.DeserializeObject<List<DeploymentPlan>>(componentDependanciesJson);

            var deploymentPlanner = new DeploymentScheduler();
            var products = deploymentPlanner.GetDeploymentSchedule(componentDependancies);

            var products0 = products[0];
            var products1 = products[1];

            Assert.AreEqual(7, products0.Count());
            Assert.AreEqual(3, products1.Count());
        }

        [Test]
        public void GetDeploymentPlanForComponentDeploymentList()
        {
            var componentDependancies = GetDeploymentPlans();

            var deploymentPlanner = new DeploymentScheduler();
            var products = deploymentPlanner.GetDeploymentSchedule(componentDependancies);

            var products0 = products[0];
            var products1 = products[1];

            Assert.AreEqual(7, products0.Count());
            Assert.AreEqual(3, products1.Count());
        }

        [Test]
        public void GetDeploymentPlanForComponentAdjacencyGraph()
        {
            var componentDependancies = new AdjacencyGraph<ComponentVertex, ComponentEdge>(true);

            //Add vertices

            var a = new ComponentVertex("a", "1.0.0", PlanAction.Skip, null);
            var b = new ComponentVertex("b", "1.0.0", PlanAction.Change, null);
            var c = new ComponentVertex("c", "1.0.0", PlanAction.Skip, null);
            var d = new ComponentVertex("d", "1.0.0", PlanAction.Change, null);
            var e = new ComponentVertex("e", "1.0.0", PlanAction.Skip, null);
            var f = new ComponentVertex("f", "1.0.0", PlanAction.Change, null);
            var g = new ComponentVertex("g", "1.0.0", PlanAction.Remove, null);
            var h = new ComponentVertex("h", "1.0.0", PlanAction.Change, null);

            var x = new ComponentVertex("x", "1.0.0", PlanAction.Skip, null);
            var y = new ComponentVertex("y", "1.0.0", PlanAction.Change, null);
            var z = new ComponentVertex("z", "1.0.0", PlanAction.Skip, null);

            componentDependancies.AddVertexRange(new ComponentVertex[]
            {
                a, b, c, d, e, f, g, h, x, y, z
            });

            //Create edges

            var b_a = new ComponentEdge(b, a);
            var b_c = new ComponentEdge(b, c);
            var c_b = new ComponentEdge(c, b);
            var d_a = new ComponentEdge(d, a);
            var e_d = new ComponentEdge(e, d);
            var f_e = new ComponentEdge(f, e);
            var g_d = new ComponentEdge(g, d);
            var h_e = new ComponentEdge(h, e);
            var h_d = new ComponentEdge(h, d);
            
            var y_x = new ComponentEdge(y, x);
            var z_y = new ComponentEdge(z, y);

            componentDependancies.AddEdgeRange(new ComponentEdge[]
            {
                b_a, b_c, c_b, d_a, e_d, f_e, g_d, h_e, h_d, y_x, z_y
            });

            var deploymentPlanner = new DeploymentScheduler();
            var products = deploymentPlanner.GetDeploymentSchedule(componentDependancies);

            var products0 = products[0];
            var products1 = products[1];

            Assert.AreEqual(7, products0.Count());
            Assert.AreEqual(3, products1.Count());
        }   
    }
}
