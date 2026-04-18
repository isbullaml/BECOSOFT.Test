using BECOSOFT.Data.Models.QueryData;

namespace BECOSOFT.Data.Services.Interfaces.QueryData {
    public interface IServerVersionService : IBaseService {
        ServerVersion GetVersion();
    }
}