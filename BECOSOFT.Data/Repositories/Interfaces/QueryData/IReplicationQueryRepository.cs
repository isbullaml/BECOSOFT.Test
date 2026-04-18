using BECOSOFT.Data.Models.QueryData;
using System.Collections.Generic;

namespace BECOSOFT.Data.Repositories.Interfaces.QueryData {
    internal interface IReplicationQueryRepository : IBaseResultRepository<ReplicatedTableQueryResult> {
        ReplicatedTableResult CheckReplicatedTables(ReplicatedTableParameters parameters);

        /// <summary>
        /// Returns the status of each replication subcriber for the whole server, grouped per publisher database
        /// <para>Inspiration: https://stackoverflow.com/questions/220340/how-do-i-check-sql-replication-status-via-t-sql</para>
        /// </summary>
        /// <returns></returns>
        Dictionary<string, List<ReplicationSubscriberStatus>> GetReplicationSubscriberStatus();

        /// <summary>
        /// Returns the status of each replication subcriber for the given <paramref name="databaseName"/>
        /// <para>Inspiration: https://stackoverflow.com/questions/220340/how-do-i-check-sql-replication-status-via-t-sql</para>
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        List<ReplicationSubscriberStatus> GetReplicationSubscriberStatus(string databaseName);
    }
}