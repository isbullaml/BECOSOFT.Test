using BECOSOFT.Data.Repositories.Interfaces.QueryData;
using BECOSOFT.Data.Services.Interfaces.QueryData;

namespace BECOSOFT.Data.Services.QueryData {
    public sealed class ServerQueryService : IServerQueryService {
        private readonly IServerQueryRepository _repository;

        internal ServerQueryService(IServerQueryRepository repository) {
            _repository = repository;
        }

        public bool LinkedServerExists(string linkedServer) {
            return _repository.LinkedServerExists(linkedServer);
        }
    }
}