using System;
using System.Collections.Generic;
using System.Linq;
using OctopusPuppet.Scheduler;

namespace OctopusPuppet.Gui.Model
{
    public class ProductDeploymentStepResult
    {
        public static implicit operator ProductDeploymentStepResult(ProductDeploymentStep productDeploymentStep)
        {
            return new ProductDeploymentStepResult(new List<ComponentDeploymentResult>())
            {
                ExecutionOrder = productDeploymentStep.ExecutionOrder,
                ComponentDeployments = productDeploymentStep.ComponentDeployments.Select(x => (ComponentDeploymentResult)x).ToList()
            };
        }

        public List<ComponentDeploymentResult> ComponentDeployments { get; set; }
        
        public int ExecutionOrder { get; set; }

        public TimeSpan? DeploymentDuration
        {
            get { 
                return ComponentDeployments
                .Select(x => x.Vertex.DeploymentDuration)
                .Max(); 
            }
        }

        public ProductDeploymentStepResult(List<ComponentDeploymentResult> componentDeployments)
        {
            ComponentDeployments = componentDeployments;
            ExecutionOrder = -1;
        }
    }
}