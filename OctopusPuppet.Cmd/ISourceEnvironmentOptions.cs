using CommandLine;

namespace OctopusPuppet.Cmd
{
    interface ISourceEnvironmentOptions
    {
        [Option("SourceEnvironment",
            SetName = "SourceEnvironment",
            HelpText = "The environment to deploy from.")]
        string SourceEnvironment { get; set; }
    }
}