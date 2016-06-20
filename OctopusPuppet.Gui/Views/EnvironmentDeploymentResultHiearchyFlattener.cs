using System.Collections;
using System.Linq;
using Aga.Controls.Tree;
using OctopusPuppet.Gui.Model;

namespace OctopusPuppet.Gui.Views
{
    public class EnvironmentDeploymentResultHiearchyFlattener : IHiearchyFlattener
    {
        public IEnumerable GetChildren(object currentItem, object dataSource)
        {
            if (currentItem == null && dataSource is EnvironmentDeploymentResult)
            {
                return ((EnvironmentDeploymentResult)dataSource).ProductDeployments;
            }
            if (currentItem is EnvironmentDeploymentResult)
            {
                return ((EnvironmentDeploymentResult)currentItem).ProductDeployments;
            }
            if (currentItem is ProductDeploymentResult)
            {
                return ((ProductDeploymentResult)currentItem).DeploymentSteps;
            }
            if (currentItem is ProductDeploymentStepResult)
            {
                return ((ProductDeploymentStepResult)currentItem).ComponentDeployments;
            }
            if (currentItem is ComponentDeploymentResult)
            {
                return null;
            }
            throw new System.NotImplementedException();
        }

        public bool HasChildren(object currentItem, object dataSource)
        {
            if (currentItem == null && dataSource is EnvironmentDeploymentResult)
            {
                return ((EnvironmentDeploymentResult)dataSource).ProductDeployments.Any();
            }
            if (currentItem is EnvironmentDeploymentResult)
            {
                return ((EnvironmentDeploymentResult)currentItem).ProductDeployments.Any();
            }
            if (currentItem is ProductDeploymentResult)
            {
                return ((ProductDeploymentResult)currentItem).DeploymentSteps.Any();
            }
            if (currentItem is ProductDeploymentStepResult)
            {
                return ((ProductDeploymentStepResult)currentItem).ComponentDeployments.Any();
            }
            if (currentItem is ComponentDeploymentResult)
            {
                return false;
            }
            throw new System.NotImplementedException();
        }
    }
}
