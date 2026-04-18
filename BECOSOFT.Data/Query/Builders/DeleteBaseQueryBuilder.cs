using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Helpers;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Utilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace BECOSOFT.Data.Query.Builders {
    /// <summary>
    /// Query builder a DELETE-statement
    /// </summary>
    internal class DeleteBaseQueryBuilder : BaseQueryBuilder {
        private readonly QueryTranslator _translator = new QueryTranslator();

        public override QueryType Type => QueryType.Delete;

        public DeleteBaseQueryBuilder(IOfflineTableExistsRepository tableExistsRepository)
            : base(tableExistsRepository) {
        }

        /// <inheritdoc />
        protected override StringBuilder GenerateQuery() {
            var queryBuilder = new StringBuilder();
            if (Info.Expression != null) {
                _translator.Translate(Info);
            }
            Info.TempTables = _translator.TempTables;
            Info.BulkCopyTempTables = _translator.BulkCopyTempTables;
            AddTempTableQueries(queryBuilder);
            queryBuilder.AppendFormat("DELETE FROM {0} ", GetTableName(Info.TypeInfo.TableDefinition.FullTableName));
            if (Info.Entity != null) {
                var isEnumerable = Info.Entity.GetType().IsGenericList();
                if (string.IsNullOrEmpty(Info.ColumnName)) {
                    Info.ColumnName = GetPrimaryKey(Info.TypeInfo.PrimaryKeyInfo.ColumnName);
                }
                if (Info.Entity.GetType() == Info.TypeInfo.EntityType) {
                    Info.Entity = Info.TypeInfo.PrimaryKeyInfo.Getter(Info.Entity);
                }
                var paramNames = new List<string>();
                if (isEnumerable) {
                    var list = ((IEnumerable)Info.Entity).Cast<object>().ToList();
                    for (var index = 0; index < list.Count; index++) {
                        paramNames.Add("@" + Info.ColumnName + index);
                    }
                    Info.Entity = list;
                } else {
                    paramNames.Add("@" + Info.ColumnName);
                }
                queryBuilder.AppendFormat("WHERE {0} {1} {2}{3}{4}", Info.ColumnName, isEnumerable ? "IN" : "=",
                                          isEnumerable ? "(" : "", string.Join(", ", paramNames), isEnumerable ? ")" : "");
            } else if (Info.Expression != null) {
                queryBuilder.AppendFormat("WHERE {0}", _translator.WhereClause);
            } else {
                throw new InvalidQueryInfoException();
            }
            AddDropTempTableQueries(queryBuilder);
            return queryBuilder;
        }

        /// <inheritdoc />
        protected override void SetParameters(DatabaseCommand command) {
            var parameters = command.GetParameterDictionary();
            if (Info.Entity != null) {
                var isEnumerable = Info.Entity.GetType().IsGenericList();
                var propertyInfo = Info.TypeInfo.GetPropertyInfo(Info.ColumnName, Info.TablePart);
                if (propertyInfo == null) {
                    throw new ArgumentException();
                }
                if (isEnumerable) {
                    var containerBaseType = Info.Entity.GetType().GetGenericArguments()[0];
                    var list = (IList<object>)Info.Entity;
                    for (var index = 0; index < list.Count; index++) {
                        var colName = propertyInfo.HasFormatSpecifier ? propertyInfo.ColumnName.FormatWith(Info.TablePart) : propertyInfo.ColumnName;

                        var param = new SqlParameter("@" + colName + index, DbTypeConverter.GetSqlTypeFromType(containerBaseType)) {
                            Value = list[index]
                        };
                        SqlParameterHelper.AddParameter(parameters, param);
                    }
                } else {
                    SqlParameterHelper.AddParameter(parameters, propertyInfo, Info.Entity, Info.TablePart);
                }
            }
            if (Info.Expression != null) {
                foreach (var parameter in _translator.Parameters) {
                    var parameterContainerType = parameter.Type;
                    var dbType = DbTypeConverter.GetSqlTypeFromType(parameterContainerType);
                    SqlParameterHelper.AddParameter(parameters, parameter.Name, dbType, parameter.Value);
                }
            }
            command.AddParameters(parameters, true);
        }
    }
}