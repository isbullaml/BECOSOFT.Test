using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BECOSOFT.Data.Models.IO {
    
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FtpFile {
        public string LocalFile { get; set; }
        public string ServerFile { get; set; }
        public string MoveToFile { get; set; }

        public string ServerFolder {
            get {
                if (ServerFile == null) { return null; }
                var split = ServerFile.Split('/');
                if (Path.HasExtension(split[split.Length - 1])) {
                    return string.Join("/", split.Take(split.Length - 1));
                }
                return string.Join("/", split);
            }
        }

        public string MoveToFolder {
            get {
                if (MoveToFile == null) { return null; }
                var split = MoveToFile.Split('/');
                if (Path.HasExtension(split[split.Length - 1])) {
                    return string.Join("/", split.Take(split.Length - 1));
                }
                return string.Join("/", split);
            }
        }

        public static FtpFile From(FtpFile folder, FtpListEntry listEntry, string localPath) {
            if (folder == null) {
                throw new ArgumentNullException(nameof(folder));
            }
            if (listEntry == null) {
                throw new ArgumentNullException(nameof(listEntry));
            }
            var directory = folder.ServerFile;
            if (!directory.EndsWith("/")) {
                directory += "/";
            }
            return new FtpFile {
                LocalFile = Path.Combine(localPath, listEntry.Name),
                ServerFile = directory + listEntry.Name,
            };
        }
        
        private string DebuggerDisplay => $"LocalFile: '{LocalFile}', ServerFile: '{ServerFile}', MoveToFile: '{MoveToFile}'";
    }
}