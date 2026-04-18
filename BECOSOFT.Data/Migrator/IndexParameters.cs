using System;
using System.Collections.Generic;
using System.Linq;
using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Helpers;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;

namespace BECOSOFT.Data.Migrator {
    public class IndexParameters {
        public IndexType IndexType { get; }
        public Schema Schema { get; }
        public string Table { get; }
        public List<IndexColumn> IndexedColumns { get; }
        public List<IndexColumn> IncludedColumns { get; }
        public string TablePart { get; }
        public bool IsUnique { get; set; }
        public int FillFactor { get; set; }
        public bool DropWhenIncludedDoesNotMatch { get; set; }
        public string FilterClause { get; set; }

        public string FullTable => TableHelper.GetCombined(Schema, Table, TablePart);
        public string CleanedTable => TableHelper.Clean(string.Format(Table, TablePart));

        private IndexParameters(IndexType indexType, Schema schema, string table, List<IndexColumn> indexedColumns = null,
                                List<IndexColumn> includedColumns = null, string tablePart = null) {
            IndexType = indexType;
            Schema = schema;
            Table = table;
            IndexedColumns = indexedColumns ?? new List<IndexColumn>();
            IncludedColumns = includedColumns ?? new List<IndexColumn>();
            TablePart = tablePart;
        }

        public static IndexParameters Create<T>(IndexType indexType, List<IndexColumn> indexedColumns = null, List<IndexColumn> includedColumns = null, string tablePart = null) where T : IEntity {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            return new IndexParameters(indexType, entityTypeInfo.TableDefinition.Schema, entityTypeInfo.TableDefinition.TableName, indexedColumns, includedColumns, tablePart);
        }

        public static IndexParameters Create(IndexType indexType, Schema schema, string table, List<IndexColumn> indexedColumns = null, List<IndexColumn> includedColumns = null, string tablePart = null) {
            return new IndexParameters(indexType, schema, table, indexedColumns, includedColumns, tablePart);
        }

        public void AddIndexColumn(string column, bool descending = false) {
            IndexedColumns.Add(new IndexColumn(column, descending));
        }

        public void AddIncludedColumn(string column) {
            IncludedColumns.Add(new IndexColumn(column));
        }

        public string GetIndexName() {
            string index;

            switch (IndexType) {
                case IndexType.PrimaryKey:
                    index = $"PK_{CleanedTable}_{string.Join("_", IndexedColumns.Select(i => i.ColumnName))}";
                    break;
                case IndexType.ForeignKey:
                    index = $"FK_{CleanedTable}_{string.Join("_", IndexedColumns.Select(i => i.ColumnName))}";
                    break;
                case IndexType.Index:
                    index = $"{CleanedTable}_{string.Join("_", IndexedColumns.Select(i => i.ColumnName))}_{(IncludedColumns.IsEmpty() ? "Index" : "Index_Included")}{(FilterClause.HasNonWhiteSpaceValue() ? "_Filter" : "")}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(IndexType), IndexType, null);
            }

            return TableHelper.Clean(index).Truncate(128, "");
        }
    }
}