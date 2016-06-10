using CommandLine;

namespace OctopusPuppet.Cmd
{
    interface ITargetEnvironmentOptions
    {
        [Option("TargetEnvironment",
            Required = true,
            SetName = "TargetEnvironment",
            HelpText = "The environment to deploy to.")]
        string TargetEnvironment { get; set; }
    }
}