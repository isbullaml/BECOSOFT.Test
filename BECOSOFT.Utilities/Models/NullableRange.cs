using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace BECOSOFT.Utilities.Models {
    /// <summary>
    /// A range-object with values that can be NULL
    /// </summary>
    /// <typeparam name="T">The type of the values</typeparam>
    [DebuggerDisplay("Min: {Min} - Max: {Max}")]
    public class NullableRange<T> where T : struct, IComparable<T>, IEquatable<T> {
        /// <summary>
        /// The minimum value in the range
        /// </summary>
        public T? Min { get; }

        /// <summary>
        /// The maximum value in the range
        /// </summary>
        public T? Max { get; }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="Min"/> has a valid value of its underlying type.
        /// </summary>
        public bool HasMin => Min.HasValue;

        /// <summary>
        /// Gets a value indicating whether the current <see cref="Max"/> has a valid value of its underlying type.
        /// </summary>
        public bool HasMax => Max.HasValue;

        /// <summary>
        /// Indicates whether this <see cref="NullableRange{T}"/> has valid <see cref="Min"/> or <see cref="Max"/> values.
        /// </summary>
        public bool HasValue => HasMin || HasMax;

        /// <summary>
        /// Initializes a <see cref="NullableRange{T}"/> with two values that represents <see cref="Min"/> and <see cref="Max"/>.
        /// </summary>
        /// <param name="min">The <see cref="Min"/> value of the range.</param>
        /// <param name="max">The <see cref="Max"/> value of the range.</param>
        public NullableRange(T? min, T? max) {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Initializes a <see cref="NullableRange{T}"/> with one value that represents both <see cref="Min"/> and <see cref="Max"/>.
        /// </summary>
        /// <param name="value">Value of the <see cref="NullableRange{T}"/></param>
        public NullableRange(T? value) {
            Min = value;
            Max = value;
        }

        /// <summary>
        /// Converts the <see cref="NullableRange{T}"/> to a <see cref="Range{T}"/>
        /// </summary>
        /// <returns>The converted range</returns>
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public Range<T> ToRange() {
            if (!HasMin || !HasMax) {
                throw new ArgumentException();
            }
            return new Range<T>(Min.Value, Max.Value);
        }

        /// <summary>
        /// Try converting the current <see cref="NullableRange{T}"/> to a <see cref="Range{T}"/>. The conversion is valid when both <see cref="Min"/> and <see cref="Max"/> have defined values.
        /// </summary>
        /// <param name="range">Resulting <see cref="Range{T}"/></param>
        /// <returns>Returns a value indicating whether the conversion was successful.</returns>
        public bool TryConvertToRange(out Range<T> range) {
            if (HasMin && HasMax) {
                range = ToRange();
                return true;
            }
            range = null;
            return false;
        }

        /// <summary>
        /// Check if a value is included in the range
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the value is included, false if not</returns>
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public bool Contains(T value) {
            if (HasMin) {
                if (HasMax) {
                    return Min.Value.CompareTo(value) <= 0 && value.CompareTo(Max.Value) <= 0;
                }
                return Min.Value.CompareTo(value) <= 0;
            }
            if (HasMax) {
                return value.CompareTo(Max.Value) <= 0;
            }
            return false;
        }

        /// <summary>
        /// Check if a value is excluded from a range
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the value is excluded, false if not</returns>
        public bool Excludes(T value) {
            return !Contains(value);
        }

        /// <summary>
        /// Check if a <see cref="NullableRange{T}"/> overlaps with the current range
        /// </summary>
        /// <param name="other">The second range</param>
        /// <returns>True if they overlap, false if not</returns>
        public bool OverlapsWith(NullableRange<T> other) {
            return Nullable.Compare(Min, other.Max) < 0 && Nullable.Compare(other.Min, Max) < 0;
        }

        /// <summary>
        /// Check if a <see cref="Range{T}"/> overlaps with the current range
        /// </summary>
        /// <param name="other">The second range</param>
        /// <returns>True if they overlap, false if not</returns>
        public bool OverlapsWith(Range<T> other) {
            return Nullable.Compare(Min, other.Max) < 0 && Nullable.Compare(other.Min, Max) < 0;
        }

        /// <summary>
        /// Checks if a <see cref="Range{T}"/> is equal to the current one
        /// </summary>
        /// <param name="other">The second range</param>
        /// <returns>True if the are equal, false if not</returns>
        public bool Equals(Range<T> other) {
            if (other == null) { return false; }
            return Nullable.Equals(Min, other.Min) && Nullable.Equals(Max, other.Max);
        }

        private bool Equals(NullableRange<T> other) {
            if (other == null) { return false; }
            return Nullable.Equals(Min, other.Min) && Nullable.Equals(Max, other.Max);
        }

        /// <inheritdoc />
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) { return false; }
            return Equals((NullableRange<T>) obj);
        }


        /// <inheritdoc />
        public override int GetHashCode() {
            unchecked {
                return (Min.GetHashCode() * 397) ^ Max.GetHashCode();
            }
        }

        /// <summary>
        /// Returns a string that represents the current range <see cref="Min"/> and <see cref="Max"/> values.
        /// </summary>
        /// <returns>A string that represents the current range <see cref="Min"/> and <see cref="Max"/> values.</returns>
        public override string ToString() {
            return $"{nameof(Min)}: {Min?.ToString() ?? "null"}, {nameof(Max)}: {Max?.ToString() ?? "null"}";
        }
    }

    public static class NullableRange {
        /// <summary>
        /// Creates a <see cref="NullableRange{T}"/> with two values that represents <see cref="NullableRange{T}.Min"/> and <see cref="NullableRange{T}.Max"/>.
        /// </summary>
        /// <param name="min">The <see cref="NullableRange{T}.Min"/> value of the range.</param>
        /// <param name="max">The <see cref="NullableRange{T}.Max"/> value of the range.</param>
        public static NullableRange<T> Create<T>(T? min, T? max) where T : struct, IComparable<T>, IEquatable<T> {
            return new NullableRange<T>(min, max);
        }
        /// <summary>
        /// Creates a <see cref="NullableRange{T}"/> with two values that represents <see cref="NullableRange{T}.Min"/> and <see cref="NullableRange{T}.Max"/>.
        /// </summary>
        /// <param name="min">The <see cref="NullableRange{T}.Min"/> value of the range.</param>
        /// <param name="max">The <see cref="NullableRange{T}.Max"/> value of the range.</param>
        public static NullableRange<T> Create<T>(T min, T max) where T : struct, IComparable<T>, IEquatable<T> {
            return new NullableRange<T>(min, max);
        }

        /// <summary>
        /// Creates a <see cref="NullableRange{T}"/> with one value that represents both <see cref="NullableRange{T}.Min"/> and <see cref="NullableRange{T}.Max"/>.
        /// </summary>
        /// <param name="value">Value of the <see cref="NullableRange{T}"/></param>
        public static NullableRange<T> Create<T>(T value) where T : struct, IComparable<T>, IEquatable<T> {
            return new NullableRange<T>(value);
        }

        /// <summary>
        /// Creates a <see cref="NullableRange{T}"/> with two values that represents <see cref="NullableRange{T}.Min"/> and <see cref="NullableRange{T}.Max"/>.
        /// </summary>
        /// <param name="min">The <see cref="NullableRange{T}.Min"/> value of the range.</param>
        public static NullableRange<T> CreateWithMin<T>(T? min) where T : struct, IComparable<T>, IEquatable<T> {
            return new NullableRange<T>(min, null);
        }

        /// <summary>
        /// Creates a <see cref="NullableRange{T}"/> with two values that represents <see cref="NullableRange{T}.Min"/> and <see cref="NullableRange{T}.Max"/>.
        /// </summary>
        /// <param name="min">The <see cref="NullableRange{T}.Min"/> value of the range.</param>
        public static NullableRange<T> CreateWithMin<T>(T min) where T : struct, IComparable<T>, IEquatable<T> {
            return new NullableRange<T>(min, null);
        }

        /// <summary>
        /// Creates a <see cref="NullableRange{T}"/> with two values that represents <see cref="NullableRange{T}.Min"/> and <see cref="NullableRange{T}.Max"/>.
        /// </summary>
        /// <param name="max">The <see cref="NullableRange{T}.Max"/> value of the range.</param>
        public static NullableRange<T> CreateWithMax<T>(T? max) where T : struct, IComparable<T>, IEquatable<T> {
            return new NullableRange<T>(null, max);
        }

        /// <summary>
        /// Creates a <see cref="NullableRange{T}"/> with two values that represents <see cref="NullableRange{T}.Min"/> and <see cref="NullableRange{T}.Max"/>.
        /// </summary>
        /// <param name="max">The <see cref="NullableRange{T}.Max"/> value of the range.</param>
        public static NullableRange<T> CreateWithMax<T>(T max) where T : struct, IComparable<T>, IEquatable<T> {
            return new NullableRange<T>(null, max);
        }

        /// <summary>
        /// Creates a <see cref="NullableRange{T}"/> with one value that represents both <see cref="NullableRange{T}.Min"/> and <see cref="NullableRange{T}.Max"/>.
        /// </summary>
        /// <param name="value">Value of the <see cref="NullableRange{T}"/></param>
        public static NullableRange<T> Create<T>(T? value) where T : struct, IComparable<T>, IEquatable<T> {
            return new NullableRange<T>(value);
        }


        /// <summary>
        /// Returns a string that represents the current range <see cref="NullableRange{T}.Min"/> and <see cref="NullableRange{T}.Max"/> values using the provided <see cref="format"/> and <see cref="formatProvider"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="range">The range to return the string representation for</param>
        /// <param name="format">The format to use</param>
        /// <param name="formatProvider">The provider to use to format the value</param>
        /// <returns>A string that represents the current range <see cref="NullableRange{T}.Min"/> and <see cref="NullableRange{T}.Max"/> values using the provided <see cref="format"/> and <see cref="formatProvider"/>.</returns>
        public static string ToString<T>(this NullableRange<T> range, string format, IFormatProvider formatProvider) where T : struct, IComparable<T>, IEquatable<T>, IFormattable {
            return $"{nameof(NullableRange<T>.Min)}: {range.Min?.ToString(format, formatProvider) ?? "null"}, {nameof(NullableRange<T>.Max)}: {range.Max?.ToString(format, formatProvider) ?? "null"}";
        }

        /// <summary>
        /// Returns a string that represents the current range <see cref="NullableRange{T}.Min"/> and <see cref="NullableRange{T}.Max"/> values using the provided <see cref="format"/> and <see cref="CultureInfo.CurrentCulture"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="range">The range to return the string representation for</param>
        /// <param name="format">The format to use</param>
        /// <returns>A string that represents the current range <see cref="NullableRange{T}.Min"/> and <see cref="NullableRange{T}.Max"/> values using the provided <see cref="format"/> and <see cref="CultureInfo.CurrentCulture"/>.</returns>
        public static string ToString<T>(this NullableRange<T> range, string format) where T : struct, IComparable<T>, IEquatable<T>, IFormattable {
            var formatProvider = CultureInfo.CurrentCulture;
            return range.ToString(format, formatProvider);
        }
    }
}