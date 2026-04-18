using BECOSOFT.Data.Extensions;
using BECOSOFT.Data.Models;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace BECOSOFT.Data.Helpers {
    /// <summary>
    /// Helper class to dump queries
    /// </summary>
    public static class QueryDumper {
        /// <summary>
        /// Gets the full query (CommandText and Parameters definition)
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static StringBuilder GetCommandText(DatabaseCommand command) {
            var commandText = InternalGetCommandText(command.CommandText, command.Parameters, command.BulkCopyTempTables);
            return commandText;
        }

        /// <summary>
        /// Gets the full query (CommandText and Parameters definition)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        internal static StringBuilder GetCommandText(ParametrizedQuery query) {
            var parameters = query.Parameters.Select(p => new SqlParameter(p.Key, p.Value)).ToList();
            var commandText = InternalGetCommandText(query.Query, parameters, query.BulkCopyTempTables);
            return commandText;
        }

        /// <summary>
        /// Gets the full query (CommandText and Parameters definition)
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static StringBuilder GetCommandText(string query, List<SqlParameter> parameters) {
            return InternalGetCommandText(query, parameters, null);
        }

        private static StringBuilder InternalGetCommandText(string query, IReadOnlyList<SqlParameter> parameters, List<TempTable<object>> bulkCopyTempTables) {
            var commandText = new StringBuilder(Environment.NewLine);
            if (query.Contains("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;", StringComparison.InvariantCultureIgnoreCase)) {
                query = query.ReplaceIgnoreCase("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;", "");
                commandText.AppendLine("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;");
            }
            if (bulkCopyTempTables.HasAny()) {
                foreach (var tempTable in bulkCopyTempTables) {
                    commandText.AppendLine(tempTable.GetCreationScript());
                    commandText.Append(tempTable.GetFillScript()).AppendLine();
                }
            }
            if (parameters.HasAny()) {
                for (var index = parameters.Count - 1; index >= 0; index--) {
                    var parameter = parameters[index];
                    if (parameter.Direction == ParameterDirection.ReturnValue) { continue; }
                    commandText.Append("DECLARE ");
                    var name = parameter.ParameterName;
                    if (!name.StartsWith("@")) {
                        name = "@" + name;
                    }
                    commandText.Append(name);
                    commandText.Append(" ");
                    if (parameter.SqlDbType == SqlDbType.Structured) {
                        continue;
                    }
                    commandText.Append(parameter.GetDbTypeDeclaration());
                    commandText.Append(" = ");

                    // Don't log passwords, replace them with '*****' 
                    if (name.Contains("password", StringComparison.InvariantCultureIgnoreCase)) {
                        commandText.Append("*****");
                    } else {
                        commandText.Append(parameter.GetParameterValueForSql());
                    }
                    if (parameter.IsDbTypeString() && parameter.Value != null && parameter.Value != DBNull.Value) {
                        commandText.Append(" COLLATE DATABASE_DEFAULT");
                    }
                    commandText.AppendLine(";");
                }
            }
            commandText.Append(query);
            return commandText;
        }
    }
}