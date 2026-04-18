using System;

namespace BECOSOFT.Utilities.Comparers {
    /// <summary>
    /// Contains generic helper functions for comparisons.
    /// </summary>
    public static class Comparer {
        
        /// <summary>Returns the larger of two values.</summary>
        /// <param name="first">The first of two values to compare.</param>
        /// <param name="second">The second of two values to compare.</param>
        /// <returns>Parameter <paramref name="first" /> or <paramref name="second" />, whichever is larger.</returns>
        public static T Max<T>(T first, T second) where T: IComparable<T> {
            if (first.CompareTo(second) > 0) {
                return first;
            }
            return second;
        }
   
        /// <summary>Returns the smaller of two values.</summary>
        /// <param name="first">The first of two values to compare.</param>
        /// <param name="second">The second of two values to compare.</param>
        /// <returns>Parameter <paramref name="first" /> or <paramref name="second" />, whichever is smaller.</returns>
        public static T Min<T>(T first, T second) where T: IComparable<T> {
            if (first.CompareTo(second) < 0) {
                return first;
            }
            return second;
        }
    }
}
