using QuickGraph;

namespace OctopusPuppet.Scheduler
{
    public class DeploymentStepGraph : AdjacencyGraph<ProductDeploymentStep, DeploymentStepEdge>
    {
        public DeploymentStepGraph() : base(true)
        {
            
        }
    }
}
