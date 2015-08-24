using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using QuickGraph.Algorithms;

namespace OctopusPuppet
{
    public class DeploymentPlanner
    {
        public List<IEnumerable<ComponentGroupVertex>> GetDeploymentPlan(IEnumerable<ComponentDeployment> componentDependancies)
        {
            var componentVertices = new Dictionary<string, ComponentVertex>();
            foreach (var componentDependancy in componentDependancies)
            {
                var componentVertex = new ComponentVertex(componentDependancy.Name, componentDependancy.Action);
                componentVertices.Add(componentDependancy.Name, componentVertex);
            }

            var componentEdges = new List<ComponentEdge>();
            foreach (var componentDependancy in componentDependancies)
            {
                var source = componentVertices[componentDependancy.Name];
                foreach (var dependancy in componentDependancy.Dependancies)
                {
                    var target = componentVertices[dependancy];
                    var componentEdge = new ComponentEdge(source, target);
                    componentEdges.Add(componentEdge);
                }
            }

            var componentDependanciesAdjacencyGraph = new AdjacencyGraph<ComponentVertex, ComponentEdge>(true);
            componentDependanciesAdjacencyGraph.AddVertexRange(componentVertices.Values);
            componentDependanciesAdjacencyGraph.AddEdgeRange(componentEdges);

            return GetDeploymentPlan(componentDependanciesAdjacencyGraph);
        }

        public List<IEnumerable<ComponentGroupVertex>> GetDeploymentPlan(AdjacencyGraph<ComponentVertex, ComponentEdge> componentDependanciesAdjacencyGraph)
        {
            var weaklyConnectedComponents = (IDictionary<ComponentVertex, int>)new Dictionary<ComponentVertex, int>();
            var numberOfProductGroups = componentDependanciesAdjacencyGraph.WeaklyConnectedComponents(weaklyConnectedComponents);

            //Work out related components
            foreach (var connectedComponent in weaklyConnectedComponents)
            {
                connectedComponent.Key.ProductGroup = connectedComponent.Value;
            }

            var componentGroups = new List<IEnumerable<ComponentGroupVertex>>();

            for (var i = 0; i < numberOfProductGroups; i++)
            {
                var relatedComponentGroupDependancies = GetComponentGroups(componentDependanciesAdjacencyGraph, i);
                componentGroups.Add(relatedComponentGroupDependancies);
            }

            return componentGroups;
        }

        private IEnumerable<ComponentGroupVertex> GetComponentGroups(AdjacencyGraph<ComponentVertex, ComponentEdge> componentDependancies, int productGroup)
        {
            var adjacencyGraphForProductGroup = GetAdjacencyGraphForProductGroup(componentDependancies, productGroup);

            var numberOfComponentGroups = AddComponentGroupToComponent(adjacencyGraphForProductGroup);

            var adjacencyGraphForComponentGroup = GetAdjacencyGraphForComponentGroup(adjacencyGraphForProductGroup, numberOfComponentGroups);

            AddExecutionOrderToComponentGroupAndComponents(adjacencyGraphForComponentGroup);

            var relatedComponentGroupDependancies = adjacencyGraphForComponentGroup
                .TopologicalSort()
                .OrderBy(x => x.ExecutionOrder);

            return relatedComponentGroupDependancies;
        }

        private AdjacencyGraph<ComponentVertex, ComponentEdge> GetAdjacencyGraphForProductGroup(AdjacencyGraph<ComponentVertex, ComponentEdge> componentDependancies, int productGroup)
        {
            var vertices = componentDependancies.Vertices
                .Where(vertex => vertex.ProductGroup == productGroup);
            var edges = componentDependancies.Edges
                .Where(edge => edge.Source.ProductGroup == productGroup);

            var relatedComponentDependancies = new AdjacencyGraph<ComponentVertex, ComponentEdge>(true);
            relatedComponentDependancies.AddVertexRange(vertices);
            relatedComponentDependancies.AddEdgeRange(edges);

            return relatedComponentDependancies;
        }

        private int AddComponentGroupToComponent(AdjacencyGraph<ComponentVertex, ComponentEdge> adjacencyGraphForProductGroup)
        {
            var stronglyConnectedComponents = (IDictionary<ComponentVertex, int>)new Dictionary<ComponentVertex, int>();
            var numberOfComponentGroups = adjacencyGraphForProductGroup.StronglyConnectedComponents(out stronglyConnectedComponents);

            //Work out execution order of related components
            foreach (var connectedComponent in stronglyConnectedComponents)
            {
                connectedComponent.Key.ComponentGroup = connectedComponent.Value;
            }

            return numberOfComponentGroups;
        }

        private void AddExecutionOrderToComponentGroupAndComponents(AdjacencyGraph<ComponentGroupVertex, ComponentGroupEdge> adjacencyGraphForComponentGroup)
        {
            var adjacencyGraphForComponentGroupThatAreNotProcessed = adjacencyGraphForComponentGroup.Clone();

            var executionOrder = -1;
            while (!adjacencyGraphForComponentGroupThatAreNotProcessed.IsVerticesEmpty)
            {
                executionOrder++;
                var rootComponentGroupVertices = adjacencyGraphForComponentGroupThatAreNotProcessed.Roots().ToList();

                foreach (var rootComponentGroupVertex in rootComponentGroupVertices)
                {
                    rootComponentGroupVertex.ExecutionOrder = executionOrder;

                    foreach (var componentVertex in rootComponentGroupVertex.Vertices)
                    {
                        componentVertex.ExecutionOrder = executionOrder;
                    }

                    adjacencyGraphForComponentGroupThatAreNotProcessed.RemoveVertex(rootComponentGroupVertex);
                }
            }
        }

        private AdjacencyGraph<ComponentGroupVertex, ComponentGroupEdge> GetAdjacencyGraphForComponentGroup(AdjacencyGraph<ComponentVertex, ComponentEdge> adjacencyGraphForProductGroup, int numberOfComponentGroups)
        {
            var relatedComponentGroupDependancies = new AdjacencyGraph<ComponentGroupVertex, ComponentGroupEdge>(true);

            for (var j = 0; j < numberOfComponentGroups; j++)
            {
                var stepGroup = j;

                var vertices = adjacencyGraphForProductGroup.Vertices
                    .Where(vertex => vertex.ComponentGroup == stepGroup);

                var edges = adjacencyGraphForProductGroup.Edges
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
