using BECOSOFT.Utilities.Models;
using System;

namespace BECOSOFT.Utilities.Extensions.Collections {
    /// <summary>
    /// Extensions for arrays
    /// </summary>
    public static class ArrayExtensions {
        /// <summary>
        /// Retrieves the value at the given <see cref="index"/> after shifting the <see cref="seed"/>-array using the <see cref="shift"/> value.
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <param name="seed">The <see cref="seed"/>-array containing the values.</param>
        /// <param name="shift">The <see cref="shift"/> that needs to occur on the <see cref="seed"/>-array.</param>
        /// <param name="index">The index to retrieve on the <see cref="seed"/>-array (after shifting).</param>
        /// <returns>Returns the shifted value of the given <see cref="seed"/>-array.</returns>
        public static T GetShiftedValue<T>(this T[] seed, int shift, int index) where T : struct {
            if (shift == 0) {
                return seed[index];
            }
            var offset = seed.Length - shift % seed.Length;
            var shiftedSeed = new T[seed.Length];
            var size = TypeSize<T>.Size;
            Buffer.BlockCopy(seed, offset * size, shiftedSeed, 0, (seed.Length - offset) * size);
            Buffer.BlockCopy(seed, 0, shiftedSeed, (seed.Length - offset) * size, offset * size);
            return shiftedSeed[index];
        }
    }
}