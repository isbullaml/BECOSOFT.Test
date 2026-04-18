using System;
using System.IO;

namespace BECOSOFT.Utilities.Helpers.IO {
    /// <summary>
    /// Helper class for <see cref="Path"/>-related functions
    /// </summary>
    public static class PathHelper {
        /// <summary>
        /// Returns the full path of a temporary file with the provided <see cref="extension"/>.
        /// </summary>
        /// <param name="extension">The extension that the resulting file name should have.</param>
        /// <returns>The full path of a temporary file with the provided <see cref="extension"/>.</returns>
        public static string GetTempFileName(string extension) {
            var path = Path.GetTempPath();
            var fileName = Path.ChangeExtension(Guid.NewGuid().ToString("D"), extension);
            return Path.Combine(path, fileName);
        }
    }
}