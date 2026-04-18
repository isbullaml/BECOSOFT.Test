using BECOSOFT.Data.Services.Interfaces.IO;

namespace BECOSOFT.Data.Models.IO {
    public interface IFtpRequest {
        event ProgressEventHandler Progress;
        void Upload(FtpFile ftpFile);
        void Download(FtpFile ftpFile);
        void Delete(FtpFile ftpFile);
        void CreateDirectory(FtpFile ftpFile);
        FtpFileListResult List(FtpListType listType, FtpFile ftpFile);
        bool ExistsOnServer(FtpFile ftpFile, bool isFolder);
        string GetServerPath(FtpFile file);
        void Move(FtpFile file);
    }
}