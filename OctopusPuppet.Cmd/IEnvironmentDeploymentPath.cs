using CommandLine;

namespace OctopusPuppet.Cmd
{
    interface IEnvironmentDeploymentPath
    {
        [Option("EnvironmentDeploymentPath",
            Required = false, 
            Default = "",
            SetName = "EnvironmentDeploymentPath",
            HelpText = "Environment Deployment path to save to.")]
        string EnvironmentDeploymentPath { get; set; }
    }
}