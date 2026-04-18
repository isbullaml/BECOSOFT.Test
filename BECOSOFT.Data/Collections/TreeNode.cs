using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BECOSOFT.Data.Collections {
    /// <inheritdoc />
    [DebuggerDisplay("{Value} - {Level}")]
    public class TreeNode<T> : ITreeNode<T> {
        /// <inheritdoc />
        public T Value { get; }
        /// <inheritdoc />
        public TreeNode<T> Parent { get; private set; }
        /// <inheritdoc />
        public List<TreeNode<T>> Children { get; }

        private TreeNode<T> HighestParent => GetHighestParent();

        /// <inheritdoc />
        public int Level {
            get {
                var level = 0;
                var nodeToCheck = this;
                while (nodeToCheck.Parent != null) {
                    level += 1;
                    nodeToCheck = nodeToCheck.Parent;
                }
                return level;
            }
        }

        public TreeNode(T value) {
            Value = value;
            Children = new List<TreeNode<T>>();
        }

        /// <inheritdoc />
        public TreeNode<T> AddChild(T child) {
            var childNode = new TreeNode<T>(child) { Parent = this };
            Children.Add(childNode);
            return childNode;
        }

        /// <inheritdoc />
        public IEnumerator<ITreeNode<T>> GetEnumerator() {
            return Children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public int GetLevelCount(int level) {
            return HighestParent.Flatten().Count(x => x.Level == level);
        }

        private TreeNode<T> GetHighestParent() {
            var highestParent = this;
            while (highestParent.Level != 0) {
                highestParent = highestParent.Parent;
            }
            return highestParent;
        }

        public IEnumerable<TreeNode<T>> Flatten() {
            yield return this;

            foreach (var node in Children.SelectMany(child => child.Flatten())) {
                yield return node;
            }
        }
    }
}
