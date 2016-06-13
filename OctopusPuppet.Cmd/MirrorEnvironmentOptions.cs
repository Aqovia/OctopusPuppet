using CommandLine;

namespace OctopusPuppet.Cmd
{
    [Verb("mirrorenvironment", HelpText = "Mirror components from one environment to the other.")]
    class MirrorEnvironmentOptions : IOctopusOptions, IComponentFilterOptions, ISourceEnvironmentOptions, IInlineDeploymentOptions, IEnvironmentDeploymentPath
    {
        public string OctopusUrl { get; set; }
        public string OctopusApiKey { get; set; }
        public string ComponentFilterPath { get; set; }
        public string TargetEnvironment { get; set; }
        public string SourceEnvironment { get; set; }
        public bool Deploy { get; set; }
        public string EnvironmentDeploymentPath { get; set; }
    }
}