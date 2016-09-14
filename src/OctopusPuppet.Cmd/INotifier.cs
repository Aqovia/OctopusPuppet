using System;
using CommandLine.Text;
using OctopusPuppet.Deployer;
using OctopusPuppet.Scheduler;

namespace OctopusPuppet.Cmd
{
    public interface INotifier : IProgress<ComponentVertexDeploymentProgress>
    {
        void PrintHelp(HelpText helpText);
        void PrintVersion(string version);
        void PrintActionMessage(string message);
        void PrintEnvironmentDeploy(EnvironmentDeployment environmentDeployment);
    }
}
