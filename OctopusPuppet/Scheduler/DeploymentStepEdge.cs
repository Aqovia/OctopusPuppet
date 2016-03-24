using QuickGraph;

namespace OctopusPuppet.Scheduler
{
    public class DeploymentStepEdge : Edge<ProductDeploymentStep>
    {
        public DeploymentStepEdge(ProductDeploymentStep source, ProductDeploymentStep target)
            : base(source, target)
        {
        }
    }
}
