using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Numeric;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace BECOSOFT.Utilities.IO {
    public enum PadDirection {
        Left,
        Right,
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class ContentLine {

        public List<ContentValue> Values { get; set; }

        public ContentLine(int? capacity = null) {
            if (capacity.HasValue) {
                Values = new List<ContentValue>(capacity.Value);
            } else {
                Values = new List<ContentValue>();
            }
        }
        
        /// <summary>
        /// Adds an empty value (<see cref="string.Empty"/>) to the <see cref="Values"/> collection.
        /// </summary>
        public void AddEmpty() {
            AddValueInternal(string.Empty);
        }

        /// <summary>
        /// Adds a value to the <see cref="Values"/> collection.
        /// </summary>
        /// <param name="value">Value to be added to the <see cref="Values"/> collection.</param>
        public void AddValue(string value) {
            AddValueInternal(value);
        }

        /// <summary>
        /// Adds a value to the <see cref="Values"/> collection. This <see cref="value"/> will not be formatted until it is written.
        /// </summary>
        /// <param name="value">Value to be added to the <see cref="Values"/> collection.</param>
        /// <param name="enclose"></param>
        public void AddValue<T>(T value, bool enclose = false) {
            AddValueInternal(value, enclose);
        }

        /// <summary>
        /// Adds a value to the <see cref="Values"/> collection.
        /// </summary>
        /// <param name="value">Value to be added to the <see cref="Values"/> collection.</param>
        /// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
        /// <param name="direction">The direction to pad.</param>
        /// <param name="paddingChar">A padding character.</param>
        public void AddPaddedValue(string value, int totalWidth, PadDirection direction = PadDirection.Right, char paddingChar = ' ') {
            AddPaddedValueInternal(value, totalWidth, direction, paddingChar);
        }

        /// <summary>
        /// Adds a value to the <see cref="Values"/> collection. This value gets converted immediately to the formatted value
        /// </summary>
        /// <param name="value">Value to be added to the <see cref="Values"/> collection.</param>
        /// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
        /// <param name="direction">The direction to pad.</param>
        /// <param name="paddingChar">A padding character.</param>
        public void AddPaddedValue<T>(T value, int totalWidth, PadDirection direction = PadDirection.Right, char paddingChar = ' ') {
            if (value is IConvertible convertible) {
                AddPaddedValueInternal(convertible.ToString(CultureInfo.InvariantCulture), totalWidth, direction, paddingChar);
                return;
            }
            if (value is IFormattable formattable) {
                AddPaddedValueInternal(formattable.ToString(null, CultureInfo.InvariantCulture), totalWidth, direction, paddingChar);
                return;
            }
            AddPaddedValueInternal(value?.ToString(), totalWidth, direction, paddingChar);
        }

        private void AddPaddedValueInternal(string value, int totalWidth, PadDirection direction, char paddingChar) {
            if (value == null) {
                AddEmpty();
            } else {
                string paddedValue;
                if (direction == PadDirection.Left) {
                    paddedValue = value.PadLeft(totalWidth, paddingChar);
                } else {
                    paddedValue = value.PadRight(totalWidth, paddingChar);
                }
                if (paddedValue.Length > totalWidth) {
                    paddedValue = value.Truncate(totalWidth, string.Empty);
                }
                AddValueInternal(paddedValue);
            }
        }

        private void AddValueInternal<T>(T value, bool enclose = false) {
            var contentValue = new ContentValue(enclose);
            if (value != null) {
                contentValue.Value = value;
            }
            AddContentValue(contentValue);
        }

        /// <summary>
        /// Adds a <see cref="ContentValue"/> to the <see cref="Values"/> collection.
        /// </summary>
        /// <param name="value"> <see cref="ContentValue"/> to be added to the <see cref="Values"/> collection.</param>
        public void AddContentValue(ContentValue value) {
            Values.Add(value);
        }

        /// <summary>
        /// Retrieves the <see cref="ContentValue.Value"/> from the <see cref="ContentLine"/> at the given <see cref="index"/> formatted as a string.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetValueAsStringAt(int index) {
            return GetContentValueAt(index)?.GetStringValue();
        }

        /// <summary>
        /// Retrieves the <see cref="object"/> <see cref="ContentValue.Value"/> from the <see cref="ContentLine"/> at the given <see cref="index"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object GetValueAt(int index) {
            return GetContentValueAt(index)?.Value;
        }


        /// <summary>
        /// Retrieves the <see cref="ContentLine"/> at the given <see cref="index"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ContentValue GetContentValueAt(int index) {
            return index.LiesBetween(0, Values.Count - 1) ? Values[index] : null;
        }

        public ContentLine Copy() {
            return new ContentLine {
                Values = Values.Select(v => v.Copy()).ToList(),
            };
        }

        private string DebuggerDisplay => $"{Values.Count} values";
    }
}