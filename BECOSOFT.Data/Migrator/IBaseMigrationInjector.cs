using System;

namespace BECOSOFT.Data.Migrator {
    public interface IBaseMigrationInjector<T> where T : Enum {
        void Inject(MigrationData<T> migrationData);
    }
}