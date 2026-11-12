using System;
using System.Collections.Generic;

namespace MunicipalServicesApplication.DataStructures
{
    // Insert/search-only ordered set. Duplicate keys are ignored by the comparer.
    public class BinarySearchTree<T>
    {
        protected sealed class Node
        {
            internal T Value;
            internal Node Left, Right;
            internal int Height = 1;
            internal bool Red = true;
            internal Node(T value) { Value = value; }
        }
        protected Node Root;
        protected readonly IComparer<T> Comparer;
        public int Count { get; private set; }
        public int Height { get { return GetHeight(Root); } }
        public BinarySearchTree(IComparer<T> comparer = null) { Comparer = comparer ?? Comparer<T>.Default; }
        public bool Insert(T value)
        {
            bool added = false;
            Root = InsertNode(Root, value, ref added);
            if (Root != null) Root.Red = false;
            if (added) Count++;
            return added;
        }
        private Node InsertNode(Node node, T value, ref bool added)
        {
            if (node == null) { added = true; return new Node(value); }
            int order = Comparer.Compare(value, node.Value);
            if (order < 0) node.Left = InsertNode(node.Left, value, ref added);
            else if (order > 0) node.Right = InsertNode(node.Right, value, ref added);
            else return node;
            return Balance(node);
        }
        protected virtual Node Balance(Node node) { UpdateHeight(node); return node; }
        protected static int GetHeight(Node node) { return node == null ? 0 : node.Height; }
        protected static void UpdateHeight(Node node) { node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right)); }
        protected static Node RotateLeft(Node node)
        {
            Node next = node.Right;
            node.Right = next.Left;
            next.Left = node;
            UpdateHeight(node); UpdateHeight(next);
            return next;
        }
        protected static Node RotateRight(Node node)
        {
            Node next = node.Left;
            node.Left = next.Right;
            next.Right = node;
            UpdateHeight(node); UpdateHeight(next);
            return next;
        }
        public bool TryGet(T key, out T value)
        {
            Node node = Root;
            while (node != null)
            {
                int order = Comparer.Compare(key, node.Value);
                if (order == 0) { value = node.Value; return true; }
                node = order < 0 ? node.Left : node.Right;
            }
            value = default(T);
            return false;
        }
        public IEnumerable<T> InOrder() { return Traverse(Root); }
        private static IEnumerable<T> Traverse(Node node)
        {
            if (node == null) yield break;
            foreach (var item in Traverse(node.Left)) yield return item;
            yield return node.Value;
            foreach (var item in Traverse(node.Right)) yield return item;
        }
        public virtual bool Validate()
        {
            bool first = true;
            T previous = default(T);
            int count = 0;
            foreach (var item in InOrder())
            {
                if (!first && Comparer.Compare(previous, item) >= 0) return false;
                previous = item; first = false; count++;
            }
            return count == Count;
        }
    }

    public sealed class AvlTree<T> : BinarySearchTree<T>
    {
        public AvlTree(IComparer<T> comparer = null) : base(comparer) { }
        protected override Node Balance(Node node)
        {
            UpdateHeight(node);
            int factor = GetHeight(node.Left) - GetHeight(node.Right);
            if (factor > 1)
            {
                if (GetHeight(node.Left.Left) < GetHeight(node.Left.Right)) node.Left = RotateLeft(node.Left);
                return RotateRight(node);
            }
            if (factor < -1)
            {
                if (GetHeight(node.Right.Right) < GetHeight(node.Right.Left)) node.Right = RotateRight(node.Right);
                return RotateLeft(node);
            }
            return node;
        }
        public override bool Validate() { return base.Validate() && ValidateHeight(Root) >= 0; }
        private static int ValidateHeight(Node node)
        {
            if (node == null) return 0;
            int left = ValidateHeight(node.Left), right = ValidateHeight(node.Right);
            if (left < 0 || right < 0 || Math.Abs(left - right) > 1 || node.Height != 1 + Math.Max(left, right)) return -1;
            return 1 + Math.Max(left, right);
        }
    }

    // Left-leaning red-black tree: rotate red links left, split temporary 4-nodes,
    // and keep equal black heights. No deletion API is claimed.
    public sealed class RedBlackTree<T> : BinarySearchTree<T>
    {
        public RedBlackTree(IComparer<T> comparer = null) : base(comparer) { }
        private static bool IsRed(Node node) { return node != null && node.Red; }
        protected override Node Balance(Node node)
        {
            if (IsRed(node.Right) && !IsRed(node.Left))
            {
                bool color = node.Red;
                node = RotateLeft(node);
                node.Red = color; node.Left.Red = true;
            }
            if (IsRed(node.Left) && IsRed(node.Left.Left))
            {
                bool color = node.Red;
                node = RotateRight(node);
                node.Red = color; node.Right.Red = true;
            }
            if (IsRed(node.Left) && IsRed(node.Right))
            {
                node.Red = !node.Red;
                node.Left.Red = !node.Left.Red;
                node.Right.Red = !node.Right.Red;
            }
            UpdateHeight(node);
            return node;
        }
        public override bool Validate() { return base.Validate() && !IsRed(Root) && BlackHeight(Root) >= 0; }
        private static int BlackHeight(Node node)
        {
            if (node == null) return 1;
            if (IsRed(node.Right) || (IsRed(node) && (IsRed(node.Left) || IsRed(node.Right)))) return -1;
            int left = BlackHeight(node.Left), right = BlackHeight(node.Right);
            if (left < 0 || right < 0 || left != right) return -1;
            return left + (node.Red ? 0 : 1);
        }
    }
}
