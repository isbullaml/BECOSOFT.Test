using System.Collections.Generic;
using System.Diagnostics;

namespace BECOSOFT.Data.Models.IO {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FtpFileListResult : FtpFileResult {
        public List<FtpListEntry> List { get; set; } = new List<FtpListEntry>();

        public FtpFileListResult(FtpFile file) : base(file) {
        }

        private string DebuggerDisplay => $"{File?.ServerFolder}: {List.Count} items";
    }
}