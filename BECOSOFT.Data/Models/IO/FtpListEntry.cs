using System;
using System.Diagnostics;

namespace BECOSOFT.Data.Models.IO {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FtpListEntry {
        public string Name { get; set; }
        public bool IsDirectory { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }

        public FtpListEntry(string name) {
            Name = name;
        }

        private string DebuggerDisplay => IsDirectory ? $"{Name} (dir)" : $"{Name} ({Size}b)";
    }
}
