﻿namespace OctopusPuppet.DeploymentPlanner
{
    public class Branch
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
