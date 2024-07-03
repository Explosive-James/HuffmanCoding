
namespace HuffmanCoding.Collections
{
    /// <summary>
    /// A dynamically sized collection for reading and writing individual bit values.
    /// </summary>
    public sealed class BitStream
    {
        #region Data
        /// <summary>
        /// The underlying stream of bits.
        /// </summary>
        readonly List<byte> _stream;
        #endregion

        #region Properties
        /// <summary>
        /// The number of bits that have been written so far.
        /// </summary>
        public ulong WrittenBits { get; private set; }

        /// <summary>
        /// The total capacity of bits that can be written 
        /// without having to reallocate / resize the stream.
        /// </summary>
        public int Capacity => _stream.Capacity * 8;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance with a capacity to store a set number 
        /// of bits without having to reallocate or resize itself.
        /// </summary>
        /// <param name="capacity">
        /// The number of bits it can store before resizing.</param>
        public BitStream(int capacity = 0)
        {
            _stream = new((int)Math.Ceiling(capacity / 8f));
        }
        /// <summary>
        /// Creates a new instance with the same bit values in the stream.
        /// </summary>
        /// <param name="stream">The bit values to have.</param>
        /// <param name="totalBits">
        /// The total number of bits that are written in the stream.</param>
        public BitStream(byte[] stream, ulong totalBits)
        {
            _stream = new(stream);
            WrittenBits = totalBits;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Inserts a bit at the end of the collection.
        /// </summary>
        /// <param name="bitValue">
        /// Should the bit be flipped, a 1 or a 0.</param>
        public void EnqueueBit(bool bitValue)
        {
            int byteIndex = (int)(WrittenBits / 8);

            if (byteIndex >= _stream.Count) _stream.Add(0);

            // We only need to write the bit if the bit needs to be flipped.
            if (bitValue) {

                int bitDepth = (int)(WrittenBits % 8);
                _stream[byteIndex] |= (byte)(0x1 << bitDepth);
            }

            WrittenBits++;
        }
        /// <summary>
        /// Removes the bit at the end of the collection.
        /// </summary>
        /// <returns>
        /// Was the bit flipped, a 1 or a 0.</returns>
        public bool DequeueBit()
        {
            WrittenBits--;

            int byteIndex = (int)(WrittenBits / 8);
            int bitDepth = (int)(WrittenBits % 8);

            byte bitmask = (byte)(0x1 << bitDepth);
            return (_stream[byteIndex] & bitmask) != 0;
        }

        /// <summary>
        /// Reverses the order of all the bits in the collection.
        /// </summary>
        public void Reverse()
        {
            byte[] buffer = new byte[(WrittenBits - 1) / 8 + 1];

            for (ulong bufferBit = 0; bufferBit < WrittenBits; bufferBit++) {

                // Calculating which byte in the stream and what
                // bit to read from the original stream of bits.
                int streamBit = (int)(WrittenBits - 1 - bufferBit);
                int streamByte = streamBit / 8;
                int streamDepth = streamBit % 8;

                // If the bit isn't flipped then we don't need to write anything.
                if ((_stream[streamByte] & 0x1 << streamDepth) == 0)
                    continue;

                // Calculating which byte and bit to write to.
                int bufferByte = (int)(bufferBit / 8);
                int bufferDepth = (int)(bufferBit % 8);
                buffer[bufferByte] |= (byte)(0x1 << bufferDepth);
            }

            // Copying the reverse bit stream into the original bit stream.
            for (int bufferIndex = 0; bufferIndex < buffer.Length; bufferIndex++)
                _stream[bufferIndex] = buffer[bufferIndex];
        }

        /// <summary>
        /// Converts the stream of bits to a collection of bytes.
        /// </summary>
        /// <returns>The bit stream as a byte array.</returns>
        public byte[] ToByteArray()
        {
            int totalBytes = (int)((WrittenBits - 1) / 8) + 1;
            byte[] results = new byte[totalBytes];

            for (int index = 0; index < totalBytes; index++) {
                results[index] = _stream[index];
            }

            return results;
        }
        #endregion
    }
}
