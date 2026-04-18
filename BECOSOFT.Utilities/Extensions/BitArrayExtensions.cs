using System;
using System.Collections;
using System.Linq;

namespace BECOSOFT.Utilities.Extensions {
    public static class BitArrayExtensions {
        /// <summary>
        /// Converts the provided <paramref name="bitArray"/> to a <see cref="byte"/>. Only the first 8 elements of the <paramref name="bitArray"/> are used.
        /// </summary>
        /// <param name="bitArray"></param>
        /// <returns></returns>
        public static byte ToByte(this BitArray bitArray) {
            var bytes = bitArray.ToBytes();
            return bytes[0];
        }

        /// <summary>
        /// Converts the provided <paramref name="bitArray"/> to an <see cref="Array"/> of <see cref="byte"/>.
        /// </summary>
        /// <param name="bitArray"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this BitArray bitArray) {
            var bytes = bitArray
                        .OfType<bool>()
                        .Select(ToChunk)
                        .GroupBy(item => item.Chunk, item => item.Value)
                        .Select(Aggregate)
                        .ToArray();
            return bytes;

            (bool Value, int Chunk) ToChunk(bool value, int index) {
                // into chunks of size 8
                return (value, index / 8);
            }

            byte Aggregate(IGrouping<int, bool> chunk) {
                // Each byte representation should be reversed 
                return (byte) chunk.Reverse().Aggregate(0, (s, bit) => (s << 1) | (bit ? 1 : 0));
            }
        }
    }
}
