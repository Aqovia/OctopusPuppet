﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CommandLine;
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
        public static int Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<BranchDeploymentOptions, MirrorEnvironmentOptions, RedploymentOptions, DeployOptions>(args);
            var exitCode = result
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
            var componentFilter = GetComponentFilter(opts.ComponentFilterPath);

            Console.WriteLine("Retrieve branch deployment plans for TargetEnvironment={0} Branch={1}", opts.TargetEnvironment, opts.Branch);
            var redeployDeploymentPlans = deploymentPlanner.GetBranchDeploymentPlans(opts.TargetEnvironment, opts.Branch, componentFilter);
            var environmentDeploymentPlan = redeployDeploymentPlans.EnvironmentDeploymentPlan;

            var deploymentScheduler = new DeploymentScheduler();
            var componentGraph = deploymentScheduler.GetComponentDeploymentGraph(environmentDeploymentPlan);
            var environmentDeployment = deploymentScheduler.GetEnvironmentDeployment(componentGraph);

            PrintEnvironmentDeploy(environmentDeployment);

            if (!string.IsNullOrEmpty(opts.EnvironmentDeploymentPath))
            {
                SaveEnvironmentDeploy(opts.EnvironmentDeploymentPath, environmentDeployment);
            }

            if (opts.Deploy)
            {
                Deploy(opts.OctopusUrl, opts.OctopusApiKey, opts.TargetEnvironment, environmentDeployment);
            }

            return 0;
        }

        private static int MirrorEnvironment(MirrorEnvironmentOptions opts)
        {
            var deploymentPlanner = new OctopusDeploymentPlanner(opts.OctopusUrl, opts.OctopusApiKey);
            var componentFilter = GetComponentFilter(opts.ComponentFilterPath);

            Console.WriteLine("Retrieve mirror environment plans for SourceEnvironment={0} TargetEnvironment={1}", opts.SourceEnvironment, opts.TargetEnvironment);
            var environmentMirrorDeploymentPlans = deploymentPlanner.GetEnvironmentMirrorDeploymentPlans(opts.SourceEnvironment, opts.TargetEnvironment, componentFilter);
            var environmentDeploymentPlan = environmentMirrorDeploymentPlans.EnvironmentDeploymentPlan;

            var deploymentScheduler = new DeploymentScheduler();
            var componentGraph = deploymentScheduler.GetComponentDeploymentGraph(environmentDeploymentPlan);
            var environmentDeployment = deploymentScheduler.GetEnvironmentDeployment(componentGraph);

            PrintEnvironmentDeploy(environmentDeployment);

            if (!string.IsNullOrEmpty(opts.EnvironmentDeploymentPath))
            {
                SaveEnvironmentDeploy(opts.EnvironmentDeploymentPath, environmentDeployment);
            }

            if (opts.Deploy)
            {
                Deploy(opts.OctopusUrl, opts.OctopusApiKey, opts.TargetEnvironment, environmentDeployment);
            }

            return 0;
        }

        private static int Redeployment(RedploymentOptions opts)
        {
            var deploymentPlanner = new OctopusDeploymentPlanner(opts.OctopusUrl, opts.OctopusApiKey);
            var componentFilter = GetComponentFilter(opts.ComponentFilterPath);

            Console.WriteLine("Retrieve mirror environment plans for TargetEnvironment={0}", opts.TargetEnvironment);
            var redeployDeploymentPlans = deploymentPlanner.GetRedeployDeploymentPlans(opts.TargetEnvironment, componentFilter);
            var environmentDeploymentPlan = redeployDeploymentPlans.EnvironmentDeploymentPlan;

            var deploymentScheduler = new DeploymentScheduler();
            var componentGraph = deploymentScheduler.GetComponentDeploymentGraph(environmentDeploymentPlan);
            var environmentDeployment = deploymentScheduler.GetEnvironmentDeployment(componentGraph);

            PrintEnvironmentDeploy(environmentDeployment);

            if (!string.IsNullOrEmpty(opts.EnvironmentDeploymentPath))
            {
                SaveEnvironmentDeploy(opts.EnvironmentDeploymentPath, environmentDeployment);
            }

            if (opts.Deploy)
            {
                Deploy(opts.OctopusUrl, opts.OctopusApiKey, opts.TargetEnvironment, environmentDeployment);
            }

            return 0;
        }

        private static int Deploy(DeployOptions opts)
        {
            var environmentDeployment = LoadEnvironmentDeploy(opts.EnvironmentDeploymentPath);
            return Deploy(opts.OctopusUrl, opts.OctopusApiKey, opts.TargetEnvironment, environmentDeployment);
        }

        private static void PrintEnvironmentDeploy(EnvironmentDeployment environmentDeployment)
        {
            var environmentDeploymentJson = JsonConvert.SerializeObject(environmentDeployment, new JsonSerializerSettings() { Formatting = Formatting.Indented });
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
            var environmentDeploymentJson = JsonConvert.SerializeObject(environmentDeployment, new JsonSerializerSettings() { Formatting = Formatting.Indented });
            File.WriteAllText(path, environmentDeploymentJson);
        }

        private static int Deploy(string url, string apiKey, string targetEnvironment, EnvironmentDeployment environmentDeployment)
        {
            var environment = new Environment
            {
                Id = targetEnvironment,
                Name = targetEnvironment
            };

            var componentVertexDeployer = new OctopusComponentVertexDeployer(url, apiKey, environment);
            var cancellationTokenSource = new CancellationTokenSource();
            var deploymentExecutor = new DeploymentExecutor(componentVertexDeployer, environmentDeployment, cancellationTokenSource.Token, null);
            deploymentExecutor.Execute().ConfigureAwait(false).GetAwaiter().GetResult();

            return 0;
        }

        private static int CommandLineParsingError(IEnumerable<Error> errors)
        {
            return -1;
        }

        private static ComponentFilter GetComponentFilter(string componentFilterPath)
        {
            var json = File.ReadAllText(componentFilterPath);
            var componentFilter = JsonConvert.DeserializeObject<ComponentFilter>(json);
            return componentFilter;
        }
    }
}
