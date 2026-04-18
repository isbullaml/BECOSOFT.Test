using BECOSOFT.Data.Models.QueryData;

namespace BECOSOFT.Data.Repositories.Interfaces.QueryData {
    internal interface IServerVersionRepository : IBaseResultRepository<ServerVersion> {
        ServerVersion GetVersion();
    }
}