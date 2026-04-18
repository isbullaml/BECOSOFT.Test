using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Helpers;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Utilities.Extensions;
using System;
using System.Text;

namespace BECOSOFT.Data.Query.Builders {
    /// <summary>
    /// Query builder an EXISTS-statement
    /// </summary>
    internal class ExistsBaseQueryBuilder : BaseQueryBuilder {
        private readonly QueryTranslator _translator = new QueryTranslator();

        public override QueryType Type => QueryType.Exists;

        public ExistsBaseQueryBuilder(IOfflineTableExistsRepository tableExistsRepository)
            : base(tableExistsRepository) {
        }

        /// <inheritdoc />
        protected override StringBuilder GenerateQuery() {
            var queryBuilder = new StringBuilder();
            if (Info.Expression != null) {
                _translator.Translate(Info, parameterPrefix: BaseLevelAlias);
            }
            Info.TempTables = _translator.TempTables;
            Info.BulkCopyTempTables = _translator.BulkCopyTempTables;
            AddTempTableQueries(queryBuilder);
            var whereClause = GetWhereClause(_translator);
            var primaryKey = Info.TypeInfo.PrimaryKeyInfo;
            queryBuilder.Append($"SELECT COUNT(DISTINCT {BaseLevelAlias}.[{primaryKey.ColumnName.FormatWith(Info.TablePart)}])");
            queryBuilder.AppendLine().AppendFormat("FROM {0} {1} ", GetTableName(Info.TypeInfo.TableDefinition.FullTableName), BaseLevelAlias);

            if (!string.IsNullOrWhiteSpace(whereClause)) {
                queryBuilder.AppendLine().Append("WHERE ");
                queryBuilder.Append(whereClause);
            }

            AddDropTempTableQueries(queryBuilder);
            return new StringBuilder(queryBuilder.ToString());
        }

        /// <inheritdoc />
        protected override void SetParameters(DatabaseCommand command) {
            var parameters = command.GetParameterDictionary();
            if (Info.Entity != null) {
                EntityPropertyInfo propertyInfo;
                if (Info.ColumnName.IsNullOrWhiteSpace()) {
                    propertyInfo = Info.TypeInfo.PrimaryKeyInfo;
                } else {
                    propertyInfo = Info.TypeInfo.GetPropertyInfo(Info.ColumnName, Info.TablePart);
                    if (propertyInfo == null) {
                        throw new ArgumentException();
                    }
                }

                SqlParameterHelper.AddParameter(parameters, propertyInfo, Info.Entity, Info.TablePart);
            }
            foreach (var translatorParameter in _translator.Parameters) {
                var dbType = DbTypeConverter.GetSqlTypeFromType(translatorParameter.Type);
                SqlParameterHelper.AddParameter(parameters, translatorParameter.Name, dbType, translatorParameter.Value);
            }

            command.AddParameters(parameters, true);
        }

        private string GetWhereClause(QueryTranslator translator) {
            string whereClause = null;
            if (Info.Expression != null) {
                whereClause = translator.WhereClause;
            } else if (Info.Entity != null) {
                var columnName = Info.ColumnName;
                if (string.IsNullOrWhiteSpace(columnName)) {
                    columnName = Info.TypeInfo.PrimaryKeyInfo.ColumnName;
                }
                whereClause = $"{BaseLevelAlias}.[{columnName}] = @{columnName}";
            }
            return whereClause;
        }
    }
}