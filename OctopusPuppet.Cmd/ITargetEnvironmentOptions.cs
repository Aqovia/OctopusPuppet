using CommandLine;

namespace OctopusPuppet.Cmd
{
    interface ITargetEnvironmentOptions
    {
        [Option("TargetEnvironment",
            SetName = "TargetEnvironment",
            HelpText = "The environment to deploy to.")]
        string TargetEnvironment { get; set; }
    }
}