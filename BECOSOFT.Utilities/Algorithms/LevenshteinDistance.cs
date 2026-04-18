using BECOSOFT.Utilities.Extensions;
using System;

namespace BECOSOFT.Utilities.Algorithms {
    /// <summary>
    /// The Levenshtein distance is a string metric for measuring the difference between two sequences.
    /// Informally, the Levenshtein distance between two words is the minimum number of single-character edits (insertions, deletions or substitutions) required to change one word into the other.
    /// <remarks>
    /// Source: <a href="https://en.wikibooks.org/wiki/Algorithm_Implementation/Strings/Levenshtein_distance#C.23">Implementation</a>
    /// Source: <a href="https://en.wikipedia.org/wiki/Levenshtein_distance">Algorithm</a>
    /// </remarks>
    /// </summary>
    public static class LevenshteinDistance {
        /// <summary>
        /// Calculate the difference between 2 strings using the Levenshtein distance algorithm
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Calculate(string a, string b, bool ignoreCase = false) {
            if (a.IsNullOrEmpty()) {
                return b?.Length ?? 0;
            }
            if (b.IsNullOrEmpty()) {
                return a.Length;
            }
            var source = ignoreCase ? a.ToLower() : a;
            var target = ignoreCase ? b.ToLower() : b;

            if (source.Length > target.Length) {
                var temp = target;
                target = source;
                source = temp;
            }

            var m = target.Length;
            var n = source.Length;
            var distance = new int[2, m + 1];
            // Initialize the distance matrix
            for (var j = 1; j <= m; j++) {
                distance[0, j] = j;
            }

            var currentRow = 0;
            for (var i = 1; i <= n; ++i) {
                currentRow = i & 1;
                distance[currentRow, 0] = i;
                var previousRow = currentRow ^ 1;
                for (var j = 1; j <= m; j++) {
                    var cost = (target[j - 1] == source[i - 1] ? 0 : 1);
                    distance[currentRow, j] = Math.Min(Math.Min(distance[previousRow, j] + 1, distance[currentRow, j - 1] + 1), distance[previousRow, j - 1] + cost);
                }
            }
            return distance[currentRow, m];
        }
    }
}