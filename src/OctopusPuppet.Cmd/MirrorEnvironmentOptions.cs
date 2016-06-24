using CommandLine;

namespace OctopusPuppet.Cmd
{
    [Verb("MirrorEnvironment", HelpText = "Mirror components from one environment to the other.")]
    class MirrorEnvironmentOptions
    {
        [Option("OctopusUrl",
            Required = true,
            SetName = "MirrorEnvironment",
            HelpText = "The url to the octopus server e.g. 'http://octopus.test.com/'")]
        public string OctopusUrl { get; set; }

        [Option("OctopusApiKey",
            Required = true,
            SetName = "MirrorEnvironment",
            HelpText = "The api key for the octopus server e.g. 'API-HAAAS4MM6YBBSAIQVVHCQQUEA0'")]
        public string OctopusApiKey { get; set; }

        [Option("ComponentFilterPath",
            SetName = "MirrorEnvironment",
            HelpText = "Component filter path.")]
        public string ComponentFilterPath { get; set; }

        [Option("ComponentFilter",
            SetName = "MirrorEnvironment",
            HelpText = "Component filter json base64 encoded.")]
        public string ComponentFilter { get; set; }

        [Option("TargetEnvironment",
            Required = true,
            SetName = "MirrorEnvironment",
            HelpText = "The environment to deploy to.")]
        public string TargetEnvironment { get; set; }

        [Option("SourceEnvironment",
            Required = true,
            SetName = "MirrorEnvironment",
            HelpText = "The environment to deploy from.")]
        public string SourceEnvironment { get; set; }

        [Option('d', "Deploy",
            SetName = "MirrorEnvironment",
            Default = false,
            HelpText = "Deploy")]
        public bool Deploy { get; set; }

        [Option('s', "HideDeploymentProgress",
            SetName = "MirrorEnvironment",
            Default = true,
            HelpText = "Hide deployment progress")]
        public bool HideDeploymentProgress { get; set; }

        [Option('p', "MaximumParallelDeployments",
            SetName = "MirrorEnvironment",
            Default = 4,
            HelpText = "Maximum parallel deployments")]
        public int MaximumParalleDeployments { get; set; }

        [Option("EnvironmentDeploymentPath",
            Required = false,
            Default = "",
            SetName = "MirrorEnvironment",
            HelpText = "Environment Deployment path to save to.")]
        public string EnvironmentDeploymentPath { get; set; }
    }
}