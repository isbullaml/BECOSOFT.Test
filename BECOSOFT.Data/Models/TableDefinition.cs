using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Helpers;
using System;
using System.Diagnostics;

namespace BECOSOFT.Data.Models {
    [DebuggerDisplay("{FullTableName}")]
    public readonly struct TableDefinition : IEquatable<TableDefinition> {
        public Schema Schema { get; }
        public string TableName { get; }
        public string FullTableName => TableHelper.GetCombined(Schema, TableName);

        public TableDefinition(Schema schema, string tableName) {
            Schema = schema;
            TableName = tableName;
        }


        /// <summary>
        /// Returns the full table name. The table name is escaped for Enter-characters and Sql.
        /// </summary>
        /// <param name="tablePart">If defined, the <see cref="tablePart"/> is inserted into placeholder in the table name</param>
        /// <returns></returns>
        public string GetFullTable(string tablePart) {
            return string.Format(FullTableName, tablePart);
        }

        public override string ToString() {
            return FullTableName;
        }

        public bool Equals(TableDefinition other) {
            return Schema == other.Schema && TableName == other.TableName;
        }

        public override bool Equals(object obj) {
            return obj is TableDefinition other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked { return ((int)Schema * 397) ^ (TableName != null ? TableName.GetHashCode() : 0); }
        }
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly struct TypeTablePartDefinition : IEquatable<TypeTablePartDefinition> {
        public Type Type { get; }
        public string TablePart { get; }
        public TableDefinition TableDefinition { get; }

        public TypeTablePartDefinition(Type type, string tablePart) {
            Type = type;
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(type);
            TableDefinition = entityTypeInfo.TableDefinition;
            TablePart = tablePart; 
        }

        private string DebuggerDisplay => $"{Type.Name} - {TableDefinition.GetFullTable(TablePart)}";

        public bool Equals(TypeTablePartDefinition other) {
            return Type == other.Type && TablePart == other.TablePart;
        }

        public override bool Equals(object obj) {
            return obj is TypeTablePartDefinition other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked { return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ (TablePart != null ? TablePart.GetHashCode() : 0); }
        }
    }
}