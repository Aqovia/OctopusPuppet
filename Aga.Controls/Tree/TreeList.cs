using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Controls.Primitives;

namespace Aga.Controls.Tree
{
    public class TreeList : ListView
    {
        #region Properties

        /// <summary>
        /// Internal collection of rows representing visible nodes, actually displayed in the ListView
        /// </summary>
        internal ObservableCollectionAdv<TreeNode> Rows
        {
            get;
            private set;
        }

        private IHiearchyFlattener _hiearchyFlattener = new TreeModelHiearchyFlattener();
        public IHiearchyFlattener HiearchyFlattener
        {
            get { return _hiearchyFlattener; }
            set
            {
                if (_hiearchyFlattener == value) return;

                _hiearchyFlattener = value;
                FlatternHiearchy();
            }
        }

        /// <summary> 
        /// View DependencyProperty
        /// </summary> 
        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register(
                "Model",
                typeof(object),
                typeof(TreeList),
                new PropertyMetadata(
                    new PropertyChangedCallback(OnModelChanged))
                );

        private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var treeList = (TreeList)d;

            object oldView = (object)e.OldValue;
            object newView = (object)e.NewValue;
            if (oldView != newView)
            {
                treeList.FlatternHiearchy();
            }
        }

        internal void FlatternHiearchy()
        {
            Root.Children.Clear();
            Rows.Clear();
            CreateChildrenNodes(Root);
        }

        public object Model
        {
            get { return (object)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        private TreeNode _root;
        internal TreeNode Root
        {
            get { return _root; }
        }

        public ReadOnlyCollection<TreeNode> Nodes
        {
            get { return Root.Nodes; }
        }

        internal TreeNode PendingFocusNode
        {
            get;
            set;
        }

        public ICollection<TreeNode> SelectedNodes
        {
            get
            {
                return SelectedItems.Cast<TreeNode>().ToArray();
            }
        }

        public TreeNode SelectedNode
        {
            get
            {
                if (SelectedItems.Count > 0)
                    return SelectedItems[0] as TreeNode;
                else
                    return null;
            }
        }
        #endregion

        public TreeList()
        {
            Rows = new ObservableCollectionAdv<TreeNode>();
            _root = new TreeNode(this, null);
            _root.IsExpanded = true;
            ItemsSource = Rows;
            ItemContainerGenerator.StatusChanged += ItemContainerGeneratorStatusChanged;
        }

        void ItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated && PendingFocusNode != null)
            {
                var item = ItemContainerGenerator.ContainerFromItem(PendingFocusNode) as TreeListItem;
                if (item != null)
                    item.Focus();
                PendingFocusNode = null;
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var ti = element as TreeListItem;
            var node = item as TreeNode;
            if (ti != null && node != null)
            {
                ti.Node = item as TreeNode;
                base.PrepareContainerForItemOverride(element, node.Tag);
            }
        }

        internal void SetIsExpanded(TreeNode node, bool value)
        {
            if (value)
            {
                if (!node.IsExpandedOnce)
                {
                    node.IsExpandedOnce = true;
                    node.AssignIsExpanded(value);
                    CreateChildrenNodes(node);
                }
                else
                {
                    node.AssignIsExpanded(value);
                    CreateChildrenRows(node);
                }
            }
            else
            {
                DropChildrenRows(node, false);
                node.AssignIsExpanded(value);
            }
        }

        internal void CreateChildrenNodes(TreeNode node)
        {
            var children = GetChildren(node);
            if (children == null) return;

            var rowIndex = Rows.IndexOf(node);
            node.ChildrenSource = children as INotifyCollectionChanged;

            foreach (object obj in children)
            {
                var child = new TreeNode(this, obj);
                var hasGrandChildren = HasChildren(child);
                child.HasChildren = hasGrandChildren;
                node.Children.Add(child);
            }
            Rows.InsertRange(rowIndex + 1, node.Children.ToArray());
        }

        private void CreateChildrenRows(TreeNode node)
        {
            int index = Rows.IndexOf(node);
            if (index >= 0 || node == _root) // ignore invisible nodes
            {
                var nodes = node.AllVisibleChildren.ToArray();
                Rows.InsertRange(index + 1, nodes);
            }
        }

        internal void DropChildrenRows(TreeNode node, bool removeParent)
        {
            int start = Rows.IndexOf(node);
            if (start >= 0 || node == _root) // ignore invisible nodes
            {
                int count = node.VisibleChildrenCount;
                if (removeParent)
                    count++;
                else
                    start++;
                Rows.RemoveRange(start, count);
            }
        }

        private IEnumerable GetChildren(TreeNode parent)
        {
            if (Model != null)
                return HiearchyFlattener.GetChildren(parent.Tag, Model);
            else
                return null;
        }

        private bool HasChildren(TreeNode parent)
        {
            if (parent == Root)
                return true;
            else if (Model != null)
                return HiearchyFlattener.HasChildren(parent.Tag, Model);
            else
                return false;
        }

        internal void InsertNewNode(TreeNode parent, object tag, int rowIndex, int index)
        {
            TreeNode node = new TreeNode(this, tag);
            if (index >= 0 && index < parent.Children.Count)
                parent.Children.Insert(index, node);
            else
            {
                index = parent.Children.Count;
                parent.Children.Add(node);
            }
            Rows.Insert(rowIndex + index + 1, node);
        }
    }
}
