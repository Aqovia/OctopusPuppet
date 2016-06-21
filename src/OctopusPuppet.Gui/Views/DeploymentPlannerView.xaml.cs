using System.Windows.Controls;

namespace OctopusPuppet.Gui.Views
{
    /// <summary>
    /// Interaction logic for DeploymentPlannerView.xaml
    /// </summary>
    public partial class DeploymentPlannerView : UserControl
    {
        public DeploymentPlannerView()
        {
            InitializeComponent();
            EnvironmentDeploymentTreeList.HiearchyFlattener = new EnvironmentDeploymentHiearchyFlattener();
            EnvironmentDeploymentResultTreeList.HiearchyFlattener = new EnvironmentDeploymentResultHiearchyFlattener();
        }
    }
}
