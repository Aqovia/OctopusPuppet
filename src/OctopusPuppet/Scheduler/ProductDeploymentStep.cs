using System;
using System.Collections.Generic;
using System.Linq;

namespace OctopusPuppet.Scheduler
{
    public class ProductDeploymentStep
    {
        public List<ComponentDeployment> ComponentDeployments { get; set; }
        
        public int ExecutionOrder { get; set; }

        public TimeSpan? DeploymentDuration
        {
            get { 
                return ComponentDeployments
                .Select(x => x.Vertex.DeploymentDuration)
                .Max(); 
            }
        }

        public ProductDeploymentStep(List<ComponentDeployment> componentDeployments)
        {
            ComponentDeployments = componentDeployments;
            ExecutionOrder = -1;
        }
    }
}