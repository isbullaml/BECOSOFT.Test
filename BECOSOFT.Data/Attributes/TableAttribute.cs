using BECOSOFT.Data.Helpers;
using BECOSOFT.Data.Models;
using System;

namespace BECOSOFT.Data.Attributes {
    /// <summary>
    /// Attribute indicating the class is a table
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute {
        public Schema Schema { get; set; }
        /// <summary>
        /// The name of the table in the database
        /// </summary>
        public string Table { get; }
        /// <summary>
        /// The name of the column of the primary key in the database
        /// </summary>
        public string PrimaryKeyColumn { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="schema">Schema of the entity </param>
        /// <param name="table">Table name of the entity. Can include string format specifier {0}.</param>
        /// <param name="primaryKeyColumn">Primary key column name of the table.</param>
        public TableAttribute(Schema schema, string table, string primaryKeyColumn) {
            Schema = schema;
            Table = table;
            PrimaryKeyColumn = primaryKeyColumn;
        }

        public TableDefinition ToDefinition() {
            return TableHelper.GetDefinition(Schema, Table);
        }
    }

    public class ResultTableAttribute : TableAttribute {
        /// <inheritdoc />
        public ResultTableAttribute() : base(Schema.Dbo, "", "") {
        }
    }
}
