using BECOSOFT.Utilities.Models;
using System;

namespace BECOSOFT.Utilities.Extensions {
    public static class GenericExtensions {
        /// <summary>
        /// Checks if the value lies in the range defined by the min and max value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>Returns whether the provided value lies in the range defined by the min and max value.</returns>
        public static bool LiesBetween<T>(this T value, T min, T max) where T : IComparable<T>, IEquatable<T> {
            return LiesBetween(value, Range<T>.From(min, max));
        }
        /// <summary>
        /// Checks if the instance lies in the provided range.
        /// </summary>
        /// <param name="value">Instance to check the range for</param>
        /// <param name="range">Range </param>
        /// <returns>Returns whether the provided value lies in the range defined by the min and max value.</returns>
        public static bool LiesBetween<T>(this T value, Range<T> range) where T : IComparable<T>, IEquatable<T> {
            return range.Contains(value);
        }
    }
}
