using System.Collections.Generic;
using System.Diagnostics;

namespace BECOSOFT.Data.Models.IO {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FtpListParameter : FtpParameter {
        /// <summary>
        /// <see cref="FtpListType"/> to fetch information for
        /// </summary>
        public FtpListType Type { get; }

        /// <summary>
        /// A list of folders to fetch the information for (according to the <see cref="FtpListType"/>).
        /// </summary>
        public List<FtpFile> Folders {
            get => FtpFiles;
            set => FtpFiles = value;
        }

        /// <summary>
        /// Initialize a <see cref="FtpListParameter"/>-object with the given <see cref="FtpServer"/> and <see cref="FtpListType"/>.
        /// </summary>
        /// <param name="server"><see cref="FtpServer"/> contains the connection properties for an FTP-server.</param>
        /// <param name="type"></param>
        public FtpListParameter(FtpServer server, FtpListType type) : base(server) {
            Type = type;
        }

        /// <summary>
        /// Initialize a <see cref="FtpListParameter"/>-object with the given <see cref="server"/>, <see cref="type"/> and <see cref="folders"/>.
        /// </summary>
        /// <param name="server"><see cref="FtpServer"/> contains the connection properties for an FTP-server.</param>
        /// <param name="type"></param>
        /// <param name="folders">Adds the <see cref="folders"/> to <see cref="Folders"/> if <see cref="folders"/> is not <see langword="null"/>.</param>
        public FtpListParameter(FtpServer server, FtpListType type, List<FtpFile> folders) : this(server, type) {
            if (folders != null) {
                Folders.AddRange(folders);
            }
        }

        /// <summary>
        /// Returns a <see cref="FtpListParameter"/>-object created from the given <see cref="FtpFileParameter"/> and <see cref="FtpListType"/>.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static FtpListParameter From(FtpFileParameter parameter, FtpListType type) {
            var result = new FtpListParameter(parameter.Server, type);
            result.Folders.AddRange(parameter.Files);
            return result;
        }

        private string DebuggerDisplay => $"{Server}, Folders: {Folders.Count}, List type: {Type}";
    }
}