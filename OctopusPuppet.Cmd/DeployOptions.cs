﻿using CommandLine;

namespace OctopusPuppet.Cmd
{
    [Verb("Deploy", HelpText = "Deploy to environment.")]
    class DeployOptions
    {
        [Option("OctopusUrl",
            Required = true,
            SetName = "Deploy",
            HelpText = "The url to the octopus server e.g. 'http://octopus.test.com/'")]
        public string OctopusUrl { get; set; }

        [Option("OctopusApiKey",
            Required = true,
            SetName = "Deploy",
            HelpText = "The api key for the octopus server e.g. 'API-HAAAS4MM6YBBSAIQVVHCQQUEA0'")]
        public string OctopusApiKey { get; set; }

        [Option("EnvironmentDeploymentPath",
            Required = false,
            Default = "",
            SetName = "Deploy",
            HelpText = "Environment Deployment path to save to.")]
        public string EnvironmentDeploymentPath { get; set; }

        [Option("TargetEnvironment",
            Required = true,
            SetName = "Deploy",
            HelpText = "The environment to deploy to.")]
        public string TargetEnvironment { get; set; }
    }
}
