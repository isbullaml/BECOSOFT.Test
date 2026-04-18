using NLog;
using System;
using System.IO;
using System.Security;
using System.Security.Permissions;

namespace BECOSOFT.Utilities.Helpers.IO {
    public static class DirectoryHelper {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Checks if the process can write to the specified <paramref name="directory"/>.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static bool HasPermission(string directory) {
            var directoryInfo = new DirectoryInfo(directory);
            if (!directoryInfo.Exists) {
                return false;
            }
            try {
                var writePermission = new FileIOPermission(FileIOPermissionAccess.Write, directoryInfo.FullName);
                writePermission.Demand();
                return true;
            } catch (SecurityException e) {
                Logger.Error(e);
                return false;
            } catch (Exception e) {
                Logger.Error(e);
                return false;
            }
        }
    }
}
