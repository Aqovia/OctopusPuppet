using CommandLine;

namespace OctopusPuppet.Cmd
{
    [Verb("branchdeployment", HelpText = "Install branch if it exists; otherwise master to environment.")]
    class BranchDeploymentOptions : IOctopusOptions, IComponentFilterOptions, IInlineDeploymentOptions, IBranchOptions, IEnvironmentDeploymentPath
    {
        public string OctopusUrl { get; set; }
        public string OctopusApiKey { get; set; }
        public string ComponentFilterPath { get; set; }
        public string TargetEnvironment { get; set; }
        public bool Deploy { get; set; }
        public string Branch { get; set; }
        public string EnvironmentDeploymentPath { get; set; }
    }
}