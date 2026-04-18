using BECOSOFT.Utilities.Models.IO;

namespace BECOSOFT.Data.Services.Interfaces.IO {
    public interface IFileExportService : IBaseService {
        ContentFileResult Save(ContentFileExportParameters exportParameters);
    }
}