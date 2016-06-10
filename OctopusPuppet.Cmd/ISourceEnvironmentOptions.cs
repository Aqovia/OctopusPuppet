using CommandLine;

namespace OctopusPuppet.Cmd
{
    interface ISourceEnvironmentOptions
    {
        [Option("SourceEnvironment",
            Required = true,
            SetName = "SourceEnvironment",
            HelpText = "The environment to deploy from.")]
        string SourceEnvironment { get; set; }
    }
}