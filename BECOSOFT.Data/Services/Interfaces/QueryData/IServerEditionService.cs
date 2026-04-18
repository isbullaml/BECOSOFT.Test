using BECOSOFT.Data.Models.QueryData;

namespace BECOSOFT.Data.Services.Interfaces.QueryData {
    public interface IServerEditionService : IBaseService {
        ServerEdition GetEdition();
    }
}