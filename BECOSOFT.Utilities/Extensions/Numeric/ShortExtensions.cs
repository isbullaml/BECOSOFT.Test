using System;

namespace BECOSOFT.Utilities.Extensions.Numeric {
    /// <summary>
    /// Extension on the <see cref="short"/>-class
    /// </summary>
    public static class ShortExtensions {
        /// <summary>
        /// Checks if the value lies in the range defined by the min and max value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>Returns whether the provided value lies in the range defined by the min and max value.</returns>
        public static bool LiesBetween(this short value, short min, short max) {
            return value >= min && value <= max;
        }
        /// <summary>
        /// Checks if the value lies in the range defined by the min and max value.
        /// If <see cref="value"/> does not have a value (<see cref="Nullable{T}.HasValue"/>), the result is <see langword="false"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>Returns whether the provided value lies in the range defined by the min and max value.</returns>
        public static bool LiesBetween(this short? value, short min, short max) {
            if (!value.HasValue) { return false; }
            return value >= min && value <= max;
        }
        /// <summary>
        /// Checks if the provided value is an even number.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns></returns>
        public static bool IsEven(this short value) {
            // use a bitwise AND to check if the provided value is odd or even
            return (value & 1) != 1;
        }

        /// <summary>
        /// Checks if the provided value is an odd number.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsOdd(this short value) {
            return !value.IsEven();
        }

        /// <summary>
        /// Returns whether the <paramref name="value"/> is negative or not.
        /// Returns <see langword="true"/> when <paramref name="value"/> is &lt; <see langword="0"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNegative(this short value) {
            return value < 0;
        }
    }
}