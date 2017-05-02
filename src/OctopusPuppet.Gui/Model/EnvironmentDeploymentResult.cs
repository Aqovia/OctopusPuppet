using System.Collections.Generic;
using System.Linq;
using OctopusPuppet.Scheduler;


namespace OctopusPuppet.Gui.Model
{
    public class EnvironmentDeploymentResult
    {
        public static implicit operator EnvironmentDeploymentResult(EnvironmentDeployment environmentDeployment)
        {
            if (environmentDeployment == null)
            {
                return null;
            }

            return new EnvironmentDeploymentResult(new List<ProductDeploymentResult>())
            {
                ProductDeployments = environmentDeployment.ProductDeployments.Select(x => (ProductDeploymentResult)x).ToList()
            };
        }

        public List<ProductDeploymentResult> ProductDeployments { get; set; }

        public EnvironmentDeploymentResult(List<ProductDeploymentResult> productDeployments)
        {
            ProductDeployments = productDeployments;
        }
    }
}
