using BECOSOFT.Data.Models.IO;
using BECOSOFT.Data.Services.Interfaces.IO;
using NLog;
using System;
using System.IO;
using System.Linq;

namespace BECOSOFT.Data.Services.IO {
    /// <inheritdoc/>
    public sealed class FtpService : IFtpService {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private delegate FtpFileResult ActionHandle(IFtpRequest request, FtpFile file);

        public event ProgressEventHandler Progress;

        public FtpResult Upload(FtpFileParameter fileParameter) {
            var totalFiles = fileParameter.Files.Count;
            Logger.Info("Start handling upload for {0} files", totalFiles);
            var progress = new FtpProgress(0, 0, false);
            if (fileParameter.WithProgress) {
                var totalBytes = fileParameter.Files.Sum(f => new FileInfo(f.LocalFile).Length);
                Logger.Debug("Progress enabled: {0} bytes ({1} files) to upload", totalBytes, totalFiles);
                progress = new FtpProgress(totalBytes, totalFiles, fileParameter.IncludeSizeProgress);
                Progress?.Invoke(progress);
            }

            FtpFileResult Action(IFtpRequest request, FtpFile file) {
                var serverPath = request.GetServerPath(file);
                var uploadResult = new FtpFileResult(file);
                if (IsInvalidFtpFile(file, uploadResult, false)) { return uploadResult; }
                try {
                    Logger.Info("Started uploading file '{0}' to '{1}'", file.LocalFile, serverPath);
                    request.Upload(file);
                    if (fileParameter.WithProgress) {
                        progress.UpdateFileProgress();
                        Progress?.Invoke(progress);
                    }
                    Logger.Info("Uploaded file '{0}' to '{1}'", file.LocalFile, serverPath);
                    uploadResult.Success = true;
                } catch (Exception ex) {
                    Logger.Error(ex, "Error occurred uploading file '{0}' to '{1}", file.LocalFile, serverPath);
                    uploadResult.Error = ex.Message;
                }
                return uploadResult;
            }

            var result = HandleResult(fileParameter, progress, Action);
            Logger.Info("Handled upload for {0} files", totalFiles);
            return result;
        }

        public FtpResult Download(FtpFileParameter fileParameter) {
            Logger.Info("Start handling download for {0} files", fileParameter.Files.Count);
            var progress = new FtpProgress(0, 0, false);
            if (fileParameter.WithProgress) {
                Logger.Debug("Progress enabled");
                long totalBytes = -1;
                int totalFiles;
                if (fileParameter.IncludeSizeProgress) {
                    var listParameter = FtpListParameter.From(fileParameter, FtpListType.File);
                    var listResult = List(listParameter);
                    totalBytes = listResult.Results.Sum(r => r.List.Sum(rr => rr.Size));
                    totalFiles = listResult.Results.Sum(r => r.List.Count);
                } else {
                    totalFiles = fileParameter.Files.Count;
                }
                Logger.Debug("Progress enabled: {0} bytes ({1} files) to download", totalBytes, totalFiles);
                progress = new FtpProgress(totalBytes, totalFiles, fileParameter.IncludeSizeProgress);
                Progress?.Invoke(progress);
            }

            FtpFileResult Action(IFtpRequest request, FtpFile file) {
                var serverPath = request.GetServerPath(file);
                var downloadResult = new FtpFileResult(file);
                if (IsInvalidFtpFile(file, downloadResult, true)) { return downloadResult; }
                if (!request.ExistsOnServer(file, false)) {
                    downloadResult.Error = $"File '{serverPath}' does not exist on server";
                    return downloadResult;
                }
                try {
                    Logger.Debug("Response received for '{0}'", file.ServerFile);
                    if (fileParameter.WithProgress) {
                        progress.UpdateFileProgress();
                    }

                    request.Download(file);

                    if (fileParameter.WithProgress) {
                        Progress?.Invoke(progress);
                    }
                    downloadResult.Success = true;
                } catch (Exception ex) {
                    Logger.Error(ex, "Error occurred downloading file '{0}' to '{1}'", serverPath, file.LocalFile);
                    downloadResult.Error = ex.Message;
                }
                return downloadResult;
            }

            var result = HandleResult(fileParameter, progress, Action);
            Logger.Info("Handled download for {0} files", fileParameter.Files.Count);
            return result;
        }

