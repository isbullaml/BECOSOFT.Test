using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Query.Builders;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Models {
    /// <summary>
    /// Class containing all info to build a query
    /// </summary>
    public class QueryInfo {
        /// <summary>
        /// The info about the type of the entity
        /// </summary>
        public EntityTypeInfo TypeInfo { get; set; }
        /// <summary>
        /// The table-part for table-consuming entities
        /// </summary>
        public string TablePart { get; set; }
        /// <summary>
        /// The entity
        /// </summary>
        public object Entity { get; set; }
        /// <summary>
        /// The entity-ID
        /// </summary>
        public int EntityID { get; set; }
        /// <summary>
        /// The entity
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// The column-name
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// The expression to use in the query
        /// </summary>
        public Expression Expression { get; set; }
        /// <summary>
        /// The list of parameters
        /// </summary>
        public Dictionary<string, object> ParameterList { get; set; }
        /// <summary>
        /// The type of join used in the WHERE-clause
        /// </summary>
        public Operator WhereJoin = Operator.Or;
        /// <summary>
        /// The premade query
        /// </summary>
        public string PremadeQuery { get; set; }
        /// <summary>
        /// List of temporary tables. The column name of the values is 'tempValue'.
        /// </summary>
        public List<TempTable<object>> TempTables { get; set; } = new List<TempTable<object>>();
        /// <summary>
        /// List of temporary tables that will be filled using BulkCopy. The column name of the values is 'tempValue'.
        /// </summary>
        public List<TempTable<object>> BulkCopyTempTables { get; set; } = new List<TempTable<object>>();
        /// <summary>
        /// Only gives distinct results if set to true
        /// </summary>
        public bool IsDistinct { get; set; }
        /// <summary>
        /// List of selected properties to use when updating or selecting from an entity
        /// </summary>
        public ICollection<EntityPropertyInfo> SelectedProperties { get; set; } = new List<EntityPropertyInfo>();
        /// <summary>
        /// Defines the link between <see cref="PremadeQuery"/> and the <see cref="SelectBaseQueryBuilder"/>.<see cref="SelectBaseQueryBuilder.GenerateQuery"/> function.
        /// The <see cref="PremadeQuery"/> will be added after the <see cref="TempTables"/> definitions. Then the default <see cref="SelectBaseQueryBuilder"/> SELECT query will be generated, instead of the WHERE-clause defined by <see cref="Expression"/>, it wil use <see cref="BaseTableIDWhereClause"/> as WHERE-clause.
        /// </summary>
        public string BaseTableIDWhereClause { get; set; }

        /// <summary>
        /// Contains the temp tables to drop (that are not already dropped by <see cref="TempTables"/>).
        /// </summary>
        public List<string> DropOnlyTempTables { get; set; }
    }
}
