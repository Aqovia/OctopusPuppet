using System;
using System.Collections.Generic;
using System.Linq;

namespace OctopusPuppet.Scheduler
{
    public class ProductDeployment 
    {
        public List<ProductDeploymentStep> DeploymentSteps { get; set; }

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

        public ProductDeployment(List<ProductDeploymentStep> deploymentSteps)
        {
            DeploymentSteps = deploymentSteps;
        }
    }
}