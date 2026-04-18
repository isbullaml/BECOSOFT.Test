using BECOSOFT.Utilities.Algorithms;
using System;
using System.Collections.Generic;
using System.Text;

namespace BECOSOFT.Utilities.Helpers {
    public static class StringHelper {
        /// <summary>
        /// The set of characters that are unreserved in RFC 2396 but are NOT unreserved in RFC 3986.
        /// </summary>
        // Source: https://stackoverflow.com/questions/846487/how-to-get-uri-escapedatastring-to-comply-with-rfc-3986
        private static readonly string[] UriRfc3986CharsToEscape = new[] { "!", "*", "'", "(", ")" };

        /// <summary>
        /// Escapes a string according to the URI data string rules given in RFC 3986.
        /// </summary>
        /// <param name="value">The value to escape.</param>
        /// <returns>The escaped value.</returns>
        /// <remarks>
        /// The <see cref="Uri.EscapeDataString"/> method is <i>supposed</i> to take on
        /// RFC 3986 behavior if certain elements are present in a .config file.  Even if this
        /// actually worked (which in my experiments it <i>doesn't</i>), we can't rely on every
        /// host actually having this configuration element present.
        /// </remarks>
        public static string EscapeUriDataStringRfc3986(string value) {
            // Start with RFC 2396 escaping by calling the .NET method to do the work.
            // This MAY sometimes exhibit RFC 3986 behavior (according to the documentation).
            // If it does, the escaping we do that follows it will be a no-op since the
            // characters we search for to replace can't possibly exist in the string.
            var escaped = new StringBuilder(Uri.EscapeDataString(value));

            // Upgrade the escaping to RFC 3986, if necessary.
            foreach (var toEscape in UriRfc3986CharsToEscape) {
                escaped.Replace(toEscape, Uri.HexEscape(toEscape[0]));
            }

            // Return the fully-RFC3986-escaped string.
            return escaped.ToString();
        }

        public static string GetBestMatchingString(string toMatch, IEnumerable<string> values, bool ignoreCase = false) {
            string bestMatch = null;
            var distance = int.MaxValue;
            foreach (var value in values) {
                var newDistance = LevenshteinDistance.Calculate(value, toMatch, ignoreCase);
                if (newDistance >= distance) { continue; }
                distance = newDistance;
                bestMatch = value;
                if (distance == 0) {
                    return bestMatch;
                }
            }
            return bestMatch;
        }
    }
}
