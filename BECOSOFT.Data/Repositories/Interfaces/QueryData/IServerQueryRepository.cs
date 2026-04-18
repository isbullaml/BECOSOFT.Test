using BECOSOFT.Data.Models.QueryData;

namespace BECOSOFT.Data.Repositories.Interfaces.QueryData {
    internal interface IServerQueryRepository : IBaseResultRepository<ServerQueryResult> {
        bool LinkedServerExists(string linkedServer);
    }
}