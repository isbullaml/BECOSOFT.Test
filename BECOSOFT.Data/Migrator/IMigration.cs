using System;
using System.Collections.Generic;

namespace BECOSOFT.Data.Migrator {
    internal interface IMigration<in T> where T : Enum {
        void RunUpgrade(MigrationRunData data);
        TableConsumingMigrationTableInfo GetTableInfo();
        Type Type { get; }
        void SetVersionInfo(T type, int version);
        IReadOnlyList<int> DynamicEntityIDs { get; }
        void SetRunningFromLinked(bool fromLinked);
    }
}