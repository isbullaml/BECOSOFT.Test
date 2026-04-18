using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BECOSOFT.Utilities.Models {
    /// <summary>
    /// A range-object
    /// </summary>
    /// <typeparam name="T">The type of the values</typeparam>
    [DebuggerDisplay("Min: {Min} - Max: {Max}")]
    public class Range<T> where T : IComparable<T>, IEquatable<T> {
        /// <summary>
        /// The minimum value in the range
        /// </summary>
        public T Min { get; }
        /// <summary>
        /// The maximum value in the range
        /// </summary>
        public T Max { get; }

        /// <summary>
        /// Creates a <see cref="Range{T}"/> with two values that represents <see cref="Min"/> and <see cref="Max"/>.
        /// </summary>
        /// <param name="min">The <see cref="Min"/> value of the range.</param>
        /// <param name="max">The <see cref="Max"/> value of the range.</param>
        public static Range<T> From(T min, T max) {
            return new Range<T>(min, max);
        }

        /// <summary>
        /// Creates a <see cref="Range{T}"/> with one value that represents both <see cref="Min"/> and <see cref="Max"/>.
        /// </summary>
        /// <param name="value">Value of the <see cref="Range{T}"/></param>
        public static Range<T> From(T value) {
            return new Range<T>(value);
        }

        /// <summary>
        /// Initializes a <see cref="Range{T}"/> with two values that represents <see cref="Min"/> and <see cref="Max"/>.
        /// </summary>
        /// <param name="min">The <see cref="Min"/> value of the range.</param>
        /// <param name="max">The <see cref="Max"/> value of the range.</param>
        public Range(T min, T max) {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Initializes a <see cref="Range{T}"/> with one value that represents both <see cref="Min"/> and <see cref="Max"/>.
        /// </summary>
        /// <param name="value">Value of the <see cref="Range{T}"/></param>
        public Range(T value) {
            Min = value;
            Max = value;
        }

        /// <summary>
        /// Check if a value is included in the range.
        /// This is always an inclusive check.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the value is included, false if not</returns>
        public virtual bool Contains(T value) {
            return Contains(value, RangeComparisonType.Inclusive);
        }

        /// <summary>
        /// Check if a value is included in the range
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <param name="type">The type of comparison</param>
        /// <returns>True if the value is included, false if not</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T value, RangeComparisonType type) {
            switch (type) {
                case RangeComparisonType.Inclusive:
                    return Min.CompareTo(value) <= 0 && value.CompareTo(Max) <= 0;
                case RangeComparisonType.Exclusive:
                    return Min.CompareTo(value) < 0 && value.CompareTo(Max) < 0;
                case RangeComparisonType.LeftExclusive:
                    return Min.CompareTo(value) < 0 && value.CompareTo(Max) <= 0;
                case RangeComparisonType.RightExclusive:
                    return Min.CompareTo(value) <= 0 && value.CompareTo(Max) < 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// Check if a value is excluded from a range
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <param name="type">The type of comparison</param>
        /// <returns>True if the value is excluded, false if not</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Excludes(T value, RangeComparisonType type = RangeComparisonType.Inclusive) {
            return !Contains(value, type);
        }

        /// <summary>
        /// Check if a <see cref="Range{T}"/> overlaps with the current range
        /// </summary>
        /// <param name="other">The second range</param>
        /// <returns>True if they overlap, false if not</returns>
        public virtual bool OverlapsWith(Range<T> other) {
            return Min.CompareTo(other.Max) < 0 && other.Min.CompareTo(Max) < 0;
        }

        /// <summary>
        /// Check if a <see cref="NullableRange{T}"/> overlaps with the current range
        /// </summary>
        /// <typeparam name="U">The type of the nullable range</typeparam>
        /// <param name="other">The second range</param>
        /// <returns>True if they overlap, false if not</returns>
        public virtual bool OverlapsWith<U>(NullableRange<U> other) where U : struct, IComparable<U>, IEquatable<U> {
            if (typeof(T) != typeof(U)) {
                return false;
            }
            return Nullable.Compare((U) (object) Min, other.Max) < 0 && Nullable.Compare(other.Min, (U) (object) Max) < 0;
        }

        /// <summary>
        /// Checks if a <see cref="NullableRange{T}"/> is equal to the current one
        /// </summary>
        /// <typeparam name="U">The type of the nullable range</typeparam>
        /// <param name="other">The second range</param>
        /// <returns>True if the are equal, false if not</returns>
        public bool Equals<U>(NullableRange<U> other) where U : struct, IComparable<U>, IEquatable<U> {
            if (other == null) { return false; }
            return Nullable.Equals(Min, other.Min) && Nullable.Equals(Max, other.Max);
        }

        protected bool Equals(Range<T> other) {
            return EqualityComparer<T>.Default.Equals(Min, other.Min) && EqualityComparer<T>.Default.Equals(Max, other.Max);
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

            return Equals((Range<T>) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() {
            unchecked {
                return (EqualityComparer<T>.Default.GetHashCode(Min) * 397) ^ EqualityComparer<T>.Default.GetHashCode(Max);
            }
        }

        /// <summary>
        /// Returns a string that represents the current range <see cref="Min"/> and <see cref="Max"/> values.
        /// </summary>
        /// <returns>A string that represents the current range <see cref="Min"/> and <see cref="Max"/> values.</returns>
        public override string ToString() {
            return $"{nameof(Min)}: {Min}, {nameof(Max)}: {Max}";
        }
    }

    public static class RangeHelper {

        /// <summary>
        /// Creates a <see cref="Range{T}"/> with two values that represents <see cref="Range{T}.Min"/> and <see cref="Range{T}.Max"/>.
        /// </summary>
        /// <param name="min">The <see cref="Range{T}.Min"/> value of the range.</param>
        /// <param name="max">The <see cref="Range{T}.Max"/> value of the range.</param>
        public static Range<T> Create<T>(T min, T max) where T : IComparable<T>, IEquatable<T> {
            return new Range<T>(min, max);
        }

        /// <summary>
        /// Creates a <see cref="Range{T}"/> with two values that represents <see cref="Range{T}.Min"/> and <see cref="Range{T}.Max"/>.
        /// </summary>
        /// <param name="min">The <see cref="Range{T}.Min"/> value of the range.</param>
        public static Range<T> CreateWithMin<T>(T min) where T : IComparable<T>, IEquatable<T> {
            return new Range<T>(min);
        }

        /// <summary>
        /// Creates a <see cref="Range{T}"/> with two values that represents <see cref="Range{T}.Min"/> and <see cref="Range{T}.Max"/>.
        /// </summary>
        /// <param name="max">The <see cref="Range{T}.Max"/> value of the range.</param>
        public static Range<T> CreateWithMax<T>(T max) where T : IComparable<T>, IEquatable<T> {
            return new Range<T>(max);
        }

        /// <summary>
        /// Initializes a <see cref="Range{T}"/> with one value that represents both <see cref="Range{T}.Min"/> and <see cref="Range{T}.Max"/>.
        /// </summary>
        /// <param name="value">Value of the <see cref="Range{T}"/></param>
        public static Range<T> Create<T>(T value) where T : IComparable<T>, IEquatable<T> {
            return new Range<T>(value);
        }


        /// <summary>
        /// Returns a string that represents the current range <see cref="Range{T}.Min"/> and <see cref="Range{T}.Max"/> values using the provided <see cref="format"/> and <see cref="formatProvider"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="range">The range to return the string representation for</param>
        /// <param name="format">The format to use</param>
        /// <param name="formatProvider">The provider to use to format the value</param>
        /// <returns>A string that represents the current range <see cref="Range{T}.Min"/> and <see cref="Range{T}.Max"/> values using the provided <see cref="format"/> and <see cref="formatProvider"/>.</returns>
        public static string ToString<T>(this Range<T> range, string format, IFormatProvider formatProvider) where T : IComparable<T>, IEquatable<T>, IFormattable {
            return $"{nameof(Range<T>.Min)}: {range.Min.ToString(format, formatProvider)}, {nameof(Range<T>.Max)}: {range.Max.ToString(format, formatProvider)}";
        }

        /// <summary>
        /// Returns a string that represents the current range <see cref="Range{T}.Min"/> and <see cref="Range{T}.Max"/> values using the provided <see cref="format"/> and <see cref="CultureInfo.CurrentCulture"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="range">The range to return the string representation for</param>
        /// <param name="format">The format to use</param>
        /// <returns>A string that represents the current range <see cref="Range{T}.Min"/> and <see cref="Range{T}.Max"/> values using the provided <see cref="format"/> and <see cref="CultureInfo.CurrentCulture"/>.</returns>
        public static string ToString<T>(this Range<T> range, string format) where T : IComparable<T>, IEquatable<T>, IFormattable {
            var formatProvider = CultureInfo.CurrentCulture;
            return range.ToString(format, formatProvider);
        }
        /// <summary>
        /// Aggregates a number of ranges into the least possible ranges.
        /// For example:
        /// [0, 1], [1, 2], [3, 4] will become [0, 2], [3, 4]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ranges"></param>
        /// <returns></returns>
        public static List<Range<T>> AggregateRanges<T>(IEnumerable<Range<T>> ranges) where T : IComparable<T>, IEquatable<T> {
            var result = new List<Range<T>>();
            var sortedRanges = ranges.OrderBy(r => r.Min).ToSafeList();
            if (sortedRanges.Count <= 1) {
                return sortedRanges;
            }

            while (sortedRanges.HasAny()) {
                var mergedMin = sortedRanges[0].Min;
                var mergedMax = sortedRanges[0].Max;
                var checkedRanges = new List<Range<T>>();
                var lastRange = sortedRanges[0];

                if (sortedRanges.Count <= 1) {
                    checkedRanges.Add(lastRange);
                } else {
                    for (var i = 0; i < sortedRanges.Count - 1; i++) {
                        var current = sortedRanges[i];
                        var next = sortedRanges[i + 1];
                        if (current.Contains(next.Min)) {
                            mergedMax = next.Max;
                            checkedRanges.Add(current);
                            lastRange = next;
                            if (sortedRanges.Count - 2 == i) {
                                checkedRanges.Add(lastRange);
                            }
                        } else {
                            checkedRanges.Add(lastRange);
                            break;
                        }
                    }
                }

                var resultingRange = Range<T>.From(mergedMin, mergedMax);
                result.Add(resultingRange);
                sortedRanges.RemoveAll(checkedRanges.Contains);
            }

            return result;
        }
    }
}