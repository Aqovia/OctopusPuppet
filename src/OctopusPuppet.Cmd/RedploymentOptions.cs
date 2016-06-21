using CommandLine;

namespace OctopusPuppet.Cmd
{
    [Verb("Redeployment", HelpText = "Re-install all components in environment.")]
    class RedploymentOptions
    {
        [Option("OctopusUrl",
            Required = true,
            SetName = "Redeployment",
            HelpText = "The url to the octopus server e.g. 'http://octopus.test.com/'")]
        public string OctopusUrl { get; set; }

        [Option("OctopusApiKey",
            Required = true,
            SetName = "Redeployment",
            HelpText = "The api key for the octopus server e.g. 'API-HAAAS4MM6YBBSAIQVVHCQQUEA0'")]
        public string OctopusApiKey { get; set; }

        [Option("ComponentFilterPath",
            SetName = "Redeployment",
            HelpText = "Component filter path.")]
        public string ComponentFilterPath { get; set; }

        [Option("TargetEnvironment",
            Required = true,
            SetName = "Redeployment",
            HelpText = "The environment to deploy to.")]
        public string TargetEnvironment { get; set; }

        [Option('d', "Deploy",
            SetName = "Redeployment",
            Default = false,
            HelpText = "Deploy")]
        public bool Deploy { get; set; }

        [Option('s', "HideDeploymentProgress",
            SetName = "Redeployment",
            Default = true,
            HelpText = "Hide deployment progress")]
        public bool HideDeploymentProgress { get; set; }

        [Option('p', "MaximumParalleDeployments",
            SetName = "Redeployment",
            Default = 4,
            HelpText = "Maximum parallel deployments")]
        public int MaximumParalleDeployments { get; set; }

        [Option("EnvironmentDeploymentPath",
            Required = false,
            Default = "",
            SetName = "Redeployment",
            HelpText = "Environment Deployment path to save to.")]
        public string EnvironmentDeploymentPath { get; set; }
    }
}