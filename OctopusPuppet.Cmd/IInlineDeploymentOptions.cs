using CommandLine;

namespace OctopusPuppet.Cmd
{
    interface IInlineDeploymentOptions : ITargetEnvironmentOptions
    {
        [Option('d', "Deploy",
            SetName = "Deploy",
            Default = false,
            HelpText = "Deploy")]
        bool Deploy { get; set; }
    }
}