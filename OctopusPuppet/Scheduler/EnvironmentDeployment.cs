using System.Collections.Generic;

namespace OctopusPuppet.Scheduler
{
    public class EnvironmentDeployment
    {
        public List<ProductDeployment> ProductDeployments { get; set; }

        public EnvironmentDeployment(List<ProductDeployment> productDeployments)
        {
            ProductDeployments = productDeployments;
        }
    }
}
