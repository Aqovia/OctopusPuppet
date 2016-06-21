using System.Collections;

namespace Aga.Controls.Tree
{
    public class TreeModelHiearchyFlattener : IHiearchyFlattener
    {
        public IEnumerable GetChildren(object currentItem, object dataSource)
        {
            var treeModel = dataSource as ITreeModel;
            if (treeModel == null)
            {
                return null;
            }
            return treeModel.GetChildren(currentItem);
        }

        public bool HasChildren(object currentItem, object dataSource)
        {
            var treeModel = dataSource as ITreeModel;
            if (treeModel == null)
            {
                return false;
            }
            return treeModel.HasChildren(currentItem);
        }
    }
}
