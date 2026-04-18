using System;
using BECOSOFT.Utilities.Extensions;

namespace BECOSOFT.Data.Models.IO {
    public abstract class FtpRequestBase {
        protected static readonly int DefaultBufferSize = 4096;
        protected readonly FtpParameter _ftpParameter;
        protected readonly FtpProgress _ftpProgress;
        protected abstract string Prefix { get; }
        protected FtpServer FtpServer => _ftpParameter.Server;
        protected bool WithProgress => _ftpParameter.WithProgress && _ftpProgress != null;

        protected FtpRequestBase(FtpParameter ftpParameter, FtpProgress ftpProgress) {
            if (ftpParameter.Server == null) {
                throw new ArgumentException();
            }
            if (ftpParameter.Server.Host.IsNullOrWhiteSpace()) {
                throw new ArgumentException();
            }
            _ftpParameter = ftpParameter;
            _ftpProgress = ftpProgress;
        }
        
        public string GetServerPath(FtpFile file) {
            var host = FtpServer.Host;

            if (FtpServer.Host.StartsWith(Prefix, StringComparison.InvariantCultureIgnoreCase)) {
                host = host.Replace(Prefix, "");
            }
            if (FtpServer.ServerType == FtpServerType.Sftp) {
                return file.ServerFile;
            }
            host = $"{Prefix}{host}:{FtpServer.Port}";
            var uri = new Uri(host).Append(FtpServer.RootFolder, file.ServerFile).AbsoluteUri;
            return uri;
        }

        public static IFtpRequest Create(FtpParameter ftpParameter, FtpProgress progress) {
            if (ftpParameter.Server == null) {
                throw new ArgumentException(nameof(ftpParameter.Server));
            }
            if (ftpParameter.Server.Host.IsNullOrWhiteSpace()) {
                throw new ArgumentException(nameof(ftpParameter.Server.Host));
            }
            if (ftpParameter.Server.ServerType == FtpServerType.Ftp) {
                return new FtpRequest(ftpParameter, progress);
            }

            if (ftpParameter.Server.ServerType == FtpServerType.Sftp) {
                return new SftpRequest(ftpParameter, progress);
            }

            throw new ArgumentException(nameof(ftpParameter.Server.ServerType));
        }
    }
}