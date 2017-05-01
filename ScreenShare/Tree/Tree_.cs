using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;

namespace ScreenCapture
{
    class Tree<T>
    {
        private class TreeNode<T>
        {
            public T Item { get; private set; }
            public int Depth { get; private set; }
            public int BranchNumber { get; private set; }

            private TreeNode<T> _parent;
            public TreeNode<T> Parent 
            { 
                get
                {
                    return _parent;
                }
                private set
                {
                    _parent = value;
                    Depth = Parent.Depth == null ? 0 : Parent.Depth + 1;
                }
            }
            public List<TreeNode<T>> Childs = new List<TreeNode<T>>();

            public TreeNode(T item, TreeNode<T> parent = null)
            {
                Item = item;
                Parent = parent;
            }

            public void Add(TreeNode<T> item)
            {
                item.Parent = this;
                item.BranchNumber = Childs.Count;
                Childs.Add(item);
            }

            public bool IsNodeOf(T item)
            {
                return Item.Equals(item);
            }

            public bool IsNodeOf(Predicate<T> match)
            {
                return match(this.Item);
            }
        }

        public List<T> NodeList = new List<T>();
        private TreeNode<T> RootNode;
        private TreeNode<T> NextChainNode;

        private int BranchCount = 2;
        public T Root 
        {
            get
            {
                return RootNode.Item;
            }
        }

        public Tree(int branchCount = 2, T root = default(T))
        {
            RootNode = new TreeNode<T>(root);
            NextChainNode = RootNode;
            BranchCount = branchCount;

            NodeList.Add(root);
        }

        public Tree(T[] items, int branchCount = 2, T root = default(T))
            : this(branchCount, root)
        {
            NextChainNode = RootNode;

            for (int i = 0; i < items.Length; i++)
            {
                Add(items[i]);
            }
        }

        public void Add(T item)
        {
            if (NextChainNode.Childs.Count == BranchCount)
            {
                var p = NextChainNode;
                while (p.Depth != 0 && p.BranchNumber == BranchCount - 1)
                {
                    p = p.Parent;
                }

                if (p.Depth == 0)
                {
                    while (p.Childs.Count != 0)
                    {
                        NextChainNode = p.Childs[0];
                    }

                }
                else
                {
                    NextChainNode = p.Parent;
                }
            }

            NextChainNode.Add(new TreeNode<T>(item));
            NodeList.Add(item);
        }

        private TreeNode<T>[] GetDepthOf(int depth)
        {
            var list_tmp = new List<TreeNode<T>>(new TreeNode<T>[] { RootNode, });
            var list_d = new List<TreeNode<T>>();

            if (depth == 0)
            {
                return list_tmp.ToArray();
            }

            for (int d = 1; d <= depth; d++)
            {
                list_d.Clear();

                foreach (var n in list_tmp)
                {
                    list_d.AddRange(n.Childs);
                }

                list_tmp = new List<TreeNode<T>>(list_d);
            }

            return list_d.ToArray();
        }

        private TreeNode<T> FindNodeIn(TreeNode<T> node, T item)
        {
            if (node.IsNodeOf(item))
            {
                return node;
            }

            foreach (var child in node.Childs)
            {
                return FindNodeIn(child, item);
            }

            return default(TreeNode<T>);
        }

        private TreeNode<T> FindNodeIn(TreeNode<T> node, Predicate<T> match)
        {
            if (node.IsNodeOf(match))
            {
                return node;
            }

            foreach(var child in node.Childs)
            {
                return FindNodeIn(child, match);
            }

            return default(TreeNode<T>);
        }
    }
}