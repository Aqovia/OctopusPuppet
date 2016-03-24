using System.Collections.Generic;
using System.Linq;
using OctopusPuppet.DeploymentPlanner;
using QuickGraph.Algorithms;

namespace OctopusPuppet.Scheduler
{
    public class DeploymentScheduler : IDeploymentScheduler
    {
        public ComponentDeploymentGraph GetComponentDeploymentGraph(EnvironmentDeploymentPlan environmentDeploymentPlan)
        {
            var componentVertices = new Dictionary<string, ComponentDeploymentVertex>();
            foreach (var componentDependancy in environmentDeploymentPlan.DeploymentPlans)
            {
                var version = componentDependancy.ComponentFrom == null ? string.Empty : componentDependancy.ComponentFrom.Version.ToString();
                var deploymentDuration = componentDependancy.ComponentFrom == null ? null : componentDependancy.ComponentFrom.DeploymentDuration;

                var componentVertex = new ComponentDeploymentVertex(componentDependancy.Name, version, componentDependancy.Action, deploymentDuration);
                componentVertices.Add(componentDependancy.Name, componentVertex);
            }

            var componentEdges = new List<ComponentDeploymentEdge>();
            foreach (var componentDependancy in environmentDeploymentPlan.DeploymentPlans)
            {
                var source = componentVertices[componentDependancy.Name];

                if (componentDependancy.ComponentFrom == null) continue;

                foreach (var dependancy in componentDependancy.ComponentFrom.Dependancies)
                {
                    var target = componentVertices[dependancy];
                    var componentEdge = new ComponentDeploymentEdge(source, target);
                    componentEdges.Add(componentEdge);
                }
            }

            var componentDependanciesAdjacencyGraph = new ComponentDeploymentGraph();
            componentDependanciesAdjacencyGraph.AddVertexRange(componentVertices.Values);
            componentDependanciesAdjacencyGraph.AddEdgeRange(componentEdges);

            return componentDependanciesAdjacencyGraph;
        }

        public EnvironmentDeployment GetEnvironmentDeployment(ComponentDeploymentGraph componentDeploymentDependanciesAdjacencyGraph)
        {
            var weaklyConnectedComponents = (IDictionary<ComponentDeploymentVertex, int>)new Dictionary<ComponentDeploymentVertex, int>();
            componentDeploymentDependanciesAdjacencyGraph.WeaklyConnectedComponents(weaklyConnectedComponents);

            //Work out related components
            foreach (var connectedComponent in weaklyConnectedComponents)
            {
                connectedComponent.Key.ProductGroup = connectedComponent.Value;
            }

            var productGroupNames = weaklyConnectedComponents.Values
                .Distinct();

            var productDeploymentPlans = productGroupNames
                .Select(productGroupName => GetComponentGroups(componentDeploymentDependanciesAdjacencyGraph, productGroupName))
                .ToList();

            var environmentDeployment = new EnvironmentDeployment(productDeploymentPlans);
            return environmentDeployment;
        }

        private ProductDeployment GetComponentGroups(ComponentDeploymentGraph componentDeploymentDependancies, int productGroup)
        {
            var adjacencyGraphForProductGroup = GetAdjacencyGraphForProductGroup(componentDeploymentDependancies, productGroup);

            var numberOfComponentGroups = AddComponentGroupToComponent(adjacencyGraphForProductGroup);

            var adjacencyGraphForComponentGroup = GetAdjacencyGraphForComponentGroup(adjacencyGraphForProductGroup, numberOfComponentGroups);

            AddExecutionOrderToComponentGroupAndComponents(adjacencyGraphForComponentGroup);

            var relatedComponentGroupDependancies = adjacencyGraphForComponentGroup
                .TopologicalSort()
                .OrderBy(x => x.ExecutionOrder)
                .ThenByDescending(x => x.DeploymentDuration)
                .ToList();

            var productDeployment = new ProductDeployment(relatedComponentGroupDependancies);

            return productDeployment;
        }

