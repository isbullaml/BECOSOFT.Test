using BECOSOFT.Data.Models.QueryData;
using BECOSOFT.Data.Repositories.Interfaces.QueryData;
using BECOSOFT.Data.Services.Interfaces.QueryData;
using System.Collections.Generic;

namespace BECOSOFT.Data.Services.QueryData {
    public sealed class ReplicationQueryService : IReplicationQueryService {
        private readonly IReplicationQueryRepository _repository;

        internal ReplicationQueryService(IReplicationQueryRepository repository) {
            _repository = repository;
        }

        public ReplicatedTableResult CheckReplicatedTables(ReplicatedTableParameters parameters) {
            return _repository.CheckReplicatedTables(parameters);
        }

        public Dictionary<string, List<ReplicationSubscriberStatus>> GetReplicationSubscriberStatus() {
            return _repository.GetReplicationSubscriberStatus();
        }

        public List<ReplicationSubscriberStatus> GetReplicationSubscriberStatus(string databaseName) {
            return _repository.GetReplicationSubscriberStatus(databaseName);
        }
    }
}