using BECOSOFT.Data.Models.QueryData;
using BECOSOFT.Data.Repositories.Interfaces.QueryData;
using System.Collections.Generic;

namespace BECOSOFT.Data.Services.Interfaces.QueryData {
    public interface IReplicationQueryService : IBaseService {
        ReplicatedTableResult CheckReplicatedTables(ReplicatedTableParameters parameters);
        /// <inheritdoc cref="IReplicationQueryRepository.GetReplicationSubscriberStatus()"/>
        Dictionary<string, List<ReplicationSubscriberStatus>> GetReplicationSubscriberStatus();
        /// <inheritdoc cref="IReplicationQueryRepository.GetReplicationSubscriberStatus(string)"/>
        List<ReplicationSubscriberStatus> GetReplicationSubscriberStatus(string databaseName);
    }
}