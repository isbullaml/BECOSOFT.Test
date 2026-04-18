using BECOSOFT.Data.Collections;
using BECOSOFT.Utilities.Converters;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BECOSOFT.Data.Parsers {
    public class ConvertibleParser<T> where T : IConvertible {
        protected static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        internal IPagedList<T> SelectConvertible(IDataReader reader) {
            Logger.Debug("Begin Select<IConvertible>");
            var pagedList = new PagedList<T>();
            if (ReadPageInfo(reader, pagedList)) {
                reader.NextResult();
            }

            while (reader.Read()) {
                var value = reader.GetValue(0).To<T>();
                pagedList.Items.Add(value);
            }
            Logger.Debug("End Select<IConvertible> with {0} result items", pagedList.Count);
            return pagedList;
        }

        protected static bool ReadPageInfo(IDataReader reader, PagedList<T> pagedList) {
            if (reader.FieldCount != 3) { return false; }
            var columnIndices = GetReaderColumnsAndIndices(reader);
            if (!columnIndices.ContainsKey("pagertotalitemcount")) { return false; }
            var valueContainer = new object[reader.FieldCount];
            reader.Read();
            reader.GetValues(valueContainer);
            ExtractPageInfo(valueContainer, pagedList, columnIndices);
            return true;
        }

        protected static Dictionary<string, int> GetReaderColumnsAndIndices(IDataReader reader) {
            var table = reader.GetSchemaTable() ?? new DataTable();
            return table.AsEnumerable()
                        .GroupBy(row => row["ColumnName"].ToString().ToLowerInvariant())
                        .ToDictionary(r => r.Key, row => row.First()["ColumnOrdinal"].To<int>());
        }

        protected static void ExtractPageInfo(object[] valueContainer, IPagedList<T> pagedList, IDictionary<string, int> columnIndices) {
            pagedList.SetPageInfo(valueContainer[columnIndices["pagercurrentskip"]].To<int>(),
                                  valueContainer[columnIndices["pagercurrenttake"]].To<int>(),
                                  valueContainer[columnIndices["pagertotalitemcount"]].To<int>());
        }
    }
}
