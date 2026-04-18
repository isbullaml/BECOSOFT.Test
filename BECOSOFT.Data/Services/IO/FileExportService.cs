using BECOSOFT.Data.Services.Interfaces.IO;
using BECOSOFT.Utilities.Helpers.IO;
using BECOSOFT.Utilities.Models.IO;

namespace BECOSOFT.Data.Services.IO {
    public class FileExportService : IFileExportService {
        public ContentFileResult Save(ContentFileExportParameters exportParameters) {
            return FileExportHelper.Save(exportParameters);
        }
    }
}