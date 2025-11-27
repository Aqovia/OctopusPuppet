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
            Default = "",
            SetName = "BranchDeployment",
            HelpText = "Component filter path.")]
        public string ComponentFilterPath { get; set; }

        [Option("ComponentFilter",
            Default = "",
            SetName = "BranchDeployment",
            HelpText = "Component filter json base64 encoded.")]
        public string ComponentFilter { get; set; }

        [Option("Branch",
            Default = "",
            SetName = "BranchDeployment",
            HelpText = "The branch to deploy from.")]
        public string Branch { get; set; }

        [Option("TargetEnvironment",
            Required = true,
            SetName = "BranchDeployment",
            HelpText = "The environment to deploy to.")]
        public string TargetEnvironment { get; set; }

        [Option('d', "Deploy",
            Default = false,
            SetName = "BranchDeployment",
            HelpText = "Deploy")]
        public bool Deploy { get; set; }

        [Option('e', "SkipMasterBranch",
            Default = false,
            SetName = "BranchDeployment",
            HelpText = "Skip deployment of components from master/main branch (versions without a branch suffix")]
        public bool SkipMasterBranch { get; set; }

        [Option('f', "DoNotUseDifferentialDeployment",
            Default = false,
            SetName = "BranchDeployment",
            HelpText = "Do not use differential deployment")]
        public bool DoNotUseDifferentialDeployment { get; set; }

        [Option('u', "UpdateVariables",
             Default = false,
             SetName = "BranchDeployment",
             HelpText = "Update variables")]
        public bool UpdateVariables { get; set; }

        [Option('s', "HideDeploymentProgress",
            Default = false,
            SetName = "BranchDeployment",
            HelpText = "Hide deployment progress")]
        public bool HideDeploymentProgress { get; set; }

        [Option('p', "MaximumParallelDeployments",
            Default = 2,
            SetName = "BranchDeployment",
            HelpText = "Maximum parallel deployments")]
        public int MaximumParallelDeployments { get; set; }

        [Option("EnvironmentDeploymentPath",
            Default = "",
            SetName = "BranchDeployment",
            HelpText = "Environment Deployment path to save to.")]
        public string EnvironmentDeploymentPath { get; set; }

        [Option("Teamcity",
            Default = false,
            SetName = "BranchDeployment",
            HelpText = "Use Teamcity output")]
        public bool Teamcity { get; set; }
    }
}