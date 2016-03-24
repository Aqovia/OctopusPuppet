using System;
using System.Collections.Generic;
using System.Linq;

namespace OctopusPuppet.Scheduler
{
    public class ProductDeploymentStep
    {
        public List<ComponentDeploymentVertex> ComponentDeployments { get; set; }
        public List<ComponentDeploymentEdge> Edges { get; set; }

        public int ExecutionOrder { get; set; }

        public TimeSpan? DeploymentDuration
        {
            get { 
                return ComponentDeployments
                .Select(x => x.DeploymentDuration)
                .Max(); 
            }
        }

        public ProductDeploymentStep(List<ComponentDeploymentVertex> componentDeployments, List<ComponentDeploymentEdge> edges)
        {
            ComponentDeployments = componentDeployments;
            Edges = edges;
            ExecutionOrder = -1;
        }
    }
}