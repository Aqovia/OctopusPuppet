using CommandLine;

namespace OctopusPuppet.Cmd
{
    [Verb("BranchDeployment", HelpText = @"Install branch if it exists; otherwise master to environment.")]
    class BranchDeploymentOptions
    {
        [Option("OctopusUrl",
            Required = true,
            SetName = "BranchDeployment",
            HelpText = "The url to the octopus server e.g. 'http://octopus.test.com/'")]
        public string OctopusUrl { get; set; }

        [Option("OctopusApiKey",
            Required = true,
            SetName = "BranchDeployment",
            HelpText = "The api key for the octopus server e.g. 'API-HAAAS4MM6YBBSAIQVVHCQQUEA0'")]
        public string OctopusApiKey { get; set; }

        [Option("ComponentFilterPath",
            SetName = "BranchDeployment",
            HelpText = "Component filter path.")]
        public string ComponentFilterPath { get; set; }

        [Option("Branch",
            Required = true,
            SetName = "BranchDeployment",
            HelpText = "The branch to deploy from.")]
        public string Branch { get; set; }

        [Option("TargetEnvironment",
            Required = true,
            SetName = "BranchDeployment",
            HelpText = "The environment to deploy to.")]
        public string TargetEnvironment { get; set; }

        [Option('d', "Deploy",
            SetName = "BranchDeployment",
            Default = false,
            HelpText = "Deploy")]
        public bool Deploy { get; set; }

        [Option('s', "HideDeploymentProgress",
            SetName = "BranchDeployment",
            Default = true,
            HelpText = "Hide deployment progress")]
        public bool HideDeploymentProgress { get; set; }

        [Option('p', "MaximumParalleDeployments",
            SetName = "BranchDeployment",
            Default = 4,
            HelpText = "Maximum parallel deployments")]
        public int MaximumParalleDeployments { get; set; }

        [Option("EnvironmentDeploymentPath",
            Required = false,
            Default = "",
            SetName = "BranchDeployment",
            HelpText = "Environment Deployment path to save to.")]
        public string EnvironmentDeploymentPath { get; set; }
    }
}