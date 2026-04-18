using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BECOSOFT.Utilities.Models {
    /// <summary>
    /// A circular range-object
    /// This can be used when a range goes back to the start after the last item.
    /// </summary>
    [DebuggerDisplay("Min: {Min} - Max: {Max}")]
    public class CircularRange<T> : Range<T> where T : IComparable<T>, IEquatable<T> {

        /// <inheritdoc />
        public CircularRange(T min, T max) : base(min, max) { }

        /// <inheritdoc />
        public CircularRange(T value) : base(value) { }

        /// <summary>
        /// Checks if the range contains a value.
        /// Because this is a circular range, Min can be bigger than Max.
        ///
        /// For example:
        /// Value is 12
        /// Range A is 17 -> 9
        /// The value is in the range because this means Range B is 17 -> END and BEGIN -> 9.
        /// </summary>
        /// <param name="value">The value to check for</param>
        /// <returns>Value indicating whether the ranges overlap</returns>
        public override bool Contains(T value) {
            if (Min.CompareTo(Max) > 0) {
                if (value.CompareTo(Min) >= 0 || value.CompareTo(Max) <= 0) {
                    return true;
                }
            } else {
                if (value.CompareTo(Min) >= 0 && value.CompareTo(Max) <= 0) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if two ranges overlap.
        /// Because this is a circular range, Min can be bigger than Max.
        ///
        /// For example:
        /// Range A is 4 -> 15
        /// Range B is 17 -> 9
        /// These ranges will overlap because this means Range B is 17 -> END and BEGIN -> 9.
        /// </summary>
        /// <param name="other">The other range</param>
        /// <returns>Value indicating whether the ranges overlap</returns>
        public override bool OverlapsWith(Range<T> other) {
            return Contains(other.Min) || Contains(other.Max) || other.Contains(Min) || other.Contains(Max);
        }

        /// <inheritdoc />
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((CircularRange<T>) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() {
            unchecked { return EqualityComparer<T>.Default.GetHashCode(Min) * 397 ^ EqualityComparer<T>.Default.GetHashCode(Max); }
        }
    }
}