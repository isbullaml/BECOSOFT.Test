using System;

namespace BECOSOFT.Data.Migrator {
    public interface IMigrationInformationProvider<T> : IBaseMigrationInformationProvider<T> where T : Enum {
    }
}