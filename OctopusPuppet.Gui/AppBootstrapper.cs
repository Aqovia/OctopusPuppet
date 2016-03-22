using System.Windows;
using Caliburn.Micro;
using OctopusPuppet.Gui.ViewModels;

namespace OctopusPuppet.Gui
{
    public class AppBootstrapper : BootstrapperBase
    {
        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<DeploymentPlannerViewModel>();
        }
    }
}
