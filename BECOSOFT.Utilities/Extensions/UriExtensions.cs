using BECOSOFT.Utilities.Collections;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Extensions {
    /// <summary>
    /// Extension for a <see cref="Uri"/>
    /// </summary>
    public static class UriExtensions {
        /// <summary>
        /// Append the paths to the given <see cref="Uri"/>. 
        /// </summary>
        /// <param name="uri">The Uri</param>
        /// <param name="allowEmptyPaths">Allow or dissallow empty strings in <see cref="paths"/></param>
        /// <param name="paths">The paths to append</param>
        /// <returns>The Uri with the paths appended</returns>
        public static Uri Append(this Uri uri, bool allowEmptyPaths, params string[] paths) {
            if (paths.IsEmpty()) {
                return uri;
            }
            var nonEmptyPaths = paths.Where(p => p != null && (allowEmptyPaths || !p.IsNullOrWhiteSpace())).ToList();
            var result = new Uri(nonEmptyPaths.Aggregate(uri.AbsoluteUri, PathAppender()));
            return result;
        }

        /// <summary>
        /// Append the paths to the given <see cref="Uri"/>. Empty paths are filtered.
        /// </summary>
        /// <param name="uri">The Uri</param>
        /// <param name="paths">The paths to append</param>
        /// <returns>The Uri with the paths appended</returns>
        public static Uri Append(this Uri uri, params string[] paths) {
            return Append(uri, false, paths);
        }

        /// <summary>
        /// Append the query-parts to the given <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">The Uri</param>
        /// <param name="queryParts">The query-parts to append</param>
        /// <returns>The Uri with the query-parts appended</returns>
        public static Uri AppendQueryPart(this Uri uri, KeyValueList<string, string> queryParts) {
            return AppendQueryPart(uri, queryParts?.ToArray());
        }

        /// <summary>
        /// Append key and value as query part to the given <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">The Uri</param>
        /// <param name="key">Key of the query part</param>
        /// <param name="value">Value of the query part</param>
        /// <returns>The Uri with key and value as query part appended</returns>
        public static Uri AppendQueryPart(this Uri uri, string key, string value) {
            return AppendQueryPart(uri, KeyValuePair.Create(key, value));
        }

        /// <summary>
        /// Append the query-parts to the given <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">The Uri</param>
        /// <param name="queryParts">The query-parts to append</param>
        /// <returns>The Uri with the query-parts appended</returns>
        public static Uri AppendQueryPart(this Uri uri, params KeyValuePair<string, string>[] queryParts) {
            if (queryParts.IsEmpty()) {
                return uri;
            }

            var nonEmptyParts = queryParts.Where(p => !p.Key.IsNullOrWhiteSpace() && !p.Value.IsNullOrWhiteSpace()).Select(p => $"{p.Key}={p.Value}").ToList();
            var result = new Uri(nonEmptyParts.Aggregate(uri.AbsoluteUri, QueryPartAppender()));
            return result;
        }

        /// <summary>
        /// Parses the query parameters from a <see cref="Uri"/>
        /// </summary>
        /// <remarks>
        /// Source: https://stackoverflow.com/a/20134983/4182837
        /// </remarks>
        /// <param name="uri"></param>
        /// <returns>The query parameters as a dictionary</returns>
        public static Dictionary<string, string> DecodeQueryParameters(this Uri uri) {
            if (uri == null) {
                throw new ArgumentNullException(nameof(uri));
            }

            if (uri.Query.Length == 0) {
                return new Dictionary<string, string>();
            }

            return uri.Query.TrimStart('?')
                      .Split(new[] { '&', ';' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(parameter => parameter.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries))
                      .GroupBy(parts => parts[0], parts => parts.Length > 2 ? string.Join("=", parts, 1, parts.Length - 1) : (parts.Length > 1 ? parts[1] : ""))
                      .ToDictionary(grouping => grouping.Key, grouping => string.Join(",", grouping));
        }

        private static Func<string, string, string> PathAppender() {
            return (current, path) =>
                $"{current.TrimEnd('/')}/{path.TrimStart('/')}";
        }

        private static Func<string, string, string> QueryPartAppender() {
            return (current, queryPart) =>
                current.Contains("?") ? $"{current}&{queryPart}" : $"{current}?{queryPart}";
        }
    }
}