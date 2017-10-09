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
            Default = "",
            SetName = "MirrorEnvironment",
            HelpText = "Component filter path.")]
        public string ComponentFilterPath { get; set; }

        [Option("ComponentFilter",
            Default = "",
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
            Default = false,
            SetName = "MirrorEnvironment",
            HelpText = "Deploy")]
        public bool Deploy { get; set; }

        [Option('f', "DoNotUseDifferentialDeployment",
            Default = false,
            SetName = "MirrorEnvironment",
            HelpText = "Do not use differential deployment")]
        public bool DoNotUseDifferentialDeployment { get; set; }

        [Option('u', "UpdateVariables",
             Default = false,
             SetName = "MirrorEnvironment",
             HelpText = "Update variables")]
        public bool UpdateVariables { get; set; }

        [Option('s', "HideDeploymentProgress",
            Default = false,
            SetName = "MirrorEnvironment",
            HelpText = "Hide deployment progress")]
        public bool HideDeploymentProgress { get; set; }

        [Option('p', "MaximumParallelDeployments",
            Default = 2,
            SetName = "MirrorEnvironment",
            HelpText = "Maximum parallel deployments")]
        public int MaximumParalleDeployments { get; set; }

        [Option("EnvironmentDeploymentPath",
            Default = "",
            SetName = "MirrorEnvironment",
            HelpText = "Environment Deployment path to save to.")]
        public string EnvironmentDeploymentPath { get; set; }

        [Option("Teamcity",
            Default = false,
            SetName = "MirrorEnvironment",
            HelpText = "Use Teamcity output")]
        public bool Teamcity { get; set; }
    }
}