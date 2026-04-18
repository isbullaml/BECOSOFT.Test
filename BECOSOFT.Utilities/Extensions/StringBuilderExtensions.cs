using BECOSOFT.Utilities.Annotations;
using System;
using System.Text;

namespace BECOSOFT.Utilities.Extensions {
    /// <summary>
    /// Extensions for the <see cref="StringBuilder"/>-class
    /// </summary>
    public static class StringBuilderExtensions {
        /// <summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter array.</summary>
        /// <returns>A reference to this instance with <paramref name="format" /> appended. Each format item in <paramref name="format" /> is replaced by the string representation of the corresponding object argument.</returns>
        /// <param name="builder">A builder instance </param>
        /// <param name="format">A composite format string (see Remarks). </param>
        /// <param name="args">An array of objects to format. </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="format" /> or <paramref name="args" /> is null. </exception>
        /// <exception cref="T:System.FormatException">
        /// <paramref name="format" /> is invalid. -or-The index of a format item is less than 0 (zero), or greater than or equal to the length of the <paramref name="args" /> array.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />. </exception>
        [StringFormatMethod("format")]
        public static StringBuilder Append(this StringBuilder builder, string format, params object[] args) {
            return builder.AppendFormat(format, args);
        }

        [StringFormatMethod("format")]
        public static StringBuilder AppendLine(this StringBuilder builder, string format, params object[] args) {
            return builder.AppendFormat(format, args).AppendLine();
        }

        /// <summary>
        /// Replaces all Enter-characters (NewLine, char 10 and char 13) with an empty <see cref="string"/>.
        /// </summary>
        /// <param name="value"><see cref="value"/> to escape.</param>
        /// <param name="replacementValue">Value to replace the Enter-characters with</param>
        /// <returns>Returns the same <see cref="StringBuilder"/> with all Enter-characters replaced.</returns>
        public static StringBuilder EscapeEnterCharacters(this StringBuilder value, string replacementValue = "") {
            return value.Replace(Environment.NewLine, replacementValue)
                .Replace(((char) 13).ToString(), replacementValue)
                .Replace(((char) 10).ToString(), replacementValue);
        }

        public static StringBuilder SqlEscape(this StringBuilder value) {
            return value.Replace("'", "''");
        }

        public static bool HasValue(this StringBuilder value) {
            return (value?.Length ?? 0) != 0;
        }

        public static bool IsEmpty(this StringBuilder value) {
            return (value?.Length ?? 0) == 0;
        }

        /// <summary>
        /// Removes all leading and trailing white-space characters from the current <see cref="T:System.String" /> object.
        /// </summary>
        /// <returns>The string that remains after all white-space characters are removed from the start and end of the current string. If no characters can be trimmed from the current instance, the method returns the current instance unchanged. </returns>
        public static StringBuilder Trim(this StringBuilder value) => value.TrimHelper(TrimType.Both);

        /// <summary>
        /// Removes all leading white-space characters from the current <see cref="T:System.String" /> object.
        /// </summary>
        /// <returns>The string that remains after all white-space characters are removed from the start of the current string. If no characters can be trimmed from the current instance, the method returns the current instance unchanged. </returns>
        public static StringBuilder TrimStart(this StringBuilder value) => value.TrimHelper(TrimType.Start);

        /// <summary>
        /// Removes all trailing white-space characters from the current <see cref="T:System.String" /> object.
        /// </summary>
        /// <returns>The string that remains after all white-space characters are removed from the end of the current string. If no characters can be trimmed from the current instance, the method returns the current instance unchanged. </returns>
        public static StringBuilder TrimEnd(this StringBuilder value) => value.TrimHelper(TrimType.End);

        private static StringBuilder TrimHelper(this StringBuilder value, TrimType trimType) {
            if (value == null || value.Length == 0) { return value; }
            var end = value.Length - 1;
            var start = 0;
            if (trimType != TrimType.End) {
                while (start < value.Length && char.IsWhiteSpace(value[start])) {
                    ++start;
                }
            }
            if (trimType != TrimType.Start) {
                while (end >= start && char.IsWhiteSpace(value[end])) {
                    --end;
                }
            }
            return value.CreateTrimmedString(start, end);
        }

        private static StringBuilder CreateTrimmedString(this StringBuilder sb, int start, int end) {
            var length = end - start + 1;
            if (length == sb.Length) {
                return sb;
            }
            if (length <= 0) {
                sb.Length = 0;
                return sb;
            }
            sb.Length = end + 1;
            sb.Remove(0, start);
            return sb;
        }

        /// <summary>
        /// Creates a new <see cref="StringBuilder"/> containing a substring from the given instance. The substring starts at a specified character position and has a specified length.
        /// </summary>
        /// <param name="value">The instance to take the substring from.</param>
        /// <param name="startIndex">The zero-based starting character position of a substring in this instance. </param>
        /// <param name="length">The number of characters in the substring. </param>
        /// <returns>A <see cref="StringBuilder"/>-instance containing the substring of length <paramref name="length" /> that begins at <paramref name="startIndex" /> in this instance, or <see cref="F:System.String.Empty" /> if <paramref name="startIndex" /> is equal to the length of this instance and <paramref name="length" /> is zero.</returns>
        public static StringBuilder Substring(this StringBuilder value, int startIndex, int length) {
            if (value == null || value.Length == 0) { return value; }
            if (startIndex + length - 1 >= value.Length || startIndex < 0) {
                throw new ArgumentOutOfRangeException();
            }
            var endIndex = startIndex + length;
            var subString = new StringBuilder(length);
            for (var i = startIndex; i < endIndex; i++) {
                subString.Append(value[i]);
            }
            return subString;
        }

        /// <summary>
        /// Returns a new string in which all occurrences of a specified <see cref="StringBuilder"/> in the current instance are replaced with another specified string ignoring the casing of the string to find.
        /// </summary>
        /// <param name="value">The original <see cref="StringBuilder"/></param>
        /// <param name="oldValue">The string to be replaced.</param>
        /// <param name="newValue">The string to replace all occurrences of <paramref name="oldValue" />.</param>
        /// <returns>Returns the provided <see cref="StringBuilder"/> except that all instances of <paramref name="oldValue" /> are replaced with <paramref name="newValue" />. If <paramref name="oldValue" /> is not found in the current instance, the method returns the current instance unchanged. </returns>
        public static StringBuilder ReplaceIgnoreCase(this StringBuilder value, string oldValue, string newValue) {
            if (value == null || value.Length == 0) { return value; }
            var lowerCaseValue = value.ToString().ToLower();
            var lowerCaseOldValue = oldValue.ToLower();

            var splitValue = lowerCaseValue.Split(lowerCaseOldValue);
            var splitValueLength = splitValue.Length;
            if (splitValueLength == 1) {
                return value;
            }
            var builder = new StringBuilder();
            var index = 0;
            var oldValueLength = oldValue.Length;
            for (var i = 0; i < splitValueLength; i++) {
                var part = splitValue[i];
                var partLength = part.Length;
                for (var valueIndex = index; valueIndex < index + partLength; valueIndex++) {
                    var c = value[valueIndex];
                    builder.Append(c);
                }

                index += partLength + oldValueLength;
                if (i == splitValueLength - 1) {
                    continue;
                }
                builder.Append(newValue);
            }
            value.Clear();
            value.Append(builder);
            return value;
        }

        private enum TrimType {
            Start,
            End,
            Both,
        }
    }
}