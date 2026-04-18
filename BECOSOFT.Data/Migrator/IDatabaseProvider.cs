using System;
using System.Collections.Generic;
using System.Data;

namespace BECOSOFT.Data.Migrator {
    internal interface IDatabaseProvider<in T> where T : Enum {
        IDbConnection Begin();
        void End();

        int EnsurePrerequisitesCreatedAndGetCurrentVersion(T type);
        int GetCurrentVersion(T type);
        Dictionary<int, int> GetCurrentDynamicVersions(T type, TableConsumingMigrationTableInfo tableInfo);
        void UpdateVersion(int oldVersion, int newVersion, string newDescription, T type);
        void UpdateDynamicVersion(int newVersion, T type, IReadOnlyList<int> entityIDs);
    }
}