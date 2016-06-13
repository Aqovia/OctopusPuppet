using CommandLine;

namespace OctopusPuppet.Cmd
{
    [Verb("deploy", HelpText = "Deploy to environment.")]
    class DeployOptions : IOctopusOptions, IDeploymentOptions
    {
        public string OctopusUrl { get; set; }
        public string OctopusApiKey { get; set; }
        public string EnvironmentDeploymentPath { get; set; }
        public string TargetEnvironment { get; set; }
    }
}
