
namespace HuffmanCoding.Collections
{
    /// <summary>
    /// The Huffman Tree is a tree of characters and their frequencies (how often they're used)
    /// where the most used / common characters are higher up in the tree, this means the most 
    /// commonly used characters can be encoded using fewer than 8 bits while still being able to
    /// encode any character, thus taking up less space overall.
    /// </summary>
    public sealed class HuffmanTree
    {
        #region Data
        /// <summary>
        /// The lookup table gives us the character / element node
        /// without having to search through the entire tree everytime.
        /// </summary>
        readonly Dictionary<char, TreeNode> _lookupTable;

        /// <summary>
        /// The root node of the tree, the highest node that's also
        /// the starting point when decoding the stream of bits.
        /// </summary>
        readonly TreeNode _rootNode;
        /// <summary>
        /// A fixed size buffer to store the path taken through the 
        /// tree to find a specific node starting from the root.
        /// </summary>
        readonly TreeNode[] _searchBuffer;
        #endregion

        #region Properties
        /// <summary>
        /// The maximum number of layers in the tree or how many nodes down
        /// until the node is gauranteed to be a character / element node.
        /// </summary>
        public int MaximumDepth { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the Huffman tree designed to work with a
        /// specific set of characters found in the frequency tree.
        /// </summary>
        /// <param name="frequencyTable">
        /// The frequency tree is made up of all the characters in the text 
        /// that will be encoded along with how often they appear in the text.
        /// </param>
        public HuffmanTree(Dictionary<char, int> frequencyTable)
        {
            _lookupTable = new(frequencyTable.Count);

            // The lowest frequency nodes need to be at the bottom of the tree,
            // this returns the currently lowest node to attach to the higher nodes.
            PriorityQueue<TreeNode, long> elementNodes = new(frequencyTable.Count);

            // Converting the frequency table into the Huffman tree nodes.
            foreach (KeyValuePair<char, int> element in frequencyTable) {

                TreeNode node = new TreeNode(element.Key, element.Value);
                elementNodes.Enqueue(node, node.GetTotalFrequency());

                // Adding the nodes to the lookup table before other nodes are generated
                // this means the only nodes inside here are the element nodes that are
                // actually used in the search.
                _lookupTable.Add(element.Key, node);
            }

            // Tracking the node that will be the lowest in the tree so the search buffer is at
            // a size where even when searching for the deepest node the buffer is big enough.
            TreeNode minimumNode = elementNodes.Peek();

            // Generating the non-element nodes and attaching them all together into one big tree.
            // When there is only one node left in the queue it doesn't need to be attached to any
            // other node and will become the root.
            while (elementNodes.Count > 1) {

                TreeNode nodeA = elementNodes.Dequeue(),
                         nodeB = elementNodes.Dequeue();
                TreeNode parentNode = new(nodeA, nodeB);

                // Assigning the parents to the child nodes so we can traverse up the tree easily.
                (nodeA.parent, nodeB.parent) = (parentNode, parentNode);

                // Tracking the depth of the tree.
                if (nodeA == minimumNode || nodeB == minimumNode) {

                    minimumNode = parentNode;
                    MaximumDepth++;
                }

                // Add the node to the queue so it can be picked up again further up the tree.
                elementNodes.Enqueue(parentNode, parentNode.GetTotalFrequency());
            }

            _rootNode = elementNodes.Dequeue();
            _searchBuffer = new TreeNode[MaximumDepth + 1];
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Encodes the character using the tree, 
        /// storing the path taken into the stream.
        /// </summary>
        /// <param name="character">Character to encode.</param>
        /// <param name="stream">Stream to write the path to.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the character isn't inside the tree.</exception>
        public void EncodeCharacter(char character, BitStream stream)
        {
            if (!_lookupTable.TryGetValue(character, out TreeNode? element)) {
                throw new ArgumentException($"Character '{character}' is not in Tree.");
            }

            // Finding the path to the node along with
            // how many steps are needed to get there.
            int pathLength = GenerateTreePath(element, _searchBuffer);

            // Iterating over the search buffer to write
            // the path needed to get to the element node.
            for (int index = 0; index < pathLength; index++) {

                TreeNode currentNode = _searchBuffer[index];

                // If the node is an element node then this
                // is the goal and we can't traverse futher
                if (currentNode.IsElement) return;

                // When decoding we need to know the path taken through the tree,
                // so a 1 is written for moving right and 0 for when moving left.
                TreeNode nextNode = _searchBuffer[index + 1];
                stream.EnqueueBit(nextNode == currentNode.right);
            }

            // This shouldn't ever happen.
            throw new Exception("Element Node not in Search Path.");
        }
        /// <summary>
        /// Decodes the character using the tree, 
        /// using the path stored in the stream.
        /// </summary>
        /// <param name="stream">
        /// The stream to read the path from.</param>
        /// <returns>The character found after following the path.</returns>
        public char DecodeCharacter(BitStream stream)
        {
            TreeNode currentNode = _rootNode;

            // While we haven't found the element / character node, keep searching.
            while (currentNode.left != null && currentNode.right != null) {

                // When encoding we use 1 for right and 0 for left.
                bool useRight = stream.DequeueBit();
                currentNode = useRight ? currentNode.right : currentNode.left;
            }

            return currentNode.element;
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Follows the element node back to the root node, writing the path of nodes needed in reverse order.
        /// This means the path of nodes starts from the root node and ends at the element node at the end.
        /// </summary>
        /// <param name="element">Element node to write the path for.</param>
        /// <param name="searchBuffer">Array to store the path of nodes to.</param>
        /// <returns>The number of nodes needed to get from the root node to the element node.</returns>
        private static int GenerateTreePath(TreeNode element, TreeNode[] searchBuffer)
        {
            // Clearing the search buffer from the previous use.
            Array.Clear(searchBuffer, 0, searchBuffer.Length);

            // Writing from the back of the array first so the start of the buffer is the root node.
            for (int currentDepth = searchBuffer.Length - 1; currentDepth >= 0; currentDepth--) {

                // Adding the current node to the end of the buffer.
                searchBuffer[currentDepth] = element;

                if (element.IsRoot) {

                    // Shifting the elements over to the start of the buffer if the buffer isn't full.
                    for (int index = currentDepth; index < searchBuffer.Length; index++)
                        searchBuffer[index - currentDepth] = searchBuffer[index];

                    return searchBuffer.Length - currentDepth;
                }

                // Moving over to the next node to find the root node.
                if (element.parent != null) element = element.parent;
            }

            throw new IndexOutOfRangeException("Buffer ran out of space before finding the root.");
        }
        #endregion
    }
}
