using System;

namespace OctopusPuppet.Scheduler
{
    public class ComponentVertex
    {
        public string OctopusProject { get; private set; }
        public string Version { get; private set; }
        public ComponentAction Action { get; private set; }
        public TimeSpan? DeploymentDuration { get; set; }

        public int ComponentGroup { get; set; }
        public int ProductGroup { get; set; }
        public int ExecutionOrder { get; set; }

        public ComponentVertex(string octopusProject, string version, ComponentAction action, TimeSpan? deploymentDuration)
        {
            OctopusProject = octopusProject;
            Version = version;
            Action = action;
            DeploymentDuration = deploymentDuration;

            ComponentGroup = -1;
            ProductGroup = -1;
            ExecutionOrder = -1;
        }
    }
}
