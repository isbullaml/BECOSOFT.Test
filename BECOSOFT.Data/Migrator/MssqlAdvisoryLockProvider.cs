using BECOSOFT.Data.Context;
using System.Data;

namespace BECOSOFT.Data.Migrator {
    internal class MssqlAdvisoryLockProvider : AdvisoryLockProvider {
        public MssqlAdvisoryLockProvider(IDbConnectionFactory connectionFactory)
            : base(connectionFactory) { }

        public override IDbConnection Begin() {
            AcquireAdvisoryLock();
            return Connection;
        }

        public override void End() {
            ReleaseAdvisoryLock();
        }

        public override void AcquireAdvisoryLock() {
            using (var command = Connection.CreateCommand()) {
                command.CommandText = $"sp_getapplock @Resource = '{AdvisoryLockName}', " +
                                      "@LockMode = 'Exclusive', " +
                                      "@LockOwner = 'Session', " +
                                      $"@LockTimeout = '{(int)LockTimeout.TotalMilliseconds}'";
                command.CommandTimeout = 0; // The lock will time out by itself
                command.ExecuteNonQuery();
            }
        }

        public override void ReleaseAdvisoryLock() {
            using (var command = Connection.CreateCommand()) {
                command.CommandText = $"sp_releaseapplock @Resource = '{AdvisoryLockName}', " +
                                      "@LockOwner = 'Session'";
                command.ExecuteNonQuery();
            }
        }
    }
}