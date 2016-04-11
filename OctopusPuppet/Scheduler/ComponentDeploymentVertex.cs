using System;
using System.Diagnostics;
using OctopusPuppet.DeploymentPlanner;

namespace OctopusPuppet.Scheduler
{
    [DebuggerDisplay("{Name} {Version}")]
    public class ComponentDeploymentVertex
    {
        public string Name { get; private set; }
        public string Version { get; private set; }
        public PlanAction Action { get; private set; }
        public TimeSpan? DeploymentDuration { get; set; }
        public bool Exists { get; set; }

        public int ComponentGroup { get; set; }
        public int ProductGroup { get; set; }
        public int ExecutionOrder { get; set; }

        public ComponentDeploymentVertex(string name, string version, PlanAction action, TimeSpan? deploymentDuration, bool exists = true)
        {
            Name = name;
            Version = version;
            Action = action;
            DeploymentDuration = deploymentDuration;
            Exists = exists;

            ComponentGroup = -1;
            ProductGroup = -1;
            ExecutionOrder = -1;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Name, Version);
        }
    }
}