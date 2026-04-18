using System.Data;

namespace BECOSOFT.Data.Migrator {
    internal interface IAdvisoryLockProvider {
        IDbConnection Begin();
        void End();
        void AcquireAdvisoryLock();
        void ReleaseAdvisoryLock();
    }
}