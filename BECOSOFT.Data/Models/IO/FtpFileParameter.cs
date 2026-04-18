using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace BECOSOFT.Data.Models.IO {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FtpFileParameter : FtpParameter {
        /// <summary>
        /// A list of files.
        /// </summary>
        public List<FtpFile> Files {
            get => FtpFiles;
            set => FtpFiles = value;
        }
        
        /// <summary>
        /// Initialize a <see cref="FtpFileParameter"/>-object with the given <see cref="FtpServer"/>.
        /// </summary>
        /// <param name="server"><see cref="FtpServer"/> contains the connection properties for an FTP-server.</param>
        public FtpFileParameter(FtpServer server) : base(server) {
        }

        /// <summary>
        /// Initialize a <see cref="FtpFileParameter"/>-object with the given <see cref="server"/> and <see cref="files"/>.
        /// </summary>
        /// <param name="server"><see cref="FtpServer"/> contains the connection properties for an FTP-server.</param>
        /// <param name="files">Adds the <see cref="files"/> to <see cref="Files"/> if <see cref="files"/> is not <see langword="null"/>.</param>
        public FtpFileParameter(FtpServer server, List<FtpFile> files) : this(server) {
            if (files.IsEmpty()) { return; }
            Files.AddRange(files);
        }

        private string DebuggerDisplay => $"{Server}, Files: {Files}";
    }
}