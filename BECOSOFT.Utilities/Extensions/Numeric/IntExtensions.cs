namespace BECOSOFT.Utilities.Extensions.Numeric {
    /// <summary>
    /// Extension on the <see cref="int"/>-class
    /// </summary>
    public static class IntExtensions {
        /// <summary>
        /// Checks if the value lies in the range defined by the min and max value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>Returns whether the provided value lies in the range defined by the min and max value.</returns>
        public static bool LiesBetween(this int value, int min, int max) {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Checks if the provided value is an even number.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns></returns>
        public static bool IsEven(this int value) {
            // use a bitwise AND to check if the provided value is odd or even:
            /* Number to check:     00011001   (25 in binary) 
                                    00000001   (00000001 is 1 in binary)
                                           &
                                    --------
                                    00000001   => number is odd

               Number to check:     00011000   (24 in binary)
                                    00000001   (00000001 is 1 in binary)
                                           &
                                    --------
                                    00000000  => number is even
             */
            return (value & 1) != 1;
        }

        /// <summary>
        /// Checks if the provided value is an odd number.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsOdd(this int value) {
            return !value.IsEven();
        }

        /// <summary>
        /// Returns whether the <paramref name="value"/> is negative or not.
        /// Returns <see langword="true"/> when <paramref name="value"/> is &lt; <see langword="0"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNegative(this int value) {
            return value < 0;
        }
    }
}