        private ComponentDeploymentGraph GetAdjacencyGraphForProductGroup(ComponentDeploymentGraph componentDeploymentDependancies, int productGroup)
        {
            var vertices = componentDeploymentDependancies.Vertices
                .Where(vertex => vertex.ProductGroup == productGroup);
            var edges = componentDeploymentDependancies.Edges
                .Where(edge => edge.Source.ProductGroup == productGroup);

            var relatedComponentDependancies = new ComponentDeploymentGraph();
            relatedComponentDependancies.AddVertexRange(vertices);
            relatedComponentDependancies.AddEdgeRange(edges);

            return relatedComponentDependancies;
        }

        private int AddComponentGroupToComponent(ComponentDeploymentGraph adjacencyDeploymentGraphForProductGroup)
        {
            var stronglyConnectedComponents = (IDictionary<ComponentDeploymentVertex, int>)new Dictionary<ComponentDeploymentVertex, int>();
            var numberOfComponentGroups = adjacencyDeploymentGraphForProductGroup.StronglyConnectedComponents(out stronglyConnectedComponents);

            //Work out execution order of related components
            foreach (var connectedComponent in stronglyConnectedComponents)
            {
                connectedComponent.Key.ComponentGroup = connectedComponent.Value;
            }

            return numberOfComponentGroups;
        }

        private void AddExecutionOrderToComponentGroupAndComponents(DeploymentStepGraph adjacencyGraphForDeploymentStep)
        {
            var adjacencyGraphForComponentGroupThatAreNotProcessed = adjacencyGraphForDeploymentStep.Clone();

            var executionOrder = -1;
            while (!adjacencyGraphForComponentGroupThatAreNotProcessed.IsVerticesEmpty)
            {
                executionOrder++;
                var rootComponentGroupVertices = adjacencyGraphForComponentGroupThatAreNotProcessed.Roots().ToList();

                foreach (var rootComponentGroupVertex in rootComponentGroupVertices)
                {
                    rootComponentGroupVertex.ExecutionOrder = executionOrder;

                    foreach (var componentVertex in rootComponentGroupVertex.ComponentDeployments)
                    {
                        componentVertex.ExecutionOrder = executionOrder;
                    }

                    adjacencyGraphForComponentGroupThatAreNotProcessed.RemoveVertex(rootComponentGroupVertex);
                }
            }
        }

        private DeploymentStepGraph GetAdjacencyGraphForComponentGroup(ComponentDeploymentGraph adjacencyDeploymentGraphForProductGroup, int numberOfComponentGroups)
        {
            var relatedComponentGroupDependancies = new DeploymentStepGraph();

            for (var j = 0; j < numberOfComponentGroups; j++)
            {
                var stepGroup = j;

                var vertices = adjacencyDeploymentGraphForProductGroup.Vertices
                    .Where(vertex => vertex.ComponentGroup == stepGroup)
                    .ToList();

                var edges = adjacencyDeploymentGraphForProductGroup.Edges
                    .Where(edge => edge.Source.ComponentGroup == stepGroup)
                    .ToList();

                var componentGroupVertex = new ProductDeploymentStep(vertices, edges);
                relatedComponentGroupDependancies.AddVertex(componentGroupVertex);
            }

            foreach (var componentGroupVertexSource in relatedComponentGroupDependancies.Vertices)
            {
                var relatedComponentGroupDependancyEdges = componentGroupVertexSource.Edges;
                var relatedComponentGroupDependancyVertices = componentGroupVertexSource.ComponentDeployments;
                var componentGroupExternalEdges = relatedComponentGroupDependancyEdges
                    .Where(edge => relatedComponentGroupDependancyVertices.All(vertex => vertex != edge.Target));

                foreach (var componentGroupExternalEdge in componentGroupExternalEdges)
                {
                    var componentGroupVertexTarget = relatedComponentGroupDependancies.Vertices
                        .First(vertex => vertex.ComponentDeployments.Any(x => x == componentGroupExternalEdge.Target));

                    var componentGroupEdge = new DeploymentStepEdge(componentGroupVertexSource, componentGroupVertexTarget);
                    relatedComponentGroupDependancies.AddEdge(componentGroupEdge);
                }
            }

            return relatedComponentGroupDependancies;
        }
    }
}
