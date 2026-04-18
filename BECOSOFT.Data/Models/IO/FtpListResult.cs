using System.Collections.Generic;
using System.Diagnostics;

namespace BECOSOFT.Data.Models.IO {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FtpListResult : IFtpResult<FtpFileListResult> {
        public List<FtpFileListResult> Results { get; } = new List<FtpFileListResult>();

        private string DebuggerDisplay => $"Contains {Results.Count} results";
    }
}