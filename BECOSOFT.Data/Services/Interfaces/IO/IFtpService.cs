using BECOSOFT.Data.Models.IO;

namespace BECOSOFT.Data.Services.Interfaces.IO {
    public delegate void ProgressEventHandler(FtpProgress ftpProgress);
    public interface IFtpService : IBaseService {
        /// <summary>
        /// Occurs when there is progress to report.
        /// </summary>
        event ProgressEventHandler Progress;
        /// <summary>
        /// Upload one or more files to the <see cref="FtpServer"/>.
        /// </summary>
        /// <param name="fileParameter"><see cref="FtpFileParameter"/> contains the <see cref="FtpFile"/> objects to upload</param>
        /// <returns></returns>
        FtpResult Upload(FtpFileParameter fileParameter);
        /// <summary>
        /// Download one or more files from the <see cref="FtpServer"/>.
        /// </summary>
        /// <param name="fileParameter"><see cref="FtpFileParameter"/> contains the <see cref="FtpFile"/> objects to download</param>
        /// <returns></returns>
        FtpResult Download(FtpFileParameter fileParameter);
        /// <summary>
        /// Delete one or more files from the <see cref="FtpServer"/>.
        /// </summary>
        /// <param name="fileParameter"><see cref="FtpFileParameter"/> contains the <see cref="FtpFile"/> objects to delete</param>
        /// <returns></returns>
        FtpResult Delete(FtpFileParameter fileParameter);
        /// <summary>
        /// Move one or more files on the <see cref="FtpServer"/>.
        /// </summary>
        /// <param name="fileParameter"><see cref="FtpFileParameter"/> contains the <see cref="FtpFile"/> objects to move</param>
        /// <returns></returns>
        FtpResult Move(FtpFileParameter fileParameter);
        /// <summary>
        /// Create one or more directories on the <see cref="FtpServer"/>.
        /// </summary>
        /// <param name="fileParameter"><see cref="FtpFileParameter"/> contains the <see cref="FtpFile"/> objects to create a directory for</param>
        /// <returns></returns>
        FtpResult CreateDirectory(FtpFileParameter fileParameter);
        /// <summary>
        /// Returns file or directory information from the <see cref="FtpServer"/>.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        FtpListResult List(FtpListParameter parameter);
    }
}