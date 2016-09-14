using System;
using System.Diagnostics;
using OctopusPuppet.DeploymentPlanner;

namespace OctopusPuppet.Scheduler
{
    [DebuggerDisplay("{Name} {Version}")]
    public class ComponentDeploymentVertex
    {
        public string Id { get; private set; }
        public string Name { get; private set; }

        /// <summary>
        /// The version of the component that should be deployed
        /// </summary>
        public SemVer Version { get; private set; }

        public PlanAction DeploymentAction { get; set; }
        public VariableAction VariableAction { get; set; }
        public TimeSpan? DeploymentDuration { get; set; }
        public bool Exists { get; set; }

        public int ComponentGroup { get; set; }
        public int ProductGroup { get; set; }
        public int ExecutionOrder { get; set; }

        public ComponentDeploymentVertex(string id, string name, SemVer version, PlanAction action, TimeSpan? deploymentDuration, bool exists = true)
        {
            Id = id;
            Name = name;
            Version = version;
            DeploymentAction = action;

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