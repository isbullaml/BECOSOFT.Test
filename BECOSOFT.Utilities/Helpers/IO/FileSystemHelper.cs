using BECOSOFT.Utilities.Extensions;
using System;
using System.IO;
using System.Reflection;

namespace BECOSOFT.Utilities.Helpers.IO {
    /// <summary>
    /// Helper class for the file-system
    /// </summary>
    public static class FileSystemHelper {
        /// <summary>
        /// Gets the local path of the executing assembly
        /// </summary>
        /// <returns>The path as a string</returns>
        public static string GetExecutingAssemblyLocalPath() {
            var baseDir = Assembly.GetExecutingAssembly().CodeBase;
            return new Uri(baseDir).LocalPath;
        }

        /// <summary>
        /// Gets the folder of the executing assembly
        /// </summary>
        /// <returns>The folder-info</returns>
        public static DirectoryInfo GetExecutingAssemblyFolder() {
            var localPath = GetExecutingAssemblyLocalPath();
            var directory = Path.GetDirectoryName(localPath);
            return directory == null ? new DirectoryInfo(localPath) : new DirectoryInfo(directory);
        }

        /// <summary>
        /// Gets the root folder of the executing assembly
        /// </summary>
        /// <returns>The root folder-info</returns>
        public static DirectoryInfo GetExecutingAssemblyRootFolder() {
            var assemblyFolder = GetExecutingAssemblyFolder();
            return assemblyFolder.Name.EqualsIgnoreCase("bin") ? assemblyFolder.Parent : assemblyFolder;
        }
    }
}