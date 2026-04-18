using System.Collections.Generic;

namespace BECOSOFT.Data.Collections {
    /// <summary>
    /// A treenode
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    public interface ITreeNode<T> : IEnumerable<ITreeNode<T>> {
        /// <summary>
        /// The value of this treenode
        /// </summary>
        T Value { get; }
        /// <summary>
        /// The parent of this treenode
        /// </summary>
        TreeNode<T> Parent { get; }
        /// <summary>
        /// The children of this treenode
        /// </summary>
        List<TreeNode<T>> Children { get; }
        /// <summary>
        /// The level of this treenode
        /// </summary>
        int Level { get; }
        /// <summary>
        /// Add a child
        /// </summary>
        /// <param name="child">The child to add</param>
        /// <returns>The new child treenode</returns>
        TreeNode<T> AddChild(T child);
        /// <summary>
        /// Get the index of the last element of a level
        /// </summary>
        /// <param name="level">The level to search</param>
        /// <returns>The last index</returns>
        int GetLevelCount(int level);
        /// <summary>
        /// Flattens the tree.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TreeNode<T>> Flatten();
    }
}