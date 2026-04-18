using BECOSOFT.Data.Services.Interfaces.IO;
using NLog;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.IO;
using System.Linq;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions.Collections;

namespace BECOSOFT.Data.Models.IO {
    public class SftpRequest : FtpRequestBase, IFtpRequest {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        protected override string Prefix => "sftp://";

        public event ProgressEventHandler Progress;


        public SftpRequest(FtpParameter ftpParameter, FtpProgress ftpProgress) : base(ftpParameter, ftpProgress) {

        }


        public void Upload(FtpFile ftpFile) {
            void ProgressAction(long bytesProcessed, long totalBytes) {
                if (!WithProgress) { return; }

                Logger.Debug("Uploaded {0} bytes of {3} from '{1}' to '{2}'", bytesProcessed, ftpFile.LocalFile, ftpFile.ServerFile, totalBytes);
                if (_ftpProgress.SizeProgress) {
                    _ftpProgress?.UpdateByteProgress(bytesProcessed);
                    Progress?.Invoke(_ftpProgress);
                }
            }

            ExecuteSftpFunc(ftpFile, (sftp, file) => {
                using (var fileStream = File.OpenRead(ftpFile.LocalFile)) {
                    var fileLength = fileStream.Length;

                    sftp.UploadFile(fileStream, ftpFile.ServerFile, bytesWritten => { ProgressAction(bytesWritten.To<long>(), fileLength); });
                }
                return true;
            });
        }


        public void Download(FtpFile ftpFile) {
            void ProgressAction(long bytesProcessed, long totalBytes) {
                if (!WithProgress) { return; }

                Logger.Debug("Downloaded {0} bytes of {3} from '{1}' to '{2}'", bytesProcessed, ftpFile.LocalFile, ftpFile.ServerFile, totalBytes);
                if (_ftpProgress.SizeProgress) {
                    _ftpProgress?.UpdateByteProgress(bytesProcessed);
                    Progress?.Invoke(_ftpProgress);
                }
            }

            ExecuteSftpFunc(ftpFile, (sftp, file) => {
                using (var fileStream = new FileStream(ftpFile.LocalFile, FileMode.Create, FileAccess.Write)) {
                    var fileLength = fileStream.Length;
                    Logger.Info("Started downloading file '{0}' ({2} bytes) to '{1}'", ftpFile.ServerFile, ftpFile.LocalFile, fileLength);

                    sftp.DownloadFile(ftpFile.ServerFile, fileStream, bytesWritten => { ProgressAction(bytesWritten.To<long>(), fileLength); });

                    Logger.Info("Finished downloading file '{0}' ({2} bytes) to '{1}'", ftpFile.ServerFile, ftpFile.LocalFile, fileLength);
                }
                return true;
            });
        }


        public void Delete(FtpFile ftpFile) {
            ExecuteSftpFunc(ftpFile, (sftp, file) => {
                sftp.DeleteFile(ftpFile.ServerFile);
                return true;
            });
            Logger.Info("Response received for '{0}'", GetServerPath(ftpFile));
        }


        public void CreateDirectory(FtpFile ftpFile) {
            ExecuteSftpFunc(ftpFile, (sftp, file) => {
                sftp.CreateDirectory(ftpFile.ServerFile);
                return true;
            });
            Logger.Info("Response received for '{0}'", GetServerPath(ftpFile));
        }


        public FtpFileListResult List(FtpListType listType, FtpFile ftpFile) {
            var listResult = new FtpFileListResult(ftpFile);
            var result = ExecuteSftpFunc(ftpFile, (sftp, file) => sftp.ListDirectory(ftpFile.ServerFile));

            foreach (var file in result) {
                var shouldAdd = ((file.IsDirectory && listType == FtpListType.Directory) ||
                                 (!file.IsDirectory && listType == FtpListType.File)) ||
                                listType == FtpListType.All;

                if (!shouldAdd) {
                    Logger.Debug("Skipped item '{0}'", file.FullName);
                    continue;
                }

                var listEntry = new FtpListEntry(file.Name) {
                    IsDirectory = file.IsDirectory,
                    Size = file.Length,
                    LastModified = file.LastWriteTime
                };

                listResult.List.Add(listEntry);
                if (listEntry.IsDirectory) {
                    Logger.Info("Added item '{0}' (directory) (last modified: {1:yyyy-MM-dd HH:mm}) to list", listEntry.Name, listEntry.LastModified);
                } else {
                    Logger.Info("Added item '{0}' with size {1} (last modified: {2:yyyy-MM-dd HH:mm}) to list", listEntry.Name, listEntry.Size, listEntry.LastModified);
                }
            }

            listResult.Success = true;

            return listResult;
        }


        public bool ExistsOnServer(FtpFile ftpFile, bool isFolder) {
            return ExecuteSftpFunc(ftpFile, (sftp, file) => {
                if (isFolder) {
                    try {
                        sftp.ListDirectory(ftpFile.ServerFile);
                        return true;
                    } catch (SftpPathNotFoundException) {
                        return false;
                    }
                }
                return sftp.Exists(ftpFile.ServerFile);
            });
        }

        public void Move(FtpFile ftpFile) {
            ExecuteSftpFunc(ftpFile, (sftp, file) => {
                sftp.RenameFile(ftpFile.ServerFile, ftpFile.MoveToFile);
                return true;
            });
            Logger.Info("Response received for '{0}'", GetServerPath(ftpFile));
        }

        private T ExecuteSftpFunc<T>(FtpFile ftpFile, Func<SftpClient, FtpFile, T> function) {
            using (var sftp = CreateSftpClient()) {
                sftp.Connect();
                var result = function(sftp, ftpFile);
                sftp.Disconnect();
                return result;
            }
        }

        private SftpClient CreateSftpClient() {
            var host = FtpServer.Host;
            if (FtpServer.Host.StartsWith(Prefix, StringComparison.InvariantCultureIgnoreCase)) {
                host = host.Replace(Prefix, "");
            }
            Logger.Debug("Full hostname: {0}", host);
            if (FtpServer.PrivateKeyFiles.IsEmpty()) {
                return new SftpClient(host, FtpServer.Port, FtpServer.Username, FtpServer.Password);
            }

            var privateKeyFiles = FtpServer.PrivateKeyFiles.Select(p => new PrivateKeyFile(p)).ToArray();
            return new SftpClient(host, FtpServer.Port, FtpServer.Username, privateKeyFiles);
        }
    }
}
