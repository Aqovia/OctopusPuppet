using System.Collections;
using System.Linq;
using Aga.Controls.Tree;
using OctopusPuppet.Scheduler;

namespace OctopusPuppet.Gui.Views
{
    public class EnvironmentDeploymentHiearchyFlattener : IHiearchyFlattener
    {
        public IEnumerable GetChildren(object currentItem, object dataSource)
        {
            if (currentItem == null && dataSource is EnvironmentDeployment)
            {
                return ((EnvironmentDeployment)dataSource).ProductDeployments;
            }
            if (currentItem is EnvironmentDeployment)
            {
                return ((EnvironmentDeployment)currentItem).ProductDeployments;
            }
            if (currentItem is ProductDeployment)
            {
                return ((ProductDeployment)currentItem).DeploymentSteps;
            }
            if (currentItem is ProductDeploymentStep)
            {
                return ((ProductDeploymentStep)currentItem).ComponentDeployments;
            }
            if (currentItem is ComponentDeployment)
            {
                return null;
            }
            throw new System.NotImplementedException();
        }

        public bool HasChildren(object currentItem, object dataSource)
        {
            if (currentItem == null && dataSource is EnvironmentDeployment)
            {
                return ((EnvironmentDeployment) dataSource).ProductDeployments.Any();
            }
            if (currentItem is EnvironmentDeployment)
            {
                return ((EnvironmentDeployment)currentItem).ProductDeployments.Any();
            }
            if (currentItem is ProductDeployment)
            {
                return ((ProductDeployment)currentItem).DeploymentSteps.Any();
            }
            if (currentItem is ProductDeploymentStep)
            {
                return ((ProductDeploymentStep)currentItem).ComponentDeployments.Any();
            }
            if (currentItem is ComponentDeployment)
            {
                return false;
            }
            throw new System.NotImplementedException();
        }
    }
}
