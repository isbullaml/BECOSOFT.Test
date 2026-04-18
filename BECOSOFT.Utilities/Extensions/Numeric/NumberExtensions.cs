using System;

namespace BECOSOFT.Utilities.Extensions.Numeric {
    /// <summary>
    /// Extensions for numbers
    /// </summary>
    public static class NumberExtensions {
        /// <summary>
        /// Returns <see langword="null"/> if <see cref="first"/> equals <see cref="second"/>, otherwise <see cref="first"/> is returned. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static T? NullIf<T>(this T first, T second) where T : struct {
            if (first.Equals(second)) {
                return null;
            }
            return first;
        }
        /// <summary>
        /// Returns <see langword="null"/> if <see cref="first"/> equals <see cref="second"/>, otherwise <see cref="first"/> is returned. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static T? NullIf<T>(this T? first, T? second) where T : struct {
            if (Nullable.Equals(first, second)) {
                return null;
            }
            return first;
        }

        /// <summary>
        /// Scale an <see cref="int"/>
        /// </summary>
        /// <param name="value">The value to scale</param>
        /// <param name="scale">The scaling factor</param>
        /// <returns>The scaled value</returns>
        public static int Scaled(this int value, double scale) {
            return (int) Math.Round(value * scale);
        }

        /// <summary>
        /// Scale a <see cref="double"/>
        /// </summary>
        /// <param name="value">The value to scale</param>
        /// <param name="scale">The scaling factor</param>
        /// <returns>The scaled value</returns>
        public static double Scaled(this double value, double scale) {
            return value * scale;
        }
        
        /// <summary>
        /// Returns <see cref="value"/> clamped to the inclusive range of <see cref="min"/> and <see cref="max"/>.
        /// If <see cref="value"/> is lower than <see cref="min"/>, <see cref="min"/> is returned.
        /// If <see cref="value"/> is higher than <see cref="max"/>, <see cref="max"/> is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value to be clamped.</param>
        /// <param name="min">The lower bound of the result.</param>
        /// <param name="max">The upper bound of the result.</param>
        /// <returns></returns>
        public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T> {
            if (value.CompareTo(min) < 0) { return min;}
            if (value.CompareTo(max) > 0) { return max; }
            return value;
        }
    }
}