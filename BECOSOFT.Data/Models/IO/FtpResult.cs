using System.Collections.Generic;

namespace BECOSOFT.Data.Models.IO {
    public class FtpResult : IFtpResult<FtpFileResult> {
        public List<FtpFileResult> Results { get; } = new List<FtpFileResult>();
    }
}