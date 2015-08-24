using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using QuickGraph;

namespace OctopusPuppet.Tests
{
    public class ComponentDependancyTests
    {
        [Test]
        public void GetDeploymentPlanForComponentJson()
        {
            var componentDependanciesToSerialize = new List<ComponentDeployment>()
            {
                new ComponentDeployment() {Name = "a", Action = ComponentAction.Skip, Dependancies = {}},
                new ComponentDeployment() {Name = "b", Action = ComponentAction.Change, Dependancies = {"a", "c"}},
                new ComponentDeployment() {Name = "c", Action = ComponentAction.Skip, Dependancies = {"b"}},
                new ComponentDeployment() {Name = "d", Action = ComponentAction.Change, Dependancies = {"a"}},
                new ComponentDeployment() {Name = "e", Action = ComponentAction.Skip, Dependancies = {"d"}},
                new ComponentDeployment() {Name = "f", Action = ComponentAction.Change, Dependancies = {"e"}},
                new ComponentDeployment() {Name = "g", Action = ComponentAction.Remove, Dependancies = {"d"}},
                new ComponentDeployment() {Name = "h", Action = ComponentAction.Change, Dependancies = {"d", "e"}},

                new ComponentDeployment() {Name = "x", Action = ComponentAction.Skip, Dependancies = {}},
                new ComponentDeployment() {Name = "y", Action = ComponentAction.Change, Dependancies = {"x"}},
                new ComponentDeployment() {Name = "z", Action = ComponentAction.Skip, Dependancies = {"y"}}
            };

            var componentDependanciesJson = JsonConvert.SerializeObject(componentDependanciesToSerialize);
            var componentDependancies = JsonConvert.DeserializeObject<List<ComponentDeployment>>(componentDependanciesJson);

            var deploymentPlanner = new DeploymentPlanner();
            var products = deploymentPlanner.GetDeploymentPlan(componentDependancies);

            var productsJson0 = JsonConvert.SerializeObject(products[0]);
            var productsJson1 = JsonConvert.SerializeObject(products[1]);
        }

        [Test]
        public void GetDeploymentPlanForComponentDeploymentList()
        {
            var componentDependancies = new List<ComponentDeployment>()
            {
                new ComponentDeployment() {Name = "a", Action = ComponentAction.Skip, Dependancies = {}},
                new ComponentDeployment() {Name = "b", Action = ComponentAction.Change, Dependancies = {"a", "c"}},
                new ComponentDeployment() {Name = "c", Action = ComponentAction.Skip, Dependancies = {"b"}},
                new ComponentDeployment() {Name = "d", Action = ComponentAction.Change, Dependancies = {"a"}},
                new ComponentDeployment() {Name = "e", Action = ComponentAction.Skip, Dependancies = {"d"}},
                new ComponentDeployment() {Name = "f", Action = ComponentAction.Change, Dependancies = {"e"}},
                new ComponentDeployment() {Name = "g", Action = ComponentAction.Remove, Dependancies = {"d"}},
                new ComponentDeployment() {Name = "h", Action = ComponentAction.Change, Dependancies = {"d", "e"}},

                new ComponentDeployment() {Name = "x", Action = ComponentAction.Skip, Dependancies = {}},
                new ComponentDeployment() {Name = "y", Action = ComponentAction.Change, Dependancies = {"x"}},
                new ComponentDeployment() {Name = "z", Action = ComponentAction.Skip, Dependancies = {"y"}}
            };

            var deploymentPlanner = new DeploymentPlanner();
            var products = deploymentPlanner.GetDeploymentPlan(componentDependancies);

            var productsJson0 = JsonConvert.SerializeObject(products[0]);
            var productsJson1 = JsonConvert.SerializeObject(products[1]);
        }

        [Test]
        public void GetDeploymentPlanForComponentAdjacencyGraph()
        {
            var componentDependancies = new AdjacencyGraph<ComponentVertex, ComponentEdge>(true);

            //Add vertices

            var a = new ComponentVertex("a", ComponentAction.Skip);
            var b = new ComponentVertex("b", ComponentAction.Change);
            var c = new ComponentVertex("c", ComponentAction.Skip);
            var d = new ComponentVertex("d", ComponentAction.Change);
            var e = new ComponentVertex("e", ComponentAction.Skip);
            var f = new ComponentVertex("f", ComponentAction.Change);
            var g = new ComponentVertex("g", ComponentAction.Remove);
            var h = new ComponentVertex("h", ComponentAction.Change);

            var x = new ComponentVertex("x", ComponentAction.Skip);
            var y = new ComponentVertex("y", ComponentAction.Change);
            var z = new ComponentVertex("z", ComponentAction.Skip);

            componentDependancies.AddVertexRange(new ComponentVertex[]
            {
                z, b, c, d, e, f, g, h, a, x, y
            });

            //Create edges

            var b_a = new ComponentEdge(b, a);
            var c_b = new ComponentEdge(c, b);
            var b_c = new ComponentEdge(b, c);
            var d_a = new ComponentEdge(d, a);
            var e_d = new ComponentEdge(e, d);
            var f_e = new ComponentEdge(f, e);
            var h_e = new ComponentEdge(h, e);
            var h_d = new ComponentEdge(h, d);
            var g_d = new ComponentEdge(g, d);

            var y_x = new ComponentEdge(y, x);
            var z_y = new ComponentEdge(z, y);

            componentDependancies.AddEdgeRange(new ComponentEdge[]
            {
                z_y, c_b, b_c, d_a, e_d, f_e, h_e, h_d, g_d, b_a, y_x
            });

            var deploymentPlanner = new DeploymentPlanner();
            var products = deploymentPlanner.GetDeploymentPlan(componentDependancies);

            var productsJson0 = JsonConvert.SerializeObject(products[0]);
            var productsJson1 = JsonConvert.SerializeObject(products[1]);

            var components = JsonConvert.SerializeObject(componentDependancies.Vertices.OrderBy(propertyName => propertyName.ProductGroup).ThenBy(propertyName => propertyName.ExecutionOrder));
        }

        
    }

    
}
