using CommandLine;

namespace OctopusPuppet.Cmd
{
    interface IDeploymentOptions : ITargetEnvironmentOptions
    {
        [Option("EnvironmentDeploymentPath",
            Required = true,
            SetName = "EnvironmentDeploymentPath",
            HelpText = "Environment Deployment path to load from.")]
        string EnvironmentDeploymentPath { get; set; }
    }
}