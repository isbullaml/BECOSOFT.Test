namespace BECOSOFT.Utilities.Extensions.Numeric {
    /// <summary>
    /// Extension on the <see cref="byte"/>-class
    /// </summary>
    public static class ByteExtensions {
        /// <summary>
        /// Checks if the value lies in the range defined by the min and max value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>Returns whether the provided value lies in the range defined by the min and max value.</returns>
        public static bool LiesBetween(this byte value, byte min, byte max) {
            return value >= min && value <= max;
        }
        /// <summary>
        /// Checks if the provided value is an even number.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns></returns>
        public static bool IsEven(this byte value) {
            // use a bitwise AND to check if the provided value is odd or even
            return (value & 1) != 1;
        }

        /// <summary>
        /// Checks if the provided value is an odd number.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsOdd(this byte value) {
            return !value.IsEven();
        }

        public static bool IsBitSet(this byte b, int offset) {
            return (b & (1 << offset)) != 0;
        }

        public static bool IsBitNotSet(this byte b, int offset) {
            return (b & (1 << offset)) == 0;
        }

        /// <summary>
        /// Returns whether the <paramref name="value"/> is negative or not.
        /// Returns <see langword="true"/> when <paramref name="value"/> is &lt; <see langword="0"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNegative(this sbyte value) {
            return value < 0;
        }
    }
}