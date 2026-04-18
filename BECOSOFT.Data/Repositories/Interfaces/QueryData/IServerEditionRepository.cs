using BECOSOFT.Data.Models.QueryData;

namespace BECOSOFT.Data.Repositories.Interfaces.QueryData {
    internal interface IServerEditionRepository : IBaseResultRepository<ServerEdition> {
        ServerEdition GetEdition();
    }
}