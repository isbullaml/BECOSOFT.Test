using BECOSOFT.Utilities.Models.IO;

namespace BECOSOFT.Data.Services.Interfaces.IO {
    public interface IFileImportService : IBaseService {
        ContentFileResult Import(ContentFileImportParameters importParameters);
    }
}