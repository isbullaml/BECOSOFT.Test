using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Helpers;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Repositories.Interfaces;
using System.Data.SqlClient;
using System.Text;

namespace BECOSOFT.Data.Query.Builders {
    internal class UpdatePropertyBaseQueryBuilder : BaseQueryBuilder {
        private readonly QueryTranslator _translator = new QueryTranslator();

        public override QueryType Type => QueryType.UpdateProperty;

        public UpdatePropertyBaseQueryBuilder(IOfflineTableExistsRepository tableExistsRepository)
            : base(tableExistsRepository) {
        }

        /// <inheritdoc />
        protected override StringBuilder GenerateQuery() {
            var queryBuilder = new StringBuilder();

            queryBuilder.AppendFormat("UPDATE {0} SET ", GetTableName(Info.TypeInfo.TableDefinition.FullTableName));
            queryBuilder.AppendFormat("[{0}] = @{1}", Info.ColumnName, Info.ColumnName.Replace("-", "_"));
            if (Info.Expression != null) {
                _translator.Translate(Info);
                Info.TempTables = _translator.TempTables;
                Info.BulkCopyTempTables = _translator.BulkCopyTempTables;
                var whereClause = _translator.WhereClause;
                queryBuilder.AppendFormat($" WHERE {whereClause}");
            } else {
                queryBuilder.AppendFormat(" WHERE [{0}] = @{0} ", Info.TypeInfo.PrimaryKeyInfo.ColumnName);
            }

            return queryBuilder;
        }

        /// <inheritdoc />
        protected override void SetParameters(DatabaseCommand command) {
            var parameters = command.GetParameterDictionary();
            if (Info.Expression != null) {
                foreach (var translatorParameter in _translator.Parameters) {
                    var dbType = DbTypeConverter.GetSqlTypeFromType(translatorParameter.Type);
                    SqlParameterHelper.AddParameter(parameters, translatorParameter.Name, dbType, translatorParameter.Value);
                }
            } else {
                SqlParameterHelper.AddParameter(parameters, Info.TypeInfo.PrimaryKeyInfo, Info.EntityID, Info.TablePart);
            }

            var parameter = new SqlParameter($"@{Info.ColumnName.Replace("-", "_")}", Info.Value);
            SqlParameterHelper.AddParameter(parameters, parameter);

            command.AddParameters(parameters);
        }
    }
}