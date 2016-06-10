using CommandLine;

namespace OctopusPuppet.Cmd
{
    interface IOctopusOptions
    {
        [Option("OctopusUrl",
            Required = true,
            SetName = "OctopusUrl",
            HelpText = "The url to the octopus server e.g. 'http://octopus.test.com/'")]
        string OctopusUrl { get; set; }

        [Option("OctopusApiKey",
            Required = true,
            SetName = "OctopusApiKey",
            HelpText = "The api key for the octopus server e.g. 'API-HAAAS4MM6YBBSAIQVVHCQQUEA0'")]
        string OctopusApiKey { get; set; }
    }
}