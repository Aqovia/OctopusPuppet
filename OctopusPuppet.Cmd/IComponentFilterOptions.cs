using CommandLine;

namespace OctopusPuppet.Cmd
{
    interface IComponentFilterOptions
    {
        [Option("ComponentFilterPath",
            SetName = "ComponentFilterPath",
            HelpText = "Component filter path.")]
        string ComponentFilterPath { get; set; }
    }
}