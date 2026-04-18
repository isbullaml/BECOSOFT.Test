using BECOSOFT.Utilities.Extensions;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;

namespace BECOSOFT.Utilities.Helpers.IO {
    /// <summary>
    /// Helper class for files
    /// </summary>
    public static class FileHelpers {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Cleans the invalid characters from a filename
        /// </summary>
        /// <param name="fileName">The filename</param>
        /// <returns>The cleaned filename</returns>
        public static string CleanFileName(string fileName) {
            return Path.GetInvalidFileNameChars()
                       .Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        public static void SetFullControl(string file) {
            if (file.IsNullOrWhiteSpace()) { return; }
            if (!File.Exists(file)) { return; }
            try {
                var fileInfo = new FileInfo(file);
                var fileSecurity = fileInfo.GetAccessControl();
                var userAccount = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                fileSecurity.AddAccessRule(new FileSystemAccessRule(userAccount, FileSystemRights.FullControl, AccessControlType.Allow));
                fileInfo.SetAccessControl(fileSecurity);
            } catch (Exception e) {
                Logger.Error(e, "Failed to set access control");
            }
            try {
                File.SetAttributes(file, FileAttributes.Normal);
            } catch (Exception e) {
                Logger.Error(e, "Failed to set file attributes");
            }
        }
    }
}