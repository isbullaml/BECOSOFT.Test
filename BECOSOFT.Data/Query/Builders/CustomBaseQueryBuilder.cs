using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Data.SqlClient;
using System.Text;

namespace BECOSOFT.Data.Query.Builders {
    /// <summary>
    /// Query builder for custom queries
    /// </summary>
    internal class CustomBaseQueryBuilder : SelectBaseQueryBuilder {
        public override QueryType Type => QueryType.Custom;

        public CustomBaseQueryBuilder(IJoinQueryBuilder joinQueryBuilder,
                                      IOfflineTableExistsRepository tableExistsRepository)
            : base(joinQueryBuilder, tableExistsRepository) {
        }

        /// <summary>
        /// Generates a query
        /// </summary>
        /// <returns>The query</returns>
        protected override StringBuilder GenerateQuery() {
            if (Info.PremadeQuery.IsNullOrWhiteSpace()) {
                throw new InvalidQueryInfoException();
            }
            if (Info.BaseTableIDWhereClause.IsNullOrWhiteSpace()) {
                var queryBuilder = new StringBuilder();
                AddTempTableQueries(queryBuilder);
                queryBuilder.AppendLine(Info.PremadeQuery);
                AddDropTempTableQueries(queryBuilder);
                return queryBuilder;
            }
            return GenerateSelectQuery();
        }

        /// <summary>
        /// Sets the parameters for query
        /// </summary>
        /// <param name="command">The command to add the parameters to</param>
        protected override void SetParameters(DatabaseCommand command) {
            if (Info.ParameterList.IsEmpty()) {
                return;
            }
            foreach (var pair in Info.ParameterList) {
                if (pair.Value == null) {
                    command.AddParameter(new SqlParameter(pair.Key, DBNull.Value));
                } else {
                    var param = new SqlParameter(pair.Key, DbTypeConverter.GetSqlTypeFromType(pair.Value.GetType())) {
                        Value = pair.Value
                    };
                    command.AddParameter(param);
                }
            }
        }
    }
}