using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Helpers;
using BECOSOFT.Data.Query;
using BECOSOFT.Data.Query.Builders;
using BECOSOFT.Utilities.Collections;
using BECOSOFT.Utilities.Comparers;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BECOSOFT.Data.Models {
    /// <summary>
    /// A class presenting a query with parameters
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ParametrizedQuery {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private int _tempTablesGenerated;
        /// <summary>
        /// The query as a string
        /// </summary>
        public string Query { get; set; }
        /// <summary>
        /// The query that will be used as link between <see cref="Query"/> and <see cref="SelectBaseQueryBuilder"/>.<see cref="BaseQueryBuilder.GenerateQuery"/>
        /// </summary>
        public string BaseTableIDWhereClause { get; set; }
        /// <summary>
        /// A list of parameters
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }
        /// <summary>
        /// List of temporary tables
        /// </summary>
        public List<TempTable<object>> TempTables { get; set; } = new List<TempTable<object>>();
        /// <summary>
        /// List of temporary tables that will be filled using <see cref="SqlBulkCopy"/>.
        /// </summary>
        public List<TempTable<object>> BulkCopyTempTables { get; set; } = new List<TempTable<object>>(0);
        /// <summary>
        /// The tablepart that will be used. Defaults to <see langword="null" />.
        /// </summary>
        public string TablePart { get; set; }

        /// <summary>
        /// Bool indicating whether the result should be distinct. Defaults to <see langword="false" />.
        /// </summary>
        public bool Distinct { get; set; }
        
        /// <summary>
        /// Query timeout (default: <see cref="DatabaseCommand.DefaultQueryTimeout"/>)
        /// </summary>
        public int? Timeout { get; set; }

        /// <summary>
        /// Number of parameters present in <see cref="Parameters"/>
        /// </summary>
        public int ParameterCount => Parameters?.Count ?? 0;

        public ParametrizedQuery() {
            Parameters = new Dictionary<string, object>();
        }

        public ParametrizedQuery(string query) : this(query, new Dictionary<string, object>(), null, false) {
        }

        public ParametrizedQuery(string query, string tablePart, bool distinct) : this(query, new Dictionary<string, object>(), tablePart, distinct) {
        }

        public ParametrizedQuery(string query, Dictionary<string, object> parameters) : this(query, parameters, null, false) {
        }

        public ParametrizedQuery(string query, Dictionary<string, object> parameters, string tablePart, bool distinct) {
            Parameters = new Dictionary<string, object>();
            Query = query;
            TablePart = tablePart;
            Distinct = distinct;
            AddParameters(parameters);
        }

        /// <summary>
        /// Checks if a parameter-key exists in the collection and adds it if it doesn't exists. Returns the <see cref="key"/>.
        /// </summary>
        /// <param name="key">The key of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        public string AddParameter(string key, object value) {
            Parameters.AddOrUpdate(key, value);
            return key;
        }

        /// <summary>
        /// Adds a list of parameters if they don't exist in the collection
        /// </summary>
        /// <param name="parameters">The list of parameters</param>
        public void AddParameters(Dictionary<string, object> parameters) => Parameters.AddRange(parameters);

        /// <summary>
        /// Creates a <see cref="TempTable{T}"/> object from the given <see cref="values"/> and optional <see cref="tableName"/>. 
        /// The column name of the values is 'tempValue'.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public TempTable<object> CreateTempTable<T>(List<T> values, string tableName = null) {
            if (tableName.IsNullOrWhiteSpace()) {
                tableName = GetTableName();
            }
            return new TempTable<object>(typeof(T)) {
                TableName = tableName,
                Values = values.Distinct().Cast<object>().ToList(),
            };
        }
        /// <summary>
        /// Creates a <see cref="TempTable{T}"/> object from the given <see cref="values"/> and optional <see cref="tableName"/>. 
        /// The column name of the values is 'tempValue'.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public TempTable<object> CreateTempTable<T>(IReadOnlyList<T> values, string tableName = null) {
            if (tableName.IsNullOrWhiteSpace()) {
                tableName = GetTableName();
            }
            return new TempTable<object>(typeof(T)) {
                TableName = tableName,
                Values = values.Distinct().Cast<object>().ToList(),
            };
        }

        /// <summary>
        /// Returns a unique table name (to use as <see cref="TempTable{T}"/> or custom temp table)
        /// </summary>
        /// <returns></returns>
        public string GetTableName() {
            var tableName = TempTableHelper.GetTempTableName(ref _tempTablesGenerated);
            return tableName;
        }

        /// <summary>
        /// Creates and adds a <see cref="TempTable{T}"/> object from the given <see cref="values"/> and optional <see cref="tableName"/>.
        /// The column name of the values is 'tempValue'.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="tableName"></param>
        /// <param name="addToBulkCopyWhenOversized">Adds the temp table that will be filled using BulkCopy</param>
        /// <returns></returns>
        public TempTable<object> AddTempTable<T>(List<T> values, string tableName = null, bool addToBulkCopyWhenOversized = true) {
            var tempTable = CreateTempTable(values, tableName);
            if ((addToBulkCopyWhenOversized && values.Count >= 3000) || tempTable.IsBulkCopyable) {
                return AddBulkCopyTempTable(tempTable);
            }
            TempTables.Add(tempTable);
            return tempTable;
        }

        /// <summary>
        /// Creates and adds a <see cref="TempTable{T}"/> object from the given <see cref="values"/> and optional <see cref="tableName"/>.
        /// The column name of the values is 'tempValue'.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="tableName"></param>
        /// <param name="addToBulkCopyWhenOversized">Adds the temp table that will be filled using BulkCopy</param>
        /// <returns></returns>
        public TempTable<object> AddTempTable<T>(IReadOnlyList<T> values, string tableName = null, bool addToBulkCopyWhenOversized = true) {
            var tempTable = CreateTempTable(values, tableName);
            if ((addToBulkCopyWhenOversized && values.Count >= 3000) || tempTable.IsBulkCopyable) {
                return AddBulkCopyTempTable(tempTable);
            }
            TempTables.Add(tempTable);
            return tempTable;
        }

        /// <summary>
        /// Adds a <paramref name="tempTable"/> to the <see cref="TempTables"/>.
        /// The column name of the values is 'tempValue'.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tempTable"></param>
        /// <returns></returns>
        public TempTable<object> AddTempTable(TempTable<object> tempTable) {
            TempTables.Add(tempTable);
            return tempTable;
        }

        /// <summary>
        /// Creates and adds a bulk copy <see cref="TempTable{T}"/> object from the given <see cref="values"/> and optional <see cref="tableName"/>.
        /// The column name of the values is 'tempValue'.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public TempTable<object> AddBulkCopyTempTable<T>(List<T> values, string tableName = null) {
            var tempTable = CreateTempTable(values, tableName);
            BulkCopyTempTables.Add(tempTable);
            return tempTable;
        }

        /// <summary>
        /// Adds a bulk copy <paramref name="tempTable"/> to the <see cref="BulkCopyTempTables"/>.
        /// The column name of the values is 'tempValue'.
        /// </summary>
        /// <param name="tempTable"></param>
        /// <returns></returns>
        public TempTable<object> AddBulkCopyTempTable(TempTable<object> tempTable) {
            BulkCopyTempTables.Add(tempTable);
            return tempTable;
        }

        /// <summary>
        /// Sets the query from a <see cref="StringBuilder"/>
        /// </summary>
        /// <param name="query">The query</param>
        public void SetQuery(StringBuilder query) {
            SetQuery(query.ToString());
        }

        /// <summary>
        /// Sets the query from a <see cref="string"/>
        /// </summary>
        /// <param name="query">The query</param>
        public void SetQuery(string query) {
            Query = query;
        }

        /// <summary>
        /// Set the <see cref="BaseTableIDWhereClause"/> property
        /// </summary>
        /// <param name="query"></param>
        public void SetBaseTableIDWhereClause(StringBuilder query) {
            BaseTableIDWhereClause = query.ToString();
        }

        internal QueryInfo ToQueryInfo() {
            return new QueryInfo {
                PremadeQuery = Query,
                ParameterList = Parameters,
                TempTables = TempTables,
                BulkCopyTempTables = BulkCopyTempTables,
                BaseTableIDWhereClause = BaseTableIDWhereClause,
                TablePart = TablePart,
                IsDistinct = Distinct
            };
        }


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [DebuggerHidden]
        private string DebuggerDisplay => QueryDumper.GetCommandText(this).ToString();


        /// <summary>
        /// Merge the current <see cref="ParametrizedQuery"/> with other <paramref name="parametrizedQueries"/>.
        /// Parameters are remapped and merged (if same type and value) to avoid duplicates.
        /// Temptables are renamed to avoid dupplicates.
        /// The query order is maintained for the <paramref name="parametrizedQueries"/>. They are inserted before the current <see cref="ParametrizedQuery"/>.
        /// </summary>
        /// <param name="parametrizedQueries"></param>
        /// <returns>A new <see cref="ParametrizedQuery"/> containing the parameters, temptables and queries of the provided parameters.</returns>
        public ParametrizedQuery MergedWith(params ParametrizedQuery[] parametrizedQueries) {
            var parameterQueriesToProcess = parametrizedQueries.ToSafeList().Where(p => !ReferenceEquals(p, this)).Distinct(ReferenceEqualityComparer<ParametrizedQuery>.Instance).ToList();
            parameterQueriesToProcess.Add(this);
            if (!CanMerge(parameterQueriesToProcess)) {
                throw new UnmergableParametrizedQueryException();
            }
            var merged = new ParametrizedQuery();
            var query = new StringBuilder();
            var adjustedQueries = MergeParameters(parameterQueriesToProcess, merged);
            adjustedQueries = MergeTempTables(adjustedQueries, merged);
            
            foreach (var parametrizedQuery in adjustedQueries.Values) {
                query.Append(parametrizedQuery).AppendLine();
            }

            var queryString = query.ToString();
            if (queryString.Contains("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;", StringComparison.InvariantCultureIgnoreCase)) {
                query.ReplaceIgnoreCase("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;", "");
                query.Insert(0, "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;\n");
            }
            merged.SetQuery(query);
            return merged;
        }

        private static bool CanMerge(List<ParametrizedQuery> parameterQueriesToProcess) {
            foreach (var parameterQuery in parameterQueriesToProcess) {
                if (parameterQuery.TablePart.HasValue()) {
                    Logger.Error($"Cannot merge parametrized query because one or more have a {nameof(parameterQuery.TablePart)} defined");
                    return false;
                }
                if (parameterQuery.BaseTableIDWhereClause.HasValue()) {
                    Logger.Error($"Cannot merge parametrized query because one or more have a {nameof(parameterQuery.BaseTableIDWhereClause)} defined");
                    return false;
                }
            }
            return true;
        }

        private static KeyValueList<ParametrizedQuery, string> MergeTempTables(KeyValueList<ParametrizedQuery, string> parameterQueriesToProcess, ParametrizedQuery merged) {
            var remappedIndex = 0;
            var temp = RemapTempTable(parameterQueriesToProcess, pq => pq.TempTables);
            temp = RemapTempTable(temp, pq => pq.BulkCopyTempTables);
            return temp;

            KeyValueList<ParametrizedQuery, string> RemapTempTable(KeyValueList<ParametrizedQuery, string> toProcess, Func<ParametrizedQuery, List<TempTable<object>>> tempTableSelector) {
                var tempTablesToRemap = new Dictionary<(int Index, int TempTableIndex), TempTable<object>>();
                var sortedTempTables = new KeyValueList<int, List<TempTable<object>>>();
                for (var qIndex = 0; qIndex < toProcess.Count; qIndex++) {
                    var parameterQuery = toProcess[qIndex];
                    var tempTables = tempTableSelector(parameterQuery.Key);
                    if (tempTables.IsEmpty()) { continue; }
                    var tempTableList = tempTables.OrderByDescending(p => p.TableName.Length).ToList();
                    for (var ttIndex = 0; ttIndex < tempTableList.Count; ttIndex++) {
                        var tempTable = tempTableList[ttIndex];
                        var key = (qIndex, pIndex: ttIndex);
                        tempTablesToRemap[key] = tempTable;
                    }
                    sortedTempTables.Add(qIndex, tempTableList);
                }
                var remappedTempTables = new Dictionary<(int Index, int ParameterIndex), string>();
                foreach (var tempTable in tempTablesToRemap) {
                    var newTempTable = merged.AddTempTable(tempTable.Value.Values, $"#Table_{remappedIndex}_Temp");
                    remappedTempTables.Add(tempTable.Key, newTempTable.TableName);
                    remappedIndex++;
                }
                var result = new KeyValueList<ParametrizedQuery, string>();
                for (var qIndex = 0; qIndex < toProcess.Count; qIndex++) {
                    var parameterQuery = toProcess[qIndex];
                    var queryBuilder = parameterQuery.Value;
                    var pair = sortedTempTables.FirstOrDefault(s => s.Key == qIndex);
                    if (pair.Value != null) {
                        var tempTables = pair.Value;
                        for (var paramIndex = 0; paramIndex < tempTables.Count; paramIndex++) {
                            var tempTable = tempTables[paramIndex];
                            var newName = remappedTempTables.TryGetValueWithDefault((qIndex, paramIndex));
                            if (newName.IsNullOrEmpty()) { continue; }
                            queryBuilder = queryBuilder.ReplaceIgnoreCase(tempTable.TableName, newName);
                        }
                    }
                    result.Add(parameterQuery.Key, queryBuilder);

                }
                return result;
            }
        }

        private static KeyValueList<ParametrizedQuery, string> MergeParameters(List<ParametrizedQuery> parameterQueriesToProcess, ParametrizedQuery merged) {
            var parametersToRemap = new Dictionary<(int Index, int ParameterIndex), (Type Type, object Object)>();
            var sortedParameters = new KeyValueList<int, KeyValueList<string, object>>();
            var parametersWithoutValue = new HashSet<(int Index, int ParameterIndex)>();
            var existingParameters = new HashSet<string>();
            var maxTimeout = parameterQueriesToProcess.Union(new[] { merged }).Select(pq => pq.Timeout).MaxOrDefault();
            for (var qIndex = 0; qIndex < parameterQueriesToProcess.Count; qIndex++) {
                var parameterQuery = parameterQueriesToProcess[qIndex];
                var parameters = parameterQuery.Parameters;
                if (parameters.IsEmpty()) { continue; }
                var parameterList = parameters.OrderByDescending(p => p.Key.Length).ToList();
                for (var pIndex = 0; pIndex < parameterList.Count; pIndex++) {
                    var parameter = parameterList[pIndex];
                    var key = (qIndex, pIndex);
                    if (parameter.Value == null) {
                        parametersWithoutValue.Add(key);
                    } else {
                        parametersToRemap[key] = (parameter.Value.GetType(), parameter.Value);
                    }
                    existingParameters.Add(parameter.Key.ToLowerInvariant());
                }
                sortedParameters.Add(qIndex, parameterList);
            }
            var remappedParameters = new Dictionary<(int Index, int ParameterIndex), string>();
            var remappedIndex = 0;
            foreach (var parameter in parametersWithoutValue) {
                string name;
                do {
                    name = $"@PARAM{remappedIndex}I";
                    remappedIndex++;
                } while (existingParameters.Contains(name.ToLowerInvariant()));
                
                merged.AddParameter(name, null);
                remappedParameters.Add(parameter, name);
            }
            foreach (var grouping in parametersToRemap.GroupBy(pr => pr.Value)) {
                string name;
                do {
                    name = $"@PARAM{remappedIndex}I";
                    remappedIndex++;
                } while (existingParameters.Contains(name.ToLowerInvariant()));
                merged.AddParameter(name, grouping.Key.Object);
                foreach (var pair in grouping) {
                    remappedParameters.Add(pair.Key, name);
                }
            }
            var result = new KeyValueList<ParametrizedQuery, string>();
            for (var qIndex = 0; qIndex < parameterQueriesToProcess.Count; qIndex++) {
                var parameterQuery = parameterQueriesToProcess[qIndex];
                var queryBuilder = parameterQuery.Query;
                var pair = sortedParameters.FirstOrDefault(s => s.Key == qIndex);
                if (pair.Value != null) {
                    for (var paramIndex = 0; paramIndex < pair.Value.Count; paramIndex++) {
                        var parameter = pair.Value[paramIndex];
                        var newName = remappedParameters.TryGetValueWithDefault((qIndex, paramIndex));
                        if (newName.IsNullOrEmpty()) { continue; }
                        queryBuilder = queryBuilder.ReplaceIgnoreCase(parameter.Key, newName);
                    }
                }

                result.Add(parameterQuery, queryBuilder);
            }
            merged.Timeout = maxTimeout;
            return result;
        }
    }
}