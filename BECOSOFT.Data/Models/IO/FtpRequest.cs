using BECOSOFT.Data.Services.Interfaces.IO;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Helpers;
using NLog;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace BECOSOFT.Data.Models.IO {
    public class FtpRequest : FtpRequestBase, IFtpRequest {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        protected override string Prefix => "ftp://";

        public event ProgressEventHandler Progress;

        public FtpRequest(FtpParameter ftpParameter, FtpProgress ftpProgress) : base(ftpParameter, ftpProgress) {

        }

        public void Upload(FtpFile ftpFile) {
            var request = CreateRequest(ftpFile.ServerFile);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            using (var requestStream = request.GetRequestStream()) {
                Logger.Info("Started writing file '{0}' to '{1}'", ftpFile.ServerFile, ftpFile.LocalFile);

                using (var fileStream = File.OpenRead(ftpFile.LocalFile)) {
                    ProcessStream(fileStream, requestStream, ProgressAction);
                }
            }

            void ProgressAction(long bytesProcessed, long totalBytes) {
                Logger.Debug("Uploaded {0} bytes of {3} from '{1}' to '{2}'", bytesProcessed, ftpFile.LocalFile, ftpFile.ServerFile, totalBytes);
            }
        }

        public void Download(FtpFile ftpFile) {
            var request = CreateRequest(ftpFile.ServerFile);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            using (var response = request.GetResponse()) {
                using (var stream = response.GetResponseStream()) {
                    Logger.Info("Started downloading file '{0}' to '{1}'", ftpFile.ServerFile, ftpFile.LocalFile);

                    using (var fileStream = new FileStream(ftpFile.LocalFile, FileMode.Create, FileAccess.Write)) {
                        ProcessStream(stream, fileStream, ProgressAction);
                    }
                    Logger.Info("Finished downloading file '{0}' to '{1}'", ftpFile.ServerFile, ftpFile.LocalFile);
                }
            }

            void ProgressAction(long bytesProcessed, long totalBytes) {
                Logger.Debug("Downloaded {0} bytes of {3} from '{1}' to '{2}'", bytesProcessed, ftpFile.ServerFile, ftpFile.LocalFile, totalBytes);
            }
        }

        public void Delete(FtpFile ftpFile) {
            var request = CreateRequest(ftpFile.ServerFile);
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            using (var _ = (FtpWebResponse) request.GetResponse()) {
                Logger.Info("Response received for '{0}'", GetServerPath(ftpFile));
            }
        }

        public void CreateDirectory(FtpFile ftpFile) {
            var request = CreateRequest(ftpFile.ServerFile);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            using (var _ = (FtpWebResponse) request.GetResponse()) {
                Logger.Info("Response received for '{0}'", GetServerPath(ftpFile));
            }
        }

        public FtpFileListResult List(FtpListType listType, FtpFile ftpFile) {
            var request = CreateRequest(ftpFile.ServerFile);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            using (var response = (FtpWebResponse) request.GetResponse()) {
                Logger.Info("Response received for '{0}'", GetServerPath(ftpFile));
                using (var stream = response.GetResponseStream()) {
                    return ParseListFromStream(stream, listType, ftpFile);
                }
            }
        }

        public bool ExistsOnServer(FtpFile ftpFile, bool isFolder) {
            var request = CreateRequest(ftpFile.ServerFile);
            request.Method = isFolder ? WebRequestMethods.Ftp.ListDirectory : WebRequestMethods.Ftp.GetDateTimestamp;
            try {
                using (var _ = (FtpWebResponse) request.GetResponse()) {
                    return true;
                }
            } catch (WebException ex) {
                var ftpWebResponse = ex.Response as FtpWebResponse;
                if (ftpWebResponse?.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable) {
                    return false;
                }
                throw new FtpException(ftpFile, ex.Message, ex);
            }
        }

        public void Move(FtpFile ftpFile) {
            var request = CreateRequest(ftpFile.ServerFile);
            request.Method = WebRequestMethods.Ftp.Rename;
            request.RenameTo = ftpFile.MoveToFile;
            using (var _ = (FtpWebResponse) request.GetResponse()) {
                Logger.Info("Response received for '{0}'", GetServerPath(ftpFile));
            }
        }

        private FtpWebRequest CreateRequest(string urlPart) {
            var host = FtpServer.Host;
            if (FtpServer.Host.StartsWith(Prefix, StringComparison.InvariantCultureIgnoreCase)) {
                host = host.Replace(Prefix, "");
            }
            var port = FtpServer.Port;
            var uri = GetUri(FtpServer.Password, host, urlPart, port).AbsoluteUri;
            var redactedUri = GetUri("******", host, urlPart, port).AbsoluteUri;
            Logger.Debug("Full hostname: {0}", redactedUri);
            var request = (FtpWebRequest) WebRequest.Create(uri);
            // FtpWebRequest is passive by default
            request.UsePassive = !FtpServer.UseActive;
            request.KeepAlive = false;
            request.UseBinary = true;
            request.EnableSsl = FtpServer.EnableSsl;
            return request;
        }

        private Uri GetUri(string password, string host, string urlPart, int port) {
            var prefixedHost = $"{Prefix}{Uri.EscapeDataString(FtpServer.Username)}:{Uri.EscapeDataString(password)}@{host}:{port}";
            var uri = new Uri(prefixedHost).Append(FtpServer.RootFolder, urlPart);
            return uri;
        }

        private void ProcessStream(Stream source, Stream target, Action<long, long> progressLog) {
            var buffer = new byte[DefaultBufferSize];
            int read;
            long progressed = 0;
            while ((read = source.Read(buffer, 0, buffer.Length)) > 0) {
                progressed += read;
                target.Write(buffer, 0, read);
                Logger.Debug($"Wrote {read} bytes");
                if (!WithProgress) { continue; }

                if (source.CanSeek) {
                    progressLog(progressed, source.Length);
                    Logger.Debug($"Progress: {progressed}/{source.Length}");
                }
                if (_ftpProgress.SizeProgress) {
                    _ftpProgress.UpdateByteProgress(read);
                    Progress?.Invoke(_ftpProgress);
                }
            }
            target.Flush();
        }

        private static FtpFileListResult ParseListFromStream(Stream stream, FtpListType listType, FtpFile ftpFile) {
            Logger.Info("Start parsing stream");
            var listResult = new FtpFileListResult(ftpFile);

            using (var streamReader = new StreamReader(stream)) {
                while (!streamReader.EndOfStream) {
                    var line = streamReader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) { continue; }
                    Logger.Info("Found item '{0}'", line);
                    var parseResult = LineParseResult.From(line);
                    if (!parseResult.DidMatch) {
                        Logger.Info("Line did not match known regexes.");
                        continue;
                    }
                    var shouldAdd = ((parseResult.IsDirectory && listType == FtpListType.Directory) ||
                                     (!parseResult.IsDirectory && listType == FtpListType.File)) ||
                                    listType == FtpListType.All;

                    if (!shouldAdd) {
                        Logger.Info("Skipped item '{0}'", line);
                        continue;
                    }
                    var listEntry = new FtpListEntry(parseResult.Name) {
                        IsDirectory = parseResult.IsDirectory,
                        Size = parseResult.Size,
                        LastModified = parseResult.DateModified
                    };
                    listResult.List.Add(listEntry);
                    if (listEntry.IsDirectory) {
                        Logger.Info("Added item '{0}' (directory) (last modified: {1:yyyy-MM-dd HH:mm}) to list", listEntry.Name, listEntry.LastModified);
                    } else {
                        Logger.Info("Added item '{0}' with size {1} (last modified: {2:yyyy-MM-dd HH:mm}) to list", listEntry.Name, listEntry.Size, listEntry.LastModified);
                    }
                }
                Logger.Info("Finished parsing stream");
                listResult.Success = true;
            }

            return listResult;
        }

        private class LineParseResult {
            private static readonly RegexOptions RegexOptions = RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace;
            /// <summary>
            /// Source: https://stackoverflow.com/questions/7060983/c-sharp-class-to-parse-webrequestmethods-ftp-listdirectorydetails-ftp-response (22/03/2018)
            /// </summary>
            private static readonly string WindowsPattern = @"^(\d+-\d+-\d+\s+\d+:\d+(?:AM|PM))\s+(<DIR>|\d+)\s+(.+)$";
            /// <summary>
            /// Source: https://stackoverflow.com/questions/1013486/parsing-ftpwebrequest-listdirectorydetails-line (30/08/2018)
            /// </summary>
            private static readonly string LinuxPattern = @"^([d-])([rwxt-]{3}){3}\s+\d{1,}\s+.*?(\d{1,})\s+(\w+\s+\d{1,2}\s+(?:\d{4})?)\s?(\d{1,2}:\d{2})?\s+(.+?)\s?$";
            private static readonly Regex WindowsRegex = new Regex(WindowsPattern, RegexOptions);
            private static readonly Regex LinuxRegex = new Regex(LinuxPattern, RegexOptions);

            public bool DidMatch { get; private set; } = true;
            public bool IsDirectory { get; private set; }
            public long Size { get; private set; }
            public string Name { get; private set; }
            public DateTime DateModified { get; private set; }

            public static LineParseResult From(string line) {
                var match = WindowsRegex.Match(line);
                var linuxMatch = LinuxRegex.Match(line);
                var result = new LineParseResult();
                var isDir = false;
                if (match.Success) {
                    isDir = match.Groups[2].Value.Equals("<DIR>");
                    result.Size = !isDir ? match.Groups[2].Value.To<long>() : 0L;
                    var dateModifiedValue = match.Groups[1].Value;
                    result.Name = match.Groups[3].Value;
                    result.DateModified = DateTime.ParseExact(dateModifiedValue, "MM-dd-yy  hh:mmtt", CultureInfo.InvariantCulture, DateTimeStyles.None);
                } else if (linuxMatch.Success) {
                    isDir = linuxMatch.Groups[1].Value.Equals("d");
                    result.Size = !isDir ? linuxMatch.Groups[3].Value.To<long>() : 0L;
                    var dateModifiedValue = $"{linuxMatch.Groups[4].Value.Trim()} {linuxMatch.Groups[5].Value.Trim()}".Replace("  ", " ").Trim();
                    result.DateModified = TryParseLinuxDateTime(dateModifiedValue);
                    result.Name = linuxMatch.Groups[6].Value;
                } else {
                    result.DidMatch = false;
                }
                result.IsDirectory = isDir;
                return result;
            }

            private static DateTime TryParseLinuxDateTime(string dateValue) {
                DateTime parseResult;
                if (DateTime.TryParseExact(dateValue, LinuxDateTimePatterns, CultureInfo.InvariantCulture, DateTimeStyles.None, out parseResult)) {
                    return parseResult;
                }
                if (DateTime.TryParseExact(dateValue, LinuxDatePatterns, CultureInfo.InvariantCulture, DateTimeStyles.None, out parseResult)) {
                    return parseResult;
                }
                if (DateTime.TryParseExact(dateValue, LinuxYearPatterns, CultureInfo.InvariantCulture, DateTimeStyles.None, out parseResult)) {
                    return parseResult;
                }
                return DateTimeHelpers.BaseDate;
            }

            private static readonly string[] LinuxDateTimePatterns = {
                "MMM dd HH:mm", "MMM dd yyyy HH:mm",
                "MMM d HH:mm", "MMM d yyyy HH:mm",
                "MMM dd H:mm", "MMM dd yyyy H:mm",
                "MMM d H:mm", "MMM d yyyy H:mm"
            };

            private static readonly string[] LinuxDatePatterns = {
                "MMM dd", "MMM dd yyyy",
                "MMM d", "MMM d yyyy",
            };

            private static readonly string[] LinuxYearPatterns = {
                "yyyy",
            };
        }
    }
}