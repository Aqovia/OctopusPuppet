using CommandLine;

namespace OctopusPuppet.Cmd
{
    [Verb("redeployment", HelpText = "Re-install all components in environment.")]
    class RedploymentOptions : IOctopusOptions, IComponentFilterOptions, IInlineDeploymentOptions, IEnvironmentDeploymentPath
    {
        public string OctopusUrl { get; set; }
        public string OctopusApiKey { get; set; }
        public string ComponentFilterPath { get; set; }
        public string TargetEnvironment { get; set; }
        public bool Deploy { get; set; }
        public string EnvironmentDeploymentPath { get; set; }
    }
}