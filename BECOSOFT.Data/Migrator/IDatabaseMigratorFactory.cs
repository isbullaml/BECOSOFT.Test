using System;

namespace BECOSOFT.Data.Migrator {
    public interface IDatabaseMigratorFactory<T> where T : Enum {
        IMigrator<T> CreateMigrator();
    }
}