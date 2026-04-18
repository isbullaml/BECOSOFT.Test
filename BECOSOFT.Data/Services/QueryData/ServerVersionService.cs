using BECOSOFT.Data.Models.QueryData;
using BECOSOFT.Data.Repositories.Interfaces.QueryData;
using BECOSOFT.Data.Services.Interfaces.QueryData;

namespace BECOSOFT.Data.Services.QueryData {
    public sealed class ServerVersionService : IServerVersionService {
        private readonly IServerVersionRepository _repository;

        internal ServerVersionService(IServerVersionRepository repository) {
            _repository = repository;
        }

        public ServerVersion GetVersion() {
            return _repository.GetVersion();
        }
    }
}