        private static bool IsInvalidFtpFile(FtpFile file, FtpFileResult fileResult, bool onlyServerFile) {
            if (string.IsNullOrWhiteSpace(file.ServerFile)) {
                fileResult.Error = "Invalid server file name";
                return true;
            }
            if (!onlyServerFile && (string.IsNullOrWhiteSpace(file.LocalFile) || !File.Exists(file.LocalFile))) {
                fileResult.Error = "Invalid local file";
                return true;
            }
            return false;
        }

        public FtpResult Delete(FtpFileParameter fileParameter) {
            Logger.Info("Start handling delete for {0} files", fileParameter.Files.Count);
            var progress = new FtpProgress(0, 0, false);
            if (fileParameter.WithProgress) {
                Logger.Debug("Progress enabled");
                var totalFiles = fileParameter.Files.Count;
                Logger.Debug("Progress enabled: {0} files to delete", totalFiles);
                progress = new FtpProgress(0, totalFiles, false);
                Progress?.Invoke(progress);
            }

            FtpFileResult Action(IFtpRequest request, FtpFile file) {
                var serverPath = request.GetServerPath(file);
                var deleteResult = new FtpFileResult(file);
                if (IsInvalidFtpFile(file, deleteResult, true)) { return deleteResult; }
                if (!request.ExistsOnServer(file, false)) {
                    deleteResult.Error = $"File '{serverPath}' does not exist on server";
                    return deleteResult;
                }
                Logger.Info("Request response for '{0}'", serverPath);
                try {
                    if (fileParameter.WithProgress) {
                        progress.UpdateFileProgress();
                    }
                    request.Delete(file);
                    if (fileParameter.WithProgress) {
                        Progress?.Invoke(progress);
                    }
                    deleteResult.Success = true;
                } catch (Exception ex) {
                    Logger.Error(ex, "Error occurred deleting file '{0}'", serverPath);
                    deleteResult.Error = ex.Message;
                }
                return deleteResult;
            }

            var result = HandleResult(fileParameter, progress, Action);
            Logger.Info("Handled delete for {0} files", fileParameter.Files.Count);
            return result;
        }

        public FtpResult CreateDirectory(FtpFileParameter fileParameter) {
            Logger.Info("Start handling directory creation for {0} directories", fileParameter.Files.Count);
            var progress = new FtpProgress(0, 0, false);
            if (fileParameter.WithProgress) {
                Logger.Debug("Progress enabled");
                var totalFiles = fileParameter.Files.Count;
                Logger.Debug("Progress enabled: {0} directories to create", totalFiles);
                progress = new FtpProgress(0, totalFiles, false);
                Progress?.Invoke(progress);
            }

            FtpFileResult Action(IFtpRequest request, FtpFile file) {
                var serverPath = request.GetServerPath(file);
                var directoryResult = new FtpFileResult(file);
                if (IsInvalidFtpFile(file, directoryResult, true)) { return directoryResult; }
                Logger.Info("Request response for '{0}'", serverPath);
                try {
                    if (fileParameter.WithProgress) {
                        progress.UpdateFileProgress();
                    }

                    request.CreateDirectory(file);

                    if (fileParameter.WithProgress) {
                        Progress?.Invoke(progress);
                    }
                    directoryResult.Success = true;
                } catch (Exception ex) {
                    Logger.Error(ex, "Error occurred creation directory '{0}'", serverPath);
                    directoryResult.Error = ex.Message;
                }
                return directoryResult;
            }

            var result = HandleResult(fileParameter, progress, Action);
            Logger.Info("Handled directory creation for {0} files", fileParameter.Files.Count);
            return result;
        }

