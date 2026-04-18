namespace BECOSOFT.Utilities.Extensions.Numeric {
    /// <summary>
    /// Extension on the <see cref="long"/>-class
    /// </summary>
    public static class ULongExtensions {
        /// <summary>
        /// Checks if the value lies in the range defined by the min and max value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>Returns whether the provided value lies in the range defined by the min and max value.</returns>
        public static bool LiesBetween(this ulong value, ulong min, ulong max) {
            return value >= min && value <= max;
        }
        /// <summary>
        /// Checks if the provided value is an even number.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns></returns>
        public static bool IsEven(this ulong value) {
            // use a bitwise AND to check if the provided value is odd or even
            return (value & 1) != 1;
        }

        /// <summary>
        /// Checks if the provided value is an odd number.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsOdd(this ulong value) {
            return !value.IsEven();
        }
    }
}