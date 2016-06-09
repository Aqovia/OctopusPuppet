using CommandLine;

namespace OctopusPuppet.Cmd
{
    interface IDeploymentOptions : ITargetEnvironmentOptions
    {
        [Option("DeploymentPath",
            SetName = "DeploymentPath",
            HelpText = "Deployment path.")]
        string DeploymentPath { get; set; }
    }
}