using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json;
using OctopusPuppet.Deployer;
using OctopusPuppet.DeploymentPlanner;
using OctopusPuppet.OctopusProvider;
using OctopusPuppet.Scheduler;
using Environment = OctopusPuppet.DeploymentPlanner.Environment;

namespace OctopusPuppet.Cmd
{

    class Program
    {
        private static string CompanyName
        {
            get
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                var companyName = fileVersionInfo.CompanyName;
                if (string.IsNullOrEmpty(companyName))
                {
                    companyName = "Taliesin Sisson";
                }

                return companyName;
            }
        }

        private static string FileVersion
        {
            get
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                var fileVersion = fileVersionInfo.FileVersion;
                if (string.IsNullOrEmpty(fileVersion))
                {
                    fileVersion = "1.0.0.0";
                }

                return fileVersion;
            }
        }

        private static int GetCopyrightStartYear
        {
            get { return 2016; }
        }

        private static int GetCopyrightEndYear
        {
            get { return DateTime.Now.Year; }
        }

        private static ParserResult<object> ParsedResult { get; set; }

        public static int Main(string[] args)
        {
            var parser = new Parser(settings =>
            {
                settings.EnableDashDash = true;
                settings.CaseSensitive = true;
                settings.HelpWriter = null;
            });

            ParsedResult = parser.ParseArguments<BranchDeploymentOptions, MirrorEnvironmentOptions, RedploymentOptions, DeployOptions>(args);
            var exitCode = ParsedResult
                .MapResult<BranchDeploymentOptions, MirrorEnvironmentOptions, RedploymentOptions, DeployOptions, int>(
                    BranchDeployment,
                    MirrorEnvironment,
                    Redeployment,
                    Deploy,
     
                    CommandLineParsingError);

            return exitCode;
        }

        private static int BranchDeployment(BranchDeploymentOptions opts)
        {
            var deploymentPlanner = new OctopusDeploymentPlanner(opts.OctopusUrl, opts.OctopusApiKey);
            var componentFilter = GetComponentFilter(opts.ComponentFilterPath, opts.ComponentFilter);

            Console.WriteLine("Retrieve branch deployment plans for TargetEnvironment=\"{0}\" Branch=\"{1}\"", opts.TargetEnvironment, opts.Branch);
            var redeployDeploymentPlans = deploymentPlanner.GetBranchDeploymentPlans(opts.TargetEnvironment, opts.Branch, componentFilter);
            var environmentDeploymentPlan = redeployDeploymentPlans.EnvironmentDeploymentPlan;

            var deploymentScheduler = new DeploymentScheduler();
            var componentGraph = deploymentScheduler.GetComponentDeploymentGraph(environmentDeploymentPlan);
            var environmentDeployment = deploymentScheduler.GetEnvironmentDeployment(componentGraph);

            PrintEnvironmentDeploy(environmentDeployment);
            SaveEnvironmentDeploy(opts.EnvironmentDeploymentPath, environmentDeployment);

            if (opts.Deploy)
            {
                return Deploy(opts.OctopusUrl, opts.OctopusApiKey, opts.TargetEnvironment, environmentDeployment, opts.HideDeploymentProgress, opts.MaximumParalleDeployments);
            }

            return 0;
        }

        private static int MirrorEnvironment(MirrorEnvironmentOptions opts)
        {
            var deploymentPlanner = new OctopusDeploymentPlanner(opts.OctopusUrl, opts.OctopusApiKey);
            var componentFilter = GetComponentFilter(opts.ComponentFilterPath, opts.ComponentFilter);

            Console.WriteLine("Retrieve mirror environment plans for SourceEnvironment=\"{0}\" TargetEnvironment=\"{1}\"", opts.SourceEnvironment, opts.TargetEnvironment);
            var environmentMirrorDeploymentPlans = deploymentPlanner.GetEnvironmentMirrorDeploymentPlans(opts.SourceEnvironment, opts.TargetEnvironment, componentFilter);
            var environmentDeploymentPlan = environmentMirrorDeploymentPlans.EnvironmentDeploymentPlan;

            var deploymentScheduler = new DeploymentScheduler();
            var componentGraph = deploymentScheduler.GetComponentDeploymentGraph(environmentDeploymentPlan);
            var environmentDeployment = deploymentScheduler.GetEnvironmentDeployment(componentGraph);

            PrintEnvironmentDeploy(environmentDeployment);
            SaveEnvironmentDeploy(opts.EnvironmentDeploymentPath, environmentDeployment);

            if (opts.Deploy)
            {
                return Deploy(opts.OctopusUrl, opts.OctopusApiKey, opts.TargetEnvironment, environmentDeployment, opts.HideDeploymentProgress, opts.MaximumParalleDeployments);
            }

            return 0;
        }

        private static int Redeployment(RedploymentOptions opts)
        {
            var deploymentPlanner = new OctopusDeploymentPlanner(opts.OctopusUrl, opts.OctopusApiKey);
            var componentFilter = GetComponentFilter(opts.ComponentFilterPath, opts.ComponentFilter);

            Console.WriteLine("Retrieve mirror environment plans for TargetEnvironment=\"{0}\"", opts.TargetEnvironment);
            var redeployDeploymentPlans = deploymentPlanner.GetRedeployDeploymentPlans(opts.TargetEnvironment, componentFilter);
            var environmentDeploymentPlan = redeployDeploymentPlans.EnvironmentDeploymentPlan;

            var deploymentScheduler = new DeploymentScheduler();
            var componentGraph = deploymentScheduler.GetComponentDeploymentGraph(environmentDeploymentPlan);
            var environmentDeployment = deploymentScheduler.GetEnvironmentDeployment(componentGraph);

            PrintEnvironmentDeploy(environmentDeployment);
            SaveEnvironmentDeploy(opts.EnvironmentDeploymentPath, environmentDeployment);

            if (opts.Deploy)
            {
                return Deploy(opts.OctopusUrl, opts.OctopusApiKey, opts.TargetEnvironment, environmentDeployment, opts.HideDeploymentProgress, opts.MaximumParalleDeployments);
            }

            return 0;
        }

        private static int Deploy(DeployOptions opts)
        {
            var environmentDeployment = LoadEnvironmentDeploy(opts.EnvironmentDeploymentPath);
            return Deploy(opts.OctopusUrl, opts.OctopusApiKey, opts.TargetEnvironment, environmentDeployment, opts.HideDeploymentProgress, opts.MaximumParallelDeployments);
        }

        private static int CommandLineParsingError(IEnumerable<Error> errors)
        {
            var firstError = errors.FirstOrDefault();
            if (firstError is VersionRequestedError)
            {
                Console.WriteLine(FileVersion);
                return 0;
            }

            var helpText = GetHelpText(ParsedResult);

            Console.WriteLine(helpText);

            if (firstError is HelpRequestedError)
            {
                return 0;
            }

            return -1;
        }

        private static void PrintEnvironmentDeploy(EnvironmentDeployment environmentDeployment)
        {
            var environmentDeploymentJson = JsonConvert.SerializeObject(environmentDeployment, new JsonSerializerSettings { Formatting = Formatting.Indented });
            Console.WriteLine(environmentDeploymentJson);
        }

        private static EnvironmentDeployment LoadEnvironmentDeploy(string path)
        {
            var json = File.ReadAllText(path);
            var environmentDeployment = JsonConvert.DeserializeObject<EnvironmentDeployment>(json);

            return environmentDeployment;
        }

        private static void SaveEnvironmentDeploy(string path, EnvironmentDeployment environmentDeployment)
        {
            if (string.IsNullOrEmpty(path)) return;

            var environmentDeploymentJson = JsonConvert.SerializeObject(environmentDeployment, new JsonSerializerSettings {Formatting = Formatting.Indented});
            File.WriteAllText(path, environmentDeploymentJson);
        }

        private static int Deploy(string url, string apiKey, string targetEnvironment, EnvironmentDeployment environmentDeployment, bool hideDeploymentProgress, int maximumParalleDeployments)
        {
            var environment = new Environment
            {
                Id = targetEnvironment,
                Name = targetEnvironment
            };

            var progress = hideDeploymentProgress ? null : new ConsoleDeployProgress();
            var componentVertexDeployer = new OctopusComponentVertexDeployer(url, apiKey, environment);
            var cancellationTokenSource = new CancellationTokenSource();
            var deploymentExecutor = new DeploymentExecutor(componentVertexDeployer, environmentDeployment, cancellationTokenSource.Token, progress, maximumParalleDeployments);
            var allDeploymentsSucceded = deploymentExecutor.Execute().ConfigureAwait(false).GetAwaiter().GetResult();

            return allDeploymentsSucceded ? 0 : 1;
        }

        private static HelpText GetHelpText(ParserResult<object> parserResult)
        {
            var headingInfo = new HeadingInfo("OctopusPuppet.cmd", FileVersion);
            var copyRightInfo = new CopyrightInfo(true, CompanyName, GetCopyrightStartYear, GetCopyrightEndYear);

            var helpText = new HelpText(headingInfo, copyRightInfo)
            {
                AddEnumValuesToHelpText = true
            }
                .AddOptions(parserResult)
                .AddVerbs(typeof(BranchDeploymentOptions), typeof(MirrorEnvironmentOptions), typeof(RedploymentOptions), typeof(DeployOptions))
                .AddPostOptionsLine("EXAMPLE USAGE: ")
                .AddPostOptionsLine("  OctopusPuppet.Cmd BranchDeployment")
                .AddPostOptionsLine("    --OctopusUrl \"http://octopus.test.com/\"")
                .AddPostOptionsLine("    --OctopusApiKey \"API-HAAAS4MM6YBBSAIQVVHCQQUEA0\"")
                .AddPostOptionsLine("    --TargetEnvironment \"Development\"")
                .AddPostOptionsLine("    --Branch \"Master\"")
                .AddPostOptionsLine("    [--ComponentFilterPath \"componentFilter.json\"]")
                .AddPostOptionsLine("    [--ComponentFilter \"Component filter json base64 encoded\"]")
                .AddPostOptionsLine("    [--Deploy]")
                .AddPostOptionsLine("    [--EnvironmentDeploymentPath \"environmentDeployment.json\"]")
                .AddPostOptionsLine("    [--MaximumParallelDeployments 4]")
                .AddPostOptionsLine("    [--HideDeploymentProgress]")
                .AddPostOptionsLine("")
                .AddPostOptionsLine("  OctopusPuppet.Cmd MirrorEnvironment")
                .AddPostOptionsLine("    --OctopusUrl \"http://octopus.test.com/\"")
                .AddPostOptionsLine("    --OctopusApiKey \"API-HAAAS4MM6YBBSAIQVVHCQQUEA0\"")
                .AddPostOptionsLine("    --SourceEnvironment \"Development\"")
                .AddPostOptionsLine("    --TargetEnvironment \"Test\"")
                .AddPostOptionsLine("    [--ComponentFilterPath \"componentFilter.json\"]")
                .AddPostOptionsLine("    [--ComponentFilter \"Component filter json base64 encoded\"]")
                .AddPostOptionsLine("    [--Deploy]")
                .AddPostOptionsLine("    [--EnvironmentDeploymentPath \"environmentDeployment.json\"]")
                .AddPostOptionsLine("    [--MaximumParallelDeployments 4]")
                .AddPostOptionsLine("    [--HideDeploymentProgress]")
                .AddPostOptionsLine("")
                .AddPostOptionsLine("  OctopusPuppet.Cmd Redeployment")
                .AddPostOptionsLine("    --OctopusUrl \"http://octopus.test.com/\"")
                .AddPostOptionsLine("    --OctopusApiKey \"API-HAAAS4MM6YBBSAIQVVHCQQUEA0\"")
                .AddPostOptionsLine("    --TargetEnvironment \"Development\"")
                .AddPostOptionsLine("    [--ComponentFilterPath \"componentFilter.json\"]")
                .AddPostOptionsLine("    [--ComponentFilter \"Component filter json base64 encoded\"]")
                .AddPostOptionsLine("    [--Deploy]")
                .AddPostOptionsLine("    [--EnvironmentDeploymentPath \"environmentDeployment.json\"]")
                .AddPostOptionsLine("    [--MaximumParallelDeployments 4]")
                .AddPostOptionsLine("    [--HideDeploymentProgress]")
                .AddPostOptionsLine("")
                .AddPostOptionsLine("  OctopusPuppet.Cmd Deploy")
                .AddPostOptionsLine("    --OctopusUrl \"http://octopus.test.com/\"")
                .AddPostOptionsLine("    --OctopusApiKey \"API-HAAAS4MM6YBBSAIQVVHCQQUEA0\"")
                .AddPostOptionsLine("    --EnvironmentDeploymentPath \"environmentDeployment.json\"")
                .AddPostOptionsLine("    --TargetEnvironment \"Development\"")
                .AddPostOptionsLine("    [--MaximumParallelDeployments 4]")
                .AddPostOptionsLine("    [--HideDeploymentProgress]")
                .AddPostOptionsLine("");

            HelpText.DefaultParsingErrorsHandler(parserResult, helpText);

            return helpText;
        }

        private static ComponentFilter GetComponentFilter(string componentFilterPath, string defaultComponentFilterJsonBase64Encoded)
        {
            if (string.IsNullOrEmpty(componentFilterPath))
            {
                if (!string.IsNullOrEmpty(defaultComponentFilterJsonBase64Encoded))
                {
                    var componentFilterJson = Encoding.Unicode.GetString(Convert.FromBase64String(defaultComponentFilterJsonBase64Encoded));
                    return JsonConvert.DeserializeObject<ComponentFilter>(componentFilterJson);
                }

                if (!File.Exists("filter.json"))
                {
                    return null;
                }

                componentFilterPath = "filter.json";
            }

            var json = File.ReadAllText(componentFilterPath);
            var componentFilter = JsonConvert.DeserializeObject<ComponentFilter>(json);
            return componentFilter;
        }
    }
}
