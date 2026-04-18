using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Models;

namespace BECOSOFT.Data.Migrator {
    public class TableConsumingMigrationTableInfo {
        public TableDefinition Table { get; }
        public EntityPropertyInfo PrimaryKey { get; }

        public TableConsumingMigrationTableInfo(TableDefinition table, EntityPropertyInfo primaryKey) {
            Table = table;
            PrimaryKey = primaryKey;
        }
    }
}