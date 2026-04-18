using BECOSOFT.Data.Services.Interfaces.IO;
using BECOSOFT.Utilities.Helpers.IO;
using BECOSOFT.Utilities.Models.IO;

namespace BECOSOFT.Data.Services.IO {
    public class FileImportService : IFileImportService {
        public ContentFileResult Import(ContentFileImportParameters importParameters) {
            return FileImportHelper.Import(importParameters);
        }
    }
}