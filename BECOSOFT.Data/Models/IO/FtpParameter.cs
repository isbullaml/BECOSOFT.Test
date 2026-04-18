using System.Collections.Generic;

namespace BECOSOFT.Data.Models.IO {
    public abstract class FtpParameter {
        /// <summary>
        /// Internal container for <see cref="FtpFile"/> objects.
        /// </summary>
        protected List<FtpFile> FtpFiles;
        /// <summary>
        /// FTP-server connection properties
        /// </summary>
        public FtpServer Server { get; }

        /// <summary>
        /// Report progress
        /// </summary>
        public bool WithProgress { get; set; }

        /// <summary>
        /// Include size progress when <see cref="WithProgress"/> is enabled. Not all actions support this.
        /// </summary>
        public bool IncludeSizeProgress { get; set; }

        /// <summary>
        /// Initialize a <see cref="FtpParameter"/>-object with the given <see cref="FtpServer"/>.
        /// </summary>
        /// <param name="server">The FTP-server to use</param>
        protected FtpParameter(FtpServer server) {
            Server = server;
            FtpFiles = new List<FtpFile>();
        }

        internal IReadOnlyList<FtpFile> GetFtpFiles() => FtpFiles.AsReadOnly();
    }
}