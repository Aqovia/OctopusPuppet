using System.Collections.Generic;
using System.Linq;
using OctopusPuppet.DeploymentPlanner;
using QuickGraph.Algorithms;

namespace OctopusPuppet.Scheduler
{
    public class DeploymentScheduler : IDeploymentScheduler
    {
        public ComponentGraph GetDeploymentComponentGraph(List<DeploymentPlan> componentDependancies)
        {
            var componentVertices = new Dictionary<string, ComponentVertex>();
            foreach (var componentDependancy in componentDependancies)
            {
                var version = componentDependancy.ComponentFrom == null ? string.Empty : componentDependancy.ComponentFrom.Version.ToString();
                var deploymentDuration = componentDependancy.ComponentFrom == null ? null : componentDependancy.ComponentFrom.DeploymentDuration;

                var componentVertex = new ComponentVertex(componentDependancy.Name, version, componentDependancy.Action, deploymentDuration);
                componentVertices.Add(componentDependancy.Name, componentVertex);
            }

            var componentEdges = new List<ComponentEdge>();
            foreach (var componentDependancy in componentDependancies)
            {
                var source = componentVertices[componentDependancy.Name];

                if (componentDependancy.ComponentFrom == null) continue;

                foreach (var dependancy in componentDependancy.ComponentFrom.Dependancies)
                {
                    var target = componentVertices[dependancy];
                    var componentEdge = new ComponentEdge(source, target);
                    componentEdges.Add(componentEdge);
                }
            }

            var componentDependanciesAdjacencyGraph = new ComponentGraph();
            componentDependanciesAdjacencyGraph.AddVertexRange(componentVertices.Values);
            componentDependanciesAdjacencyGraph.AddEdgeRange(componentEdges);

            return componentDependanciesAdjacencyGraph;
        }

        public List<List<ComponentGroupVertex>> GetDeploymentSchedule(ComponentGraph componentDependanciesAdjacencyGraph)
        {
            var weaklyConnectedComponents = (IDictionary<ComponentVertex, int>)new Dictionary<ComponentVertex, int>();
            componentDependanciesAdjacencyGraph.WeaklyConnectedComponents(weaklyConnectedComponents);

            //Work out related components
            foreach (var connectedComponent in weaklyConnectedComponents)
            {
                connectedComponent.Key.ProductGroup = connectedComponent.Value;
            }

            var productGroupNames = weaklyConnectedComponents.Values
                .Distinct();

            var productDeploymentPlans = productGroupNames
                .Select(productGroupName => GetComponentGroups(componentDependanciesAdjacencyGraph, productGroupName))
                .ToList();

            return productDeploymentPlans;
        }

        private List<ComponentGroupVertex> GetComponentGroups(ComponentGraph componentDependancies, int productGroup)
        {
            var adjacencyGraphForProductGroup = GetAdjacencyGraphForProductGroup(componentDependancies, productGroup);

            var numberOfComponentGroups = AddComponentGroupToComponent(adjacencyGraphForProductGroup);

            var adjacencyGraphForComponentGroup = GetAdjacencyGraphForComponentGroup(adjacencyGraphForProductGroup, numberOfComponentGroups);

            AddExecutionOrderToComponentGroupAndComponents(adjacencyGraphForComponentGroup);

            var relatedComponentGroupDependancies = adjacencyGraphForComponentGroup
                .TopologicalSort()
                .OrderBy(x => x.ExecutionOrder)
                .ThenByDescending(x => x.DeploymentDuration)
                .ToList();

            return relatedComponentGroupDependancies;
        }

        private ComponentGraph GetAdjacencyGraphForProductGroup(ComponentGraph componentDependancies, int productGroup)
        {
            var vertices = componentDependancies.Vertices
                .Where(vertex => vertex.ProductGroup == productGroup);
            var edges = componentDependancies.Edges
                .Where(edge => edge.Source.ProductGroup == productGroup);

            var relatedComponentDependancies = new ComponentGraph();
            relatedComponentDependancies.AddVertexRange(vertices);
            relatedComponentDependancies.AddEdgeRange(edges);

            return relatedComponentDependancies;
        }

        private int AddComponentGroupToComponent(ComponentGraph adjacencyGraphForProductGroup)
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

        private void AddExecutionOrderToComponentGroupAndComponents(ComponentGroupGraph adjacencyGraphForComponentGroup)
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

        private ComponentGroupGraph GetAdjacencyGraphForComponentGroup(ComponentGraph adjacencyGraphForProductGroup, int numberOfComponentGroups)
        {
            var relatedComponentGroupDependancies = new ComponentGroupGraph();

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
