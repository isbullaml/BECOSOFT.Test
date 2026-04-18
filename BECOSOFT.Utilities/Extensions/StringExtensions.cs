using BECOSOFT.Utilities.Annotations;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BECOSOFT.Utilities.Extensions {
    /// <summary>
    /// Extensions for the <see cref="string"/>-class
    /// </summary>
    public static class StringExtensions {
        private const string HtmlTagPattern = "<.*?>";
        private const string ExtraSpacesPattern = @"\s+";
        private static readonly Regex HtmlRemoveRegex = new Regex(HtmlTagPattern, RegexOptions.Compiled);
        private static readonly Regex ExtraSpacesRemoveRegex = new Regex(ExtraSpacesPattern, RegexOptions.Compiled);

        /// <summary>Replaces the format item in a specified string with the string representation of a corresponding object in a specified array.</summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format. </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="format" /> or <paramref name="args" /> is null. </exception>
        /// <exception cref="T:System.FormatException">
        /// <paramref name="format" /> is invalid.-or- The index of a format item is less than zero, or greater than or equal to the length of the <paramref name="args" /> array. </exception>
        /// <returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representation of the corresponding objects in <paramref name="args" />.</returns>
        [StringFormatMethod("format")]
        public static string FormatWith(this string format, params object[] args) {
            return string.Format(format, args);
        }

        /// <summary>Indicates whether the specified string is not null and not an <see cref="F:System.String.Empty" /> string.</summary>
        /// <param name="value">The string to test. </param>
        /// <returns>true if the <paramref name="value" /> parameter is not null and not an empty string (""); otherwise, false.</returns>
        [ContractAnnotation("value:null => false")]
        public static bool HasValue(this string value) {
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>Indicates whether the specified string is not null and not an <see cref="F:System.String.Empty" /> string and does not consist of only white-space characters.</summary>
        /// <param name="value">The string to test.</param>
        /// <returns>true if the <paramref name="value" /> parameter is not null and not an empty string ("") and does not consist of only white-space characters; otherwise, false.</returns>
        [ContractAnnotation("value:null => false")]
        public static bool HasNonWhiteSpaceValue(this string value) {
            return !string.IsNullOrWhiteSpace(value);
        }

        /// <summary>Indicates whether the specified string is null or an <see cref="F:System.String.Empty" /> string.</summary>
        /// <param name="value">The string to test. </param>
        /// <returns>true if the <paramref name="value" /> parameter is null or an empty string (""); otherwise, false.</returns>
        [ContractAnnotation("value:null => true")]
        public static bool IsNullOrEmpty(this string value) {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>Indicates whether a specified string is null, empty, or consists only of white-space characters.</summary>
        /// <param name="value">The string to test.</param>
        /// <returns>true if the <paramref name="value" /> parameter is null or <see cref="F:System.String.Empty" />, or if <paramref name="value" /> consists exclusively of white-space characters.</returns>
        [ContractAnnotation("value:null => true")]
        public static bool IsNullOrWhiteSpace(this string value) {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Split a string based on a <see cref="string"/> separator.
        /// </summary>
        /// <param name="value">Value to split</param>
        /// <param name="separator">Separator used to split the provided value</param>
        /// <returns>Returns the split result.</returns>
        public static string[] Split(this string value, string separator) {
            if (value == null) {
                return Array.Empty<string>();
            }
            return separator == null ? new[] { value } : value.Split(new[] { separator }, StringSplitOptions.None);
        }

        /// <summary>
        /// Removes all control characters (0x00) from a string.
        /// </summary>
        /// <param name="value">Value to remove white space for.</param>
        /// <returns>A string with all white spaces removed</returns>
        public static string RemoveControlCharacters(this string value) {
            if (value.IsNullOrEmpty()) {
                return string.Empty;
            }
            var newString = new StringBuilder();
            foreach (var ch in value) {
                if (!char.IsControl(ch)) {
                    newString.Append(ch);
                }
            }
            return newString.ToString();
        }

        /// <summary>
        /// Checks whether the provided <paramref name="value"/> contains a character of the specified <paramref name="category"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public static bool Contains(this string value, UnicodeCategory category) {
            if (value.IsNullOrEmpty()) {
                return false;
            }
            return value.Any(c => char.GetUnicodeCategory(c) == category);
        }

        /// <summary>
        /// Removes all white spaces from a string.
        /// </summary>
        /// <param name="value">Value to remove white space for.</param>
        /// <returns>A string with all white spaces removed</returns>
        public static string RemoveWhitespace(this string value) {
            return value.IsNullOrEmpty() ? string.Empty : string.Join("", value.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Splits a <see cref="string"/> into a <see cref="List{T}"/> using the provided <see cref="splitter"/> (default: ',').
        /// If the provided <see cref="value"/> is null or empty, an empty <see cref="List{T}"/> is returned.
        /// </summary>
        /// <typeparam name="T">Type of the resulting <see cref="List{T}"/>. Split elements are converted to this type.</typeparam>
        /// <param name="value">The string to split.</param>
        /// <param name="splitter">Character to split the string on. By default the string is split on ','.</param>
        /// <returns>A list containing the split elements. If the provided <see cref="value"/> is null or empty, the result will be an empty <see cref="List{T}"/>.</returns>
        public static List<T> ToSplitList<T>(this string value, char splitter = ',') {
            if (value.IsNullOrEmpty()) {
                return new List<T>(0);
            }
            var split = value.Split(splitter);
            var converter = Converter.GetDelegate(typeof(T));
            return split.Select(s => (T) converter(s)).ToList();
        }

        /// <summary>
        /// Splits a <see cref="string"/> into a <see cref="List{T}"/> using the provided <see cref="splitter"/> string.
        /// </summary>
        /// <typeparam name="T">Type of the resulting <see cref="List{T}"/>. Split elements are converted to this type.</typeparam>
        /// <param name="value">The string to split.</param>
        /// <param name="splitter">String to split the string on.</param>
        /// <returns>A list containing the split elements.</returns>
        public static List<T> ToSplitList<T>(this string value, string splitter) {
            if (value.IsNullOrEmpty()) {
                return new List<T>(0);
            }
            var split = value.Split(splitter);
            var converter = Converter.GetDelegate(typeof(T));
            return split.Select(s => (T) converter(s)).ToList();
        }

        /// <summary>
        /// Splits a <see cref="string"/> into a <see cref="List{T}"/> of <see cref="string"/> of (alpha-)numeric parts.
        /// Note: Control characters are considered alphanumeric.
        /// </summary>
        /// <param name="value">The string to split.</param>
        /// <returns></returns>
        public static List<string> ToAlphanumericallySplitList(this string value) {
            if (value.IsNullOrEmpty()) {
                return new List<string>(0);
            }
            var sb = new StringBuilder();
            var result = new List<string>();
            var isNumeric = false;
            for (var i = 0; i < value.Length; i++) {
                var c = value[i];
                var isDigit = char.IsDigit(c);
                if (i != 0 && isNumeric != isDigit) {
                    result.Add(sb.ToString());
                    sb.Clear();
                }
                sb.Append(c);
                isNumeric = isDigit;
            }
            if (sb.Length > 0) {
                result.Add(sb.ToString());
            }
            return result;
        }

        /// <summary>
        /// Truncates a string if needed and appends the trailing string. The max length of the string includes the trailing string length.
        /// </summary>
        /// <param name="value">String to truncate</param>
        /// <param name="maxLength">Maximum length of the string.</param>
        /// <param name="trailing">Trailing string that gets appended when a truncate occurs.</param>
        /// <returns>A truncated string</returns>
        public static string Truncate(this string value, int maxLength, string trailing = "...") {
            if (value.IsNullOrEmpty()) {
                return string.Empty;
            }
            var newLength = Math.Max(maxLength, 0) - (trailing?.Length ?? 0);
            if (value.Length <= newLength || newLength < 0) {
                return value;
            }
            return value.Substring(0, newLength) + (trailing ?? "");
        }

        /// <summary>
        /// Returns a string containing a specified number of characters from the left side of a string.
        /// </summary>
        /// <param name="value">String expression from which the leftmost characters are returned.</param>
        /// <param name="length">Indicates how many characters to return. If greater than or equal to the number of characters in <paramref name="value" />, the entire string is returned.</param>
        /// <returns>Returns a string containing a specified number of characters from the left side of a string.</returns>
        public static string Left(this string value, int length) {
            return value.Truncate(length, trailing: null);
        }

        /// <summary>
        /// Returns a string containing a specified number of characters from the right side of a string.
        /// </summary>
        /// <param name="value">String  expression from which the rightmost characters are returned.</param>
        /// <param name="length">Indicates how many characters to return. If greater than or equal to the number of characters in <paramref name="value" />, the entire string is returned.</param>
        /// <returns>Returns a string containing a specified number of characters from the right side of a string.</returns>
        public static string Right(this string value, int length) {
            if (value.IsNullOrEmpty()) {
                return string.Empty;
            }
            var newLength = Math.Max(length, 0);
            var valueLength = value.Length;
            if (valueLength <= newLength || newLength < 0) {
                return value;
            }
            return value.Substring(valueLength - newLength, newLength);
        }

        /// <summary>
        /// Checks if a string is equals to another string while ignoring case
        /// </summary>
        /// <param name="value">The original string</param>
        /// <param name="toCompare">The string to compare to</param>
        /// <returns>Value indicating whether the strings are equal</returns>
        public static bool EqualsIgnoreCase(this string value, string toCompare) {
            return string.Equals(value, toCompare, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Returns <see langword="null"/> if <see cref="first"/> equals <see cref="second"/>, otherwise <see cref="first"/> is returned. 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static string NullIf(this string first, string second) {
            if (string.Equals(first, second)) {
                return null;
            }
            return first;
        }

        /// <summary>
        /// Returns <see langword="null"/> if <see cref="first"/> equals <see cref="second"/>, otherwise <see cref="first"/> is returned. 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static char? NullIf(this char first, char second) {
            if (first == second) {
                return null;
            }
            return first;
        }

        /// <summary>
        /// Returns a new string in which all occurrences of the specified <paramref name="oldValues"/> with the <paramref name="replacementValue"/>.
        /// </summary>
        /// <param name="value">The original string</param>
        /// <param name="oldValues">The string values to replace within the <paramref name="value"/>.</param>
        /// <param name="replacementValue"></param>
        /// <returns></returns>
        public static string ReplaceMany(this string value, IEnumerable<string> oldValues, string replacementValue = "") {
            if (value.IsNullOrEmpty()) { return value; }
            var oldValueList = oldValues.ToSafeList().ToArray();
            var split = value.Split(oldValueList, StringSplitOptions.None);
            return string.Join(replacementValue, split);
        }

        /// <summary>
        /// Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string ignoring the casing of the string to find.
        /// </summary>
        /// <param name="value">The original string</param>
        /// <param name="oldValue">The string to be replaced.</param>
        /// <param name="newValue">The string to replace all occurrences of <paramref name="oldValue" />.</param>
        /// <returns>A string that is equivalent to the current string except that all instances of <paramref name="oldValue" /> are replaced with <paramref name="newValue" />. If <paramref name="oldValue" /> is not found in the current instance, the method returns the current instance unchanged. </returns>
        public static string ReplaceIgnoreCase(this string value, string oldValue, string newValue) {
            if (value.IsNullOrEmpty()) { return string.Empty; }
            var lowerCaseValue = value.ToLower();
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
            return builder.ToString();
        }

        /// <summary>
        /// Strips the HTML-tags of a string
        /// </summary>
        /// <param name="value">The string to strip</param>
        /// <returns>The stripped string</returns>
        public static string StripHtmlTags(this string value) {
            return value.IsNullOrEmpty() ? value : HtmlRemoveRegex.Replace(value, string.Empty);
        }

        /// <summary>
        /// Strips the XML-tags (&lt;, &gt; and &amp;) of a string
        /// </summary>
        /// <param name="value">The string to strip</param>
        /// <param name="replacementValue">Value to replace the XML-characters with</param>
        /// <returns>The stripped string</returns>
        public static string StripXmlTags(this string value, string replacementValue = "") {
            return value.IsNullOrEmpty() ? value : value.ReplaceMany(new[] { "<", ">", "&" }, replacementValue);
        }

        /// <summary>
        /// Replaces all Enter-characters (NewLine, char 10 and char 13) with a replacement <see cref="string"/> value.
        /// </summary>
        /// <param name="value"><see cref="value"/> to escape.</param>
        /// <param name="replacementValue">Value to replace the Enter-characters with</param>
        /// <returns>Returns a new string with all Enter-characters replaced.</returns>
        public static string EscapeEnterCharacters(this string value, string replacementValue = "") {
            if (value.IsNullOrEmpty()) {
                return value;
            }
            return value.Replace(Environment.NewLine, replacementValue)
                        .Replace(((char) 13).ToString(), replacementValue)
                        .Replace(((char) 10).ToString(), replacementValue);
        }

        /// <summary>
        /// Returns a string where all accented characters are replaced with their normalized equivalent.
        /// </summary>
        /// <param name="value">The string that will be normalized</param>
        /// <returns>A new string with the accents removed</returns>
        public static string RemoveAccents(this string value) {
            var normalized = value.Normalize(NormalizationForm.FormKD);
            var removal = Encoding.GetEncoding(Encoding.ASCII.CodePage,
                                                    new EncoderReplacementFallback(""),
                                                    new DecoderReplacementFallback(""));
            var bytes = removal.GetBytes(normalized);
            return Encoding.ASCII.GetString(bytes);
        }

        /// <summary>
        /// Checks if a string contains a substring
        /// </summary>
        /// <param name="value">The string to check</param>
        /// <param name="toCheck">The substring</param>
        /// <param name="comparisonType">(Optional) The type of comparison</param>
        /// <returns>Value indicating whether the substring is in in the original string</returns>
        public static bool Contains(this string value, string toCheck, StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase) {
            if (value == null || toCheck == null) { return false; }
            return value.IndexOf(toCheck, comparisonType) >= 0;
        }

        /// <summary>
        /// Removes all the duplicate spaces from a string
        /// </summary>
        /// <param name="value">The string to remove the spaces from</param>
        /// <returns>The string without the duplicate spaces</returns>
        public static string RemoveDuplicateSpaces(this string value) {
            return value.IsNullOrEmpty() ? value : ExtraSpacesRemoveRegex.Replace(value, " ");
        }

        /// <summary>
        /// SQL-escape a string
        /// </summary>
        /// <param name="value">The string to escape</param>
        /// <returns>The escaped string</returns>
        public static string SqlEscape(this string value) {
            return value.Replace("'", "''");
        }

        /// <summary>
        /// Change the casing of the characters of the provided string using the provided <see cref="TextCasing"/> value.
        /// </summary>
        /// <param name="value">The string that will be changed</param>
        /// <param name="casing">The casing to apply</param>
        /// <returns>Returns the result string with the casing applied.</returns>
        public static string ToCasing(this string value, TextCasing casing) {
            if (value.IsNullOrEmpty()) {
                return value;
            }
            string result;
            switch (casing) {
                case TextCasing.Lower:
                    result = value.ToLower();
                    break;
                case TextCasing.Upper:
                    result = value.ToUpper();
                    break;
                case TextCasing.LowerInvariantCulture:
                    result = value.ToLowerInvariant();
                    break;
                case TextCasing.UpperInvariantCulture:
                    result = value.ToUpperInvariant();
                    break;
                case TextCasing.TitleCase:
                    result = value.ToTitleCase();
                    break;
                case TextCasing.TitleCaseInvariantCulture:
                    result = value.ToTitleCaseInvariant();
                    break;
                default:
                    result = value;
                    break;
            }
            return result;
        }

        /// <summary>
        /// Converts a string to title case using <see cref="CultureInfo.CurrentCulture"/>. The string first gets converted to lower case before changing to title case.
        /// </summary>
        /// <param name="value">Value to convert to title case</param>
        /// <returns>The string converted to title case.</returns>
        public static string ToTitleCase(this string value) {
            return value.ToTitleCase(CultureInfo.CurrentCulture);
        }

        public static string CleanFileName(this string fileName) {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        /// <summary>
        /// Converts a string to title case using <see cref="CultureInfo.InvariantCulture"/>. The string first gets converted to lower case before changing to title case.
        /// </summary>
        /// <param name="value">Value to convert to title case</param>
        /// <returns>The string converted to invariant title case.</returns>
        public static string ToTitleCaseInvariant(this string value) {
            return value.ToTitleCase(CultureInfo.InvariantCulture);
        }

        private static string ToTitleCase(this string value, CultureInfo cultureInfo) {
            return value.IsNullOrEmpty() ? value : cultureInfo.TextInfo.ToTitleCase(value.ToLower());
        }

        /// <summary>
        /// Returns the string with <see cref="toInsert"/> inserted at <see cref="every"/> count of characters.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="toInsert"></param>
        /// <param name="every"></param>
        /// <returns></returns>
        public static string InsertStringAtEveryCount(this string value, string toInsert, int every) {
            if (value.IsNullOrWhiteSpace()) { return value; }
            if (value.Length < every || every == 0) { return value; }
            if (toInsert.IsNullOrEmpty()) { return value; }
            var sb = new StringBuilder();
            foreach (var partition in value.Partition(every)) {
                var charList = partition.ToList();
                foreach (var c in charList) {
                    sb.Append(c);
                }
                if (charList.Count == every) {
                    sb.Append(toInsert);
                }
            }
            return sb.ToString();
        }
    }
}
