using CommandLine;

namespace OctopusPuppet.Cmd
{
    interface IBranchOptions
    {
        [Option("Branch",
            Required = true,
            SetName = "Branch",
            HelpText = "The branch to deploy from.")]
        string Branch { get; set; }
    }
}