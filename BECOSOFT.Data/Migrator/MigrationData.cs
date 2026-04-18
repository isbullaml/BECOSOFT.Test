using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace BECOSOFT.Data.Migrator {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class MigrationData<T> where T : Enum {
        internal T Type { get; set; }
        internal int Version { get; set; }
        internal string Description { get; set; }
        internal TypeInfo TypeInfo { get; set; }
        internal IMigration<T> Instance { get; }
        internal IReadOnlyList<int> DynamicEntityIDs => Instance?.DynamicEntityIDs;

        internal MigrationData(T migrationAttributeType, int migrationVersion, string migrationInformation, TypeInfo typeInfo, IMigration<T> instance) {
            Type = migrationAttributeType;
            Version = migrationVersion;
            Description = migrationInformation;
            TypeInfo = typeInfo;
            Instance = instance;
            Instance.SetVersionInfo(Type, Version);
        }

        private MigrationData() {
        }

        public static MigrationData<T> BaseMigration() {
            return new MigrationData<T> {
                Version = 0,
                Description = "",
                TypeInfo = null
            };
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [DebuggerHidden]
        private string DebuggerDisplay => $"{Type} ({Version}) on {TypeInfo.Name}";

        /// <inheritdoc />
        public override string ToString() {
            return DebuggerDisplay;
        }
    }
}