        public FtpListResult List(FtpListParameter parameter) {
            var type = parameter.Type.ToString().ToLower();
            Logger.Info("Start handling {1} listing for {0} directories", parameter.Folders.Count, type);
            var progress = new FtpProgress(0, 0, false);
            if (parameter.WithProgress) {
                Logger.Debug("Progress enabled");
                var totalFiles = parameter.Folders.Count;
                Logger.Debug("Progress enabled: {0} directories to parse", totalFiles);
                progress = new FtpProgress(0, totalFiles, false);
                Progress?.Invoke(progress);
            }

            FtpFileResult Action(IFtpRequest request, FtpFile file) {
                var serverPath = request.GetServerPath(file);
                var listResult = new FtpFileListResult(file);
                if (IsInvalidFtpFile(file, listResult, true)) { return listResult; }
                if (!request.ExistsOnServer(file, true)) {
                    listResult.Error = $"Directory '{serverPath}' does not exist on server";
                    return listResult;
                }
                Logger.Info("Request response for '{0}'", serverPath);
                try {
                    if (parameter.WithProgress) {
                        progress.UpdateFileProgress();
                    }
                    listResult = request.List(parameter.Type, file);

                    if (parameter.WithProgress) {
                        Progress?.Invoke(progress);
                    }
                    Logger.Info("Found {1} {2} in directory '{0}'", serverPath, listResult.List.Count, type);
                } catch (Exception ex) {
                    Logger.Error(ex, "Error occurred listing directory '{0}'", serverPath);
                    listResult.Error = ex.Message;
                }
                return listResult;
            }

            var result = HandleListResult(parameter, null, Action);
            Logger.Info("Handled {1} listing for {0} directories", parameter.Folders.Count, type);
            return result;
        }

        public FtpResult Move(FtpFileParameter parameter) {
            Logger.Info("Start handling delete for {0} files", parameter.Files.Count);
            var progress = new FtpProgress(0, 0, false);
            if (parameter.WithProgress) {
                Logger.Debug("Progress enabled");
                var totalFiles = parameter.Files.Count;
                Logger.Debug("Progress enabled: {0} files to move", totalFiles);
                progress = new FtpProgress(0, totalFiles, false);
                Progress?.Invoke(progress);
            }

            FtpFileResult Action(IFtpRequest request, FtpFile file) {
                var serverPath = request.GetServerPath(file);
                var moveResult = new FtpFileResult(file);
                if (IsInvalidFtpFile(file, moveResult, true)) { return moveResult; }
                if (!request.ExistsOnServer(file, false)) {
                    moveResult.Error = $"File '{serverPath}' does not exist on server";
                    return moveResult;
                }
                Logger.Info("Request response for '{0}'", serverPath);
                try {
                    if (parameter.WithProgress) {
                        progress.UpdateFileProgress();
                    }
                    request.Move(file);
                    if (parameter.WithProgress) {
                        Progress?.Invoke(progress);
                    }
                    moveResult.Success = true;
                } catch (Exception ex) {
                    Logger.Error(ex, "Error occurred moving file '{0}'", serverPath);
                    moveResult.Error = ex.Message;
                }
                return moveResult;
            }

            var result = HandleResult(parameter, null, Action);
            Logger.Info("Handled move for {0} files", parameter.Files.Count);
            return result;
        }

        private FtpResult HandleResult(FtpParameter fileParameter, FtpProgress progress, ActionHandle action) {
            var result = new FtpResult();
            Handle(result, fileParameter, progress, action);
            return result;
        }

        private FtpListResult HandleListResult(FtpParameter fileParameter, FtpProgress progress, ActionHandle action) {
            var result = new FtpListResult();
            Handle(result, fileParameter, progress, action);
            return result;
        }

        private void Handle<T>(IFtpResult<T> result, FtpParameter fileParameter, FtpProgress progress, ActionHandle action) where T : FtpFileResult {
            var request = FtpRequestBase.Create(fileParameter, progress);
            void RequestOnProgress(FtpProgress ftpProgress) => Progress?.Invoke(ftpProgress);
            if (fileParameter.WithProgress) { request.Progress += RequestOnProgress; }

            var files = fileParameter.GetFtpFiles();
            foreach (var ftpFile in files) {
                var actionResult = action(request, ftpFile);
                result.Results.Add((T)actionResult);
            }
        }
    }
}