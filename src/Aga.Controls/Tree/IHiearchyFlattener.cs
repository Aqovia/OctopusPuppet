using System.Collections;

namespace Aga.Controls.Tree
{
    public interface IHiearchyFlattener
    {
        /// <summary>
        /// Get list of children of the specified currentItem
        /// </summary>
        IEnumerable GetChildren(object currentItem, object dataSource);

        /// <summary>
        /// returns wheather specified currentItem has any children or not.
        /// </summary>
        bool HasChildren(object currentItem, object dataSource);
    }
}
