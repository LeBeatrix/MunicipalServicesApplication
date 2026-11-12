using System;
using System.Collections.Generic;

namespace MunicipalServicesApplication.DataStructures
{
    // Ordinary n-ary tree: used for municipality -> category -> request hierarchy.
    public sealed class TreeNode<T>
    {
        private readonly List<TreeNode<T>> children = new List<TreeNode<T>>();
        public T Value { get; private set; }
        public IReadOnlyList<TreeNode<T>> Children { get { return children.AsReadOnly(); } }
        public TreeNode(T value) { Value = value; }
        public TreeNode<T> Add(T value) { var child = new TreeNode<T>(value); children.Add(child); return child; }
        public IEnumerable<T> PreOrder()
        {
            yield return Value;
            foreach (var child in children)
                foreach (var value in child.PreOrder()) yield return value;
        }
    }

    // Binary decision tree has no ordering assumption; leaves classify requests.
    public sealed class BinaryNode<T>
    {
        public T Value { get; private set; }
        public BinaryNode<T> Left { get; private set; }
        public BinaryNode<T> Right { get; private set; }
        public BinaryNode(T value, BinaryNode<T> left = null, BinaryNode<T> right = null)
        { Value = value; Left = left; Right = right; }
        public IEnumerable<T> PreOrder()
        {
            yield return Value;
            if (Left != null) foreach (var value in Left.PreOrder()) yield return value;
            if (Right != null) foreach (var value in Right.PreOrder()) yield return value;
        }
    }
}
