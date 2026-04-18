using BECOSOFT.Data.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace BECOSOFT.Data.Migrator {
    internal interface IMigrationFinder<T> : IBaseService where T : Enum {
        IEnumerable<MigrationData<T>> FindMigrations(T migrationType);
    }
}