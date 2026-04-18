using BECOSOFT.Data.Models.QueryData;
using BECOSOFT.Data.Repositories.Interfaces.QueryData;
using BECOSOFT.Data.Services.Interfaces.QueryData;

namespace BECOSOFT.Data.Services.QueryData {
    public sealed class ServerEditionService : IServerEditionService {
        private readonly IServerEditionRepository _repository;

        internal ServerEditionService(IServerEditionRepository repository) {
            _repository = repository;
        }

        public ServerEdition GetEdition() {
            return _repository.GetEdition();
        }
    }
}