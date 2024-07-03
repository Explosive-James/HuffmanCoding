
namespace HuffmanCoding.Collections
{
    /// <summary>
    /// A node for the Huffman tree to store the character and it's frequency
    /// or to store the children nodes to form the rest of the tree's structure.
    /// </summary>
    internal sealed class TreeNode
    {
        #region Data
        /// <summary>
        /// The character this node represents, default if it's not an element node.
        /// </summary>
        public readonly char element;
        /// <summary>
        /// The frequency the character element appeared in the input text.
        /// </summary>
        public readonly long frequency;

        /// <summary>
        /// The child node this node connects to when searching down the tree.
        /// </summary>
        public readonly TreeNode? left, right;
        /// <summary>
        /// The parent node this node connects to when searching down the tree.
        /// </summary>
        public TreeNode? parent;
        #endregion

        #region Properties
        /// <summary>
        /// Is this node the root node for the tree, does it have no parent node.
        /// </summary>
        public bool IsRoot => parent == null;

        /// <summary>
        /// Is this node storing a character / element and has no other children.
        /// </summary>
        public bool IsElement { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance as an element node, containing no children.
        /// </summary>
        /// <param name="element">The character this node should represent.</param>
        /// <param name="frequency">The frequency the character appeared in the input text.</param>
        public TreeNode(char element, int frequency)
        {
            this.element = element;
            this.frequency = frequency;

            IsElement = true;
        }
        /// <summary>
        /// Creates a new instance as a structure node, containing children.
        /// </summary>
        /// <param name="left">Node connected to the left of this node in the tree.</param>
        /// <param name="right">Node connected to the right of this node in the tree.</param>
        public TreeNode(TreeNode left, TreeNode right)
        {
            this.left = left;
            this.right = right;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// The total value of the frequency / frequencies attached to this node, 
        /// searching recusrively down the tree if this node is a structure node.
        /// </summary>
        /// <returns>The total frequency of this node.</returns>
        public long GetTotalFrequency()
        {
            if (left != null && right != null)
            {
                return left.GetTotalFrequency() + right.GetTotalFrequency();
            }

            /* When deserializing it's super important the tree is reconstructed in the same
             * order it was originally, so to prevent characters of equal frequency swapping places
             * we use the element as a determinator, taking up the least significan bits so
             * it's only ever the difference when the frequencies are the same.*/
            return (frequency << 32) | element;
        }
        #endregion
    }
}
