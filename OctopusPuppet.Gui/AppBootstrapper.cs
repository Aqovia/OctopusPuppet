using System.Collections.Generic;
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
            var settings = new Dictionary<string, object>
               {
                   { "SizeToContent", SizeToContent.Manual },
                   { "Height" , 768  },
                   { "Width"  , 768 },
               };

            DisplayRootViewFor<DeploymentPlannerViewModel>(settings);
        }
    }
}
