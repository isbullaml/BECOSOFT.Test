using System.Collections.Generic;

namespace BECOSOFT.Data.Models.IO {
    public interface IFtpResult<T> where T : FtpFileResult {
        List<T> Results { get;}
    }
}