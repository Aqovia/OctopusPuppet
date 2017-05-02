using System;
using System.Collections.Generic;
using System.Linq;
using OctopusPuppet.Scheduler;

namespace OctopusPuppet.Gui.Model
{
    public class ProductDeploymentResult
    {
        public static implicit operator ProductDeploymentResult(ProductDeployment productDeployment)
        {
            if (productDeployment == null)
            {
                return null;
            }

            return new ProductDeploymentResult(new List<ProductDeploymentStepResult>())
            {
                DeploymentSteps = productDeployment.DeploymentSteps.Select(x => (ProductDeploymentStepResult)x).ToList()
            };
        }

        public List<ProductDeploymentStepResult> DeploymentSteps { get; set; }

        public TimeSpan? DeploymentDuration
        {
            get
            {
                if (DeploymentSteps == null)
                {
                    return null;
                }

                var deploymentDurations = DeploymentSteps
                    .Select(x => x.DeploymentDuration)
                    .ToList();

                if (deploymentDurations.Any(x => !x.HasValue))
                {
                    return null;
                }

                var totalDuration = new TimeSpan(0);
                totalDuration = deploymentDurations
                    .Where(x => x.HasValue)
                    .Aggregate(totalDuration, (current, deploymentDuration) => current.Add(deploymentDuration.Value));

                return totalDuration;
            }
        }

        public ProductDeploymentResult(List<ProductDeploymentStepResult> deploymentSteps)
        {
            DeploymentSteps = deploymentSteps;
        }
    }
}
