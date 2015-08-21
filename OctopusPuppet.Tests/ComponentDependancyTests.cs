using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Graphviz;

namespace OctopusPuppet.Tests
{
    public class ComponentDependancyTests
    {
        [Test(Description = "Sort Components")]
        public void SortComponents()
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

            var products = GetProducts(componentDependancies);

            var productsJson0 = JsonConvert.SerializeObject(products[0]);
            var productsJson1 = JsonConvert.SerializeObject(products[1]);

            var components = JsonConvert.SerializeObject(componentDependancies.Vertices.OrderBy(propertyName => propertyName.ProductGroup).ThenBy(propertyName => propertyName.ExecutionOrder));
        }

        private List<IEnumerable<ComponentGroupVertex>> GetProducts(AdjacencyGraph<ComponentVertex, ComponentEdge> componentDependancies)
        {
            var weaklyConnectedComponents = (IDictionary<ComponentVertex, int>)new Dictionary<ComponentVertex, int>();
            var numberOfProductGroups = componentDependancies.WeaklyConnectedComponents(weaklyConnectedComponents);

            //Work out related components
            foreach (var connectedComponent in weaklyConnectedComponents)
            {
                connectedComponent.Key.ProductGroup = connectedComponent.Value;
            }

            var componentGroups = new List<IEnumerable<ComponentGroupVertex>>();

            for (var i = 0; i < numberOfProductGroups; i++)
            {
                var relatedComponentGroupDependancies = GetComponentGroups(componentDependancies, i);
                componentGroups.Add(relatedComponentGroupDependancies);
            }

            return componentGroups;
        }

        private IEnumerable<ComponentGroupVertex> GetComponentGroups(AdjacencyGraph<ComponentVertex, ComponentEdge> componentDependancies, int productGroup)
        {
            var vertices = componentDependancies.Vertices
                    .Where(vertex => vertex.ProductGroup == productGroup);
            var edges = componentDependancies.Edges
                .Where(edge => edge.Source.ProductGroup == productGroup);

            var relatedComponentDependancies = new AdjacencyGraph<ComponentVertex, ComponentEdge>(true);
            relatedComponentDependancies.AddVertexRange(vertices);
            relatedComponentDependancies.AddEdgeRange(edges);

            var stronglyConnectedComponents = (IDictionary<ComponentVertex, int>)new Dictionary<ComponentVertex, int>();
            var numberOfComponentGroups = relatedComponentDependancies.StronglyConnectedComponents(out stronglyConnectedComponents);

            //Work out execution order of related components
            foreach (var connectedComponent in stronglyConnectedComponents)
            {
                connectedComponent.Key.ComponentGroup = connectedComponent.Value;
            }

            var adjacencyGraphForComponentGroup = GetAdjacencyGraphForComponentGroup(relatedComponentDependancies, numberOfComponentGroups);

            AddExecutionOrder(adjacencyGraphForComponentGroup);

            var relatedComponentGroupDependancies = adjacencyGraphForComponentGroup.TopologicalSort();

            return relatedComponentGroupDependancies;
        }

        private void AddExecutionOrder(AdjacencyGraph<ComponentGroupVertex, ComponentGroupEdge> adjacencyGraphForComponentGroup)
        {
            var parallelExecutionAdjacencyGraph = adjacencyGraphForComponentGroup.Clone();

            var executionOrder = -1;
            while (!parallelExecutionAdjacencyGraph.IsVerticesEmpty)
            {
                executionOrder++;
                var rootComponentGroupVertices = parallelExecutionAdjacencyGraph.Roots().ToList();

                foreach (var rootComponentGroupVertex in rootComponentGroupVertices)
                {
                    rootComponentGroupVertex.ExecutionOrder = executionOrder;

                    foreach (var componentVertex in rootComponentGroupVertex.Vertices)
                    {
                        componentVertex.ExecutionOrder = executionOrder;
                    }
                    
                    parallelExecutionAdjacencyGraph.RemoveVertex(rootComponentGroupVertex);
                }
            }
        }

        private AdjacencyGraph<ComponentGroupVertex, ComponentGroupEdge> GetAdjacencyGraphForComponentGroup(AdjacencyGraph<ComponentVertex, ComponentEdge> relatedComponentDependancies, int numberOfComponentGroups)
        {
            var relatedComponentGroupDependancies = new AdjacencyGraph<ComponentGroupVertex, ComponentGroupEdge>(true);
            
            for (var j = 0; j < numberOfComponentGroups; j++)
            {
                var stepGroup = j;

                var vertices = relatedComponentDependancies.Vertices
                    .Where(vertex => vertex.ComponentGroup == stepGroup);

                var edges = relatedComponentDependancies.Edges
                    .Where(edge => edge.Source.ComponentGroup == stepGroup);

                var componentGroupVertex = new ComponentGroupVertex(vertices, edges);
                relatedComponentGroupDependancies.AddVertex(componentGroupVertex);
            }

            foreach (var componentGroupVertexSource in relatedComponentGroupDependancies.Vertices)
            {
                var relatedComponentGroupDependancyEdges = componentGroupVertexSource.Edges;
                var relatedComponentGroupDependancyVertices = componentGroupVertexSource.Vertices;
                var componentGroupExternalEdges = relatedComponentGroupDependancyEdges
                    .Where(edge => relatedComponentGroupDependancyVertices.All(vertex => vertex != edge.Target));

                foreach (var componentGroupExternalEdge in componentGroupExternalEdges)
                {
                    var componentGroupVertexTarget = relatedComponentGroupDependancies.Vertices
                        .First(vertex => vertex.Vertices.Any(x => x == componentGroupExternalEdge.Target));

                    var componentGroupEdge = new ComponentGroupEdge(componentGroupVertexSource, componentGroupVertexTarget);
                    relatedComponentGroupDependancies.AddEdge(componentGroupEdge);
                }
            }

            return relatedComponentGroupDependancies;
        }
    }

    
}
