using HuffmanCoding.Collections;
using System.Text;

namespace HuffmanCoding
{
    /// <summary>
    /// Serializes and deserializes UTF-16 text using Huffman coding, lossless text compression.
    /// </summary>
    public static class HuffmanSerializer
    {
        #region Public Functions
        /// <summary>
        /// Serializes the text using Huffman coding.
        /// </summary>
        /// <param name="data">Text to serialize.</param>
        /// <returns>Serialized version of the text.</returns>
        public static byte[] SerializeText(string data)
        {
            BinaryWriter writer = new(new MemoryStream(data.Length), Encoding.Unicode);
            Dictionary<char, int> frequencyTable = GenerateFrequencyTable(data);

            // Writing the number of entries that are in the table as there is
            // no marker we can use to denote the end of the entry sequence.
            writer.Write(frequencyTable.Count);

            foreach (KeyValuePair<char, int> entry in frequencyTable) {

                writer.Write(entry.Key);
                writer.Write(entry.Value);
            }

            // Writing the length of the original text so during reconstruction
            // there is not need to reallocate / resize the string builder.
            writer.Write(data.Length);

            // Converting the original text into the encoded Huffman data.
            HuffmanTree huffmanTree = new(frequencyTable);
            BitStream stream = new(data.Length * 8);

            foreach (char character in data) {
                huffmanTree.EncodeCharacter(character, stream);
            }

            // Writing the number of bits in the stream as the last few bits of
            // the byte might be empty and we shouldn't attempt to decode it.
            writer.Write(stream.WrittenBits);
            writer.Write(stream.ToByteArray());

            writer.Flush();
            return ((MemoryStream)writer.BaseStream).ToArray();
        }
        /// <summary>
        /// Deserializes the Huffman coding back into it's original text.
        /// </summary>
        /// <param name="data">Huffman encoded data to decode.</param>
        /// <returns>Text that was originally serialized.</returns>
        public static string DeserializeText(byte[] data)
        {
            BinaryReader reader = new(new MemoryStream(data), Encoding.Unicode);

            // Reading the number of entries that were originally written.
            int entryCount = reader.ReadInt32();
            Dictionary<char, int> frequencyTable = new(entryCount);

            // Reconstructing the frequency table that was originally generated.
            for (int index = 0; index < entryCount; index++) {

                char character = reader.ReadChar();
                int frequency = reader.ReadInt32();

                frequencyTable.Add(character, frequency);
            }

            // The length of the original data was written so we can use this
            // to prevent reallocation of the string builder when we're decoding.
            StringBuilder results = new StringBuilder(reader.ReadInt32());

            // Reading the number of bits that were originally written to avoid
            // reading some extra bits that might have been written originally.
            ulong writtenBits = reader.ReadUInt64();

            int payloadSize = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
            byte[] payload = reader.ReadBytes(payloadSize);

            // Rebuilding the Huffman tree and bit stream.
            HuffmanTree huffmanTree = new(frequencyTable);
            BitStream stream = new(payload, writtenBits);

            // We need to read the bits in the oder they were written in
            // as dequeuing the bits gives us them in the reverse order.
            stream.Reverse();

            while (stream.WrittenBits > 0) {
                results.Append(huffmanTree.DecodeCharacter(stream));
            }

            return results.ToString();
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Generates the frequency table used to generate the Huffman tree.
        /// </summary>
        /// <param name="inputText">Text to generate the frequency table from.</param>
        /// <returns>Frequency table to create the Huffman tree.</returns>
        private static Dictionary<char, int> GenerateFrequencyTable(string inputText)
        {
            Dictionary<char, int> frequencyMap = new(inputText.Length);

            foreach (char character in inputText) {

                if (frequencyMap.TryAdd(character, 1))
                    continue;
                frequencyMap[character]++;
            }

            return frequencyMap;
        }
        #endregion
    }
}
