using BECOSOFT.Data.Helpers;

namespace BECOSOFT.Data.Migrator {
    public class IndexColumn {
        public string ColumnName { get; }
        public bool Descending { get; }

        public IndexColumn(string columnName, bool descending = false) {
            ColumnName = TableHelper.Clean(columnName);
            Descending = descending;
        }

        public string GetIndexPart() {
            return $"[{ColumnName}] {(Descending ? "DESC" : "")}";
        }
    }
}