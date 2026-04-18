using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Assemblies;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Models.Mapping;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Helpers;
using BECOSOFT.Utilities.Models.Mapping.Filters;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;

namespace BECOSOFT.Data.Query.Mapping {
    public static class FilterContainerQueryBuilder {
        public static FilterContainerQueryResult CreateQuery(FilterContainerQueryParameter parameter) {
            // todo : add specific errors
            if (!parameter.IsValid()) {
                return new FilterContainerQueryResult();
            }

            var parseParameter = parameter.ToParseParameter(false);
            var query = ParseCondition(parseParameter);
            var preselectionQueryResult = CreatePreselectionQuery(parameter);

            return new FilterContainerQueryResult {
                Parameters = parameter,
                Success = true,
                Query = query,
                PreselectionQuery = preselectionQueryResult.Success ? preselectionQueryResult.Query : null,
            };
        }

        private static FilterContainerQueryResult CreatePreselectionQuery(FilterContainerQueryParameter parameter) {
            if (!parameter.IsValid() || !parameter.HasPreselectionTablePart) {
                return new FilterContainerQueryResult();
            }
            var parseParameter = parameter.ToParseParameter(true);
            var query = ParseCondition(parseParameter);
            return new FilterContainerQueryResult {
                Parameters = parameter,
                Success = query != null,
                Query = query,
            };
        }

        private static ParametrizedQuery ParseCondition(FilterContainerParseParameter parameter) {
            var rootCondition = parameter.RootCondition;
            var container = parameter.Container;
            var parametrizedQuery = new ParametrizedQuery();
            var queryResult = ParseCondition(rootCondition, parametrizedQuery, parameter, 0, 0);
            var aliasesSeen = queryResult.TableAliases.ToDistinctList();

            var sb = new StringBuilder();

            var preselectionTablePart = parameter.HasPreselectionTablePart ? "{0}" : null;
            var mainFrom = aliasesSeen.FirstOrDefault(a => a.Item1 == container.MainType);
            if (mainFrom == null) {
                var entityTypeInfo = EntityConverter.GetEntityTypeInfo(container.MainType);
                var table = entityTypeInfo.TableDefinition.GetFullTable(preselectionTablePart);
                if (entityTypeInfo.TableDefinition.Schema == Schema.Dbo) {
                    table = entityTypeInfo.TableDefinition.TableName;
                }
                mainFrom = Tuple.Create(container.MainType, table, container.AliasMapping.TryGetValueWithDefault(container.MainType));
            }
            var mainEntity = EntityConverter.GetEntityTypeInfo(mainFrom.Item1);
            var pkColumnName = mainEntity.IsBaseEntity ? mainEntity.PrimaryKeyInfo.ColumnName : mainEntity.TablePrimaryKey;
            sb.AppendLine(" SELECT DISTINCT {0}.{1} ", mainFrom.Item3, pkColumnName);
            if (container.ExtraResultColumns.HasAny()) {
                foreach (var extraResultColumn in container.ExtraResultColumns) {
                    sb.AppendLine(" , {0}.{1}", mainFrom.Item3, extraResultColumn);
                }
            }
            if (!parameter.IsPreselection && parameter.MainTempTableName.HasValue()) {
                sb.AppendLine(" INTO {0} ", parameter.MainTempTableName);
            }
            sb.AppendLine(" FROM {0} {1} ", mainFrom.Item2, mainFrom.Item3);
            if (parameter.IsPreselection && parameter.QueryOptions.PreselectionJoinPart.HasValue()) {
                sb.AppendLine(parameter.QueryOptions.PreselectionJoinPart);
            } else if (!parameter.IsPreselection && parameter.QueryOptions.JoinPart.HasValue()) {
                sb.AppendLine(parameter.QueryOptions.JoinPart);
            }
            foreach (var join in aliasesSeen) {
                if (join.Equals(mainFrom)) { continue; }
                sb.AppendLine(" LEFT JOIN {0} {1} ON {1}.{2} = {3}.{2} ", join.Item2, join.Item3, pkColumnName, mainFrom.Item3);
            }
            if (queryResult.QueryPart != null) {
                sb.AppendLine(" WHERE ");
                sb.Append(queryResult.QueryPart);
            }
            parametrizedQuery.SetQuery(sb);
            return parametrizedQuery;
        }

        private static FilterConditionResult ParseCondition(FilterCondition parentCondition, ParametrizedQuery parametrizedQuery,
                                                            FilterContainerParseParameter parseParameter, int level, int index) {
            var groupingCondition = parentCondition as FilterGroupingCondition;
            var propertyCondition = parentCondition as FilterPropertyCondition;
            if (groupingCondition == null && propertyCondition == null) {
                throw new UnsupportedException($"Unknown {nameof(parentCondition)} type");
            }
            var result = new FilterConditionResult();
            if (groupingCondition != null) {
                var query = new StringBuilder();
                query.Append("(");
                var isFirst = true;
                for (var i = 0; i < groupingCondition.Conditions.Count; i++) {
                    var condition = groupingCondition.Conditions[i];
                    var parsedCondition = ParseCondition(condition, parametrizedQuery, parseParameter, level + 1, i);
                    if (parsedCondition?.QueryPart == null) { continue; }
                    if (parsedCondition.UsedTableAlias != null) {
                        result.TableAliases.Add(parsedCondition.UsedTableAlias);
                    }
                    if (parsedCondition.TableAliases.HasAny()) {
                        result.TableAliases.AddRange(parsedCondition.TableAliases);
                    }
                    if (!isFirst) {
                        query.AppendFormat(" {0} ", groupingCondition.LogicalGroupingValue.ToString().ToUpper());
                    }
                    query.AppendLine(parsedCondition.QueryPart.ToString());
                    isFirst = false;
                }
                query.Append(")");
                result.QueryPart = query;
                return result;
            }
            if (!propertyCondition.IsValid()) { return null; }

            var container = parseParameter.Container;
            var entity = container.FilterOptions.FirstOrDefault(e => e.Entity.EqualsIgnoreCase(propertyCondition.Entity));
            var property = entity?.Properties.FirstOrDefault(p => p.Property.EqualsIgnoreCase(propertyCondition.Property));
            if (property == null) { return null; }

            var type = Type.GetType(property.ParentClass);
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(type);
            var alias = container.AliasMapping.TryGetValueWithDefault(type); // + (type == container.MainType ? "" : $"_{level}");
            var preselectedValue = propertyCondition.PreselectedValue;
            string tablePart = null;
            if (entityTypeInfo.IsTableConsuming) {
                if (preselectedValue.HasValue && preselectedValue.Value != 0) {
                    alias = $"{alias}{preselectedValue}";
                    tablePart = entity.PreselectionFilterTableNames.TryGetValueWithDefault(preselectedValue.Value);
                } else if (parseParameter.HasPreselectionTablePart) {
                    tablePart = "{0}";
                    alias = alias.FormatWith(tablePart);
                } else {
                    return null;
                }
            } else {
                if (preselectedValue.HasValue && preselectedValue.Value != 0) {
                    alias = $"{alias}{preselectedValue}";
                }
            }
            var table = entityTypeInfo.TableDefinition.GetFullTable(tablePart);
            if (entityTypeInfo.TableDefinition.Schema == Schema.Dbo) {
                table = entityTypeInfo.TableDefinition.TableName;
            }
            var createExists = false;
            var foreignKeyProperty = entity.ForeignKeyProperty;
            if (entity.HasMultipleValues) {
                createExists = true;
                var mainTypeEntityInfo = EntityConverter.GetEntityTypeInfo(container.MainType);
                var mainTypePrimaryKeyColumn = mainTypeEntityInfo.PrimaryKeyInfo.ColumnName;
                foreignKeyProperty = mainTypePrimaryKeyColumn;
            } else if (type == container.MainType || foreignKeyProperty.IsNullOrWhiteSpace()) {
                result.UsedTableAlias = Tuple.Create(type, table, alias);
            } else {
                createExists = true;
            }

            var isHtmlStripped = false;
            var conditionProperty = property.ActualProperty?.NullIf("") ?? property.Property;
            if (conditionProperty.Contains("_HtmlStrippedLength", StringComparison.InvariantCultureIgnoreCase)) {
                conditionProperty = conditionProperty.Substring(0, conditionProperty.LastIndexOf("_", StringComparison.InvariantCultureIgnoreCase));
                isHtmlStripped = true;
            }
            if (conditionProperty.Contains("_HtmlStripped", StringComparison.InvariantCultureIgnoreCase)) {
                conditionProperty = conditionProperty.Substring(0, conditionProperty.LastIndexOf("_", StringComparison.InvariantCultureIgnoreCase));
                isHtmlStripped = true;
            }
            var propertyInfo = entityTypeInfo.GetPropertyInfo(conditionProperty, null);
            if (propertyInfo == null) {
                foreach (var baseChildProperty in entityTypeInfo.LinkedBaseChildProperties) {
                    var baseChildEntityInfo = EntityConverter.GetEntityTypeInfo(baseChildProperty.BaseEntityType);
                    if (baseChildEntityInfo == null) { continue; }
                    propertyInfo = baseChildEntityInfo.GetPropertyInfo(conditionProperty, null);
                    if (propertyInfo != null) {
                        break;
                    }
                }
            }
            if (propertyInfo == null && property.DataTypeValue != FilterDataType.LinkedSelect) {
                return null;
            }

            string preselectionFilterEntity;
            var preselectionFilterProperty = entity.PreselectionFilterProperty;
            if (preselectionFilterProperty == null) {
                preselectionFilterProperty = property.PreselectionFilterProperty;
                preselectionFilterEntity = property.ParentClass;
            } else {
                preselectionFilterEntity = entity.PreselectionFilterEntity;
            }
            var paramLevelPart = $"PARAM_{level}_{index}";
            string preSelectionFilterQuery = null;
            if (preselectionFilterProperty.HasValue() && ((preselectedValue.HasValue && preselectedValue.Value != 0) || entity.PreselectedValuePlaceholder.HasValue())) {
                var temp = new StringBuilder();
                temp.Append("{0}");
                var entityType = Type.GetType(preselectionFilterEntity);
                var eti = EntityConverter.GetEntityTypeInfo(entityType);
                var preselectionProp = eti.GetPropertyInfo(preselectionFilterProperty, null);
                if (preselectedValue.HasValue) {
                    var intParamName = parametrizedQuery.AddParameter($"@{parseParameter.ParameterPrefix}_{paramLevelPart}_{parametrizedQuery.ParameterCount}", preselectedValue.Value);
                    temp.AppendLine("({0}.[{1}] = {2})", alias, preselectionProp.ColumnName, intParamName);
                } else {
                    temp.AppendLine("({0}.[{1}] = {2})", alias, preselectionProp.ColumnName, entity.PreselectedValuePlaceholder);
                }
                temp.Append(" ) ");
                preSelectionFilterQuery = temp.ToString();
            }
            var sb = new StringBuilder();
            var dataType = property.DataTypeValue;
            if (propertyCondition.OperatorValue.IsLengthOperator()) {
                dataType = isHtmlStripped ? FilterDataType.StringStrippedFromHtmlLength : FilterDataType.StringLength;
            }
            var op = propertyCondition.OperatorValue.GetOperator();
            var isNegating = propertyCondition.OperatorValue.IsNegating();
            var existsSb = new StringBuilder();
            var requiresNotExistsCheck = createExists && RequiresNotExistsCheck(propertyCondition.OperatorValue, dataType, propertyCondition.Value, propertyCondition.Values);
            if (createExists) {
                var mainTypeEntityInfo = EntityConverter.GetEntityTypeInfo(container.MainType);
                var mainTypePrimaryKeyColumn = mainTypeEntityInfo.IsBaseEntity ? mainTypeEntityInfo.PrimaryKeyInfo.ColumnName : mainTypeEntityInfo.TablePrimaryKey;
                var mainTypeAlias = container.AliasMapping.TryGetValueWithDefault(container.MainType);
                if (requiresNotExistsCheck) {
                    // RequiresNotExistsCheck performs various logic tests to see if this NOT EXISTS check is required.
                    // checks for the absence of a row in the table.
                    existsSb.AppendLine(" ( ");
                    existsSb.AppendLine(" NOT EXISTS ( ");
                    existsSb.AppendLine("     SELECT 1 ");
                    existsSb.AppendLine("     FROM {0} {1} ", table, alias);
                    existsSb.AppendLine("     WHERE {0}.{1} = {2}.{3} ", alias, foreignKeyProperty, mainTypeAlias, mainTypePrimaryKeyColumn);
                    if (entity.ExtraForeignKeyProperties.HasAny()) {
                        foreach (var extraForeignKeyProperty in entity.ExtraForeignKeyProperties) {
                            existsSb.AppendLine(" AND {0}.{1} = {2}.{3} ", alias, extraForeignKeyProperty, mainTypeAlias, extraForeignKeyProperty);
                        }
                    }
                    if (preselectionFilterProperty.HasValue() && ((preselectedValue.HasValue && preselectedValue.Value != 0) || entity.PreselectedValuePlaceholder.HasValue())) {
                        var temp = new StringBuilder();
                        temp.AppendLine();
                        temp.AppendLine(" {0} ", FilterLogicalGrouping.And.ToString().ToUpper());
                        temp.AppendLine(" ( ");
                        existsSb.Append(preSelectionFilterQuery.FormatWith(temp));
                    }
                    existsSb.AppendLine(" ) ");
                    existsSb.AppendLine(" OR ");
                }
                if ((isNegating && entity.HasMultipleValues) || (dataType == FilterDataType.Exists && !propertyCondition.Value.To<bool>())) {
                    // isNegating && entity.HasMultipleValues:
                    // necessary for multi row tables where we require a NOT EXISTS with a "positive" condition instead of EXISTS with a "negating" condition.
                    // For example: article has tags 1 & 2. If the condition is "NOT IN" tag 1 then the EXISTS with negating condition will always be true because the article has tag 2 that satisfies the condition.
                    // This is not the expected result. The expected result is a check that the product does not have a tag that equals 1, so we need to change it to a NOT EXISTS "IN" tag 1
                    isNegating = false; // don't use isNegating further down in this function
                    existsSb.AppendLine(" NOT ");
                }
                existsSb.AppendLine(" EXISTS ( ");
                existsSb.AppendLine("     SELECT 1 ");
                existsSb.AppendLine("     FROM {0} {1} ", table, alias);
                existsSb.AppendLine("     WHERE {0}.{1} = {2}.{3} ", alias, foreignKeyProperty, mainTypeAlias, mainTypePrimaryKeyColumn);
                if (entity.ExtraForeignKeyProperties.HasAny()) {
                    foreach (var extraForeignKeyProperty in entity.ExtraForeignKeyProperties) {
                        existsSb.AppendLine(" AND {0}.{1} = {2}.{3} ", alias, extraForeignKeyProperty, mainTypeAlias, extraForeignKeyProperty);
                    }
                }
                existsSb.AppendLine("     AND (");
            }
            if (property.CustomQueryPart != null) {
                sb.Append("(").AppendLine(property.CustomQueryPart());
            }
            switch (dataType) {
                case FilterDataType.Exists:
                    if (propertyInfo == null) { return null; }
                    var existsParamName = parametrizedQuery.AddParameter($"@{parseParameter.ParameterPrefix}_{paramLevelPart}_{parametrizedQuery.ParameterCount}", 0);
                    sb.AppendLine("({0} = {0})", existsParamName);
                    break;
                case FilterDataType.Boolean:
                    if (propertyInfo == null) { return null; }
                    var boolCondValue = propertyCondition.Value.To<bool>();
                    var boolParamName = parametrizedQuery.AddParameter($"@{parseParameter.ParameterPrefix}_{paramLevelPart}_{parametrizedQuery.ParameterCount}", boolCondValue);
                    if (boolCondValue) {
                        sb.AppendLine("({0}.[{1}] = {2})", alias, propertyInfo.ColumnName, boolParamName);
                    } else {
                        sb.AppendLine("({0}.[{1}] IS NULL OR {0}.[{1}] = {2})", alias, propertyInfo.ColumnName, boolParamName);
                    }
                    break;
                case FilterDataType.Integer:
                    if (propertyInfo == null) { return null; }
                    var intCondValue = propertyCondition.Value.To<long>();
                    var intParamName = parametrizedQuery.AddParameter($"@{parseParameter.ParameterPrefix}_{paramLevelPart}_{parametrizedQuery.ParameterCount}", intCondValue);
                    if (intCondValue == 0 && propertyCondition.OperatorValue.IsEqualToOperator() && !isNegating) {
                        sb.AppendLine("({0}.[{1}] IS NULL OR {0}.[{1}] {3} {2})", alias, propertyInfo.ColumnName, intParamName, op);
                    } else {
                        sb.AppendLine("({0}.[{1}] {3} {2})", alias, propertyInfo.ColumnName, intParamName, op);
                    }
                    break;
                case FilterDataType.StringLength:
                    if (propertyInfo == null) { return null; }
                    var strLengthCondValue = propertyCondition.Value.To<int>();
                    var strLengthParamName = parametrizedQuery.AddParameter($"@{parseParameter.ParameterPrefix}_{paramLevelPart}_{parametrizedQuery.ParameterCount}", strLengthCondValue);
                    if (strLengthCondValue == 0 && propertyCondition.OperatorValue.IsEqualToOperator() && !isNegating) {
                        sb.AppendLine("({0}.[{1}] IS NULL OR LEN({0}.[{1}]) {3} {2})", alias, propertyInfo.ColumnName, strLengthParamName, op);
                    } else {
                        sb.AppendLine("(ISNULL(LEN({0}.[{1}]), 0) {3} {2})", alias, propertyInfo.ColumnName, strLengthParamName, op);
                    }
                    break;
                case FilterDataType.String:
                    if (propertyInfo == null) { return null; }
                    var strCondValue = propertyCondition.Value.To<string>();
                    if (propertyCondition.OperatorValue == FilterOperator.Contains || propertyCondition.OperatorValue == FilterOperator.NotContains) {
                        strCondValue = $"%{strCondValue}%";
                    }
                    var strParamName = parametrizedQuery.AddParameter($"@{parseParameter.ParameterPrefix}_{paramLevelPart}_{parametrizedQuery.ParameterCount}", strCondValue);
                    if (strCondValue.IsNullOrEmpty() && !isNegating) {
                        sb.AppendLine("({0}.[{1}] IS NULL OR {0}.[{1}] {3} {2})", alias, propertyInfo.ColumnName, strParamName, op);
                    } else {
                        sb.AppendLine("({0}.[{1}] {3} {2})", alias, propertyInfo.ColumnName, strParamName, op);
                    }
                    break;
                case FilterDataType.StringStrippedFromHtml:
                    if (propertyInfo == null) { return null; }
                    var strStripped = propertyCondition.Value.To<string>();
                    if (propertyCondition.OperatorValue == FilterOperator.Contains || propertyCondition.OperatorValue == FilterOperator.NotContains) {
                        strStripped = $"%{strStripped}%";
                    }
                    var strStrippedParamName = parametrizedQuery.AddParameter($"@{parseParameter.ParameterPrefix}_{paramLevelPart}_{parametrizedQuery.ParameterCount}", strStripped);
                    var stripFunctionName = SqlAssemblyFunctions.StripHtmlTags.SqlFunctionName;
                    if (strStripped.IsNullOrEmpty() && !isNegating) {
                        sb.AppendLine("({0}.[{1}] IS NULL OR {4}({0}.[{1}], 1) {3} {2})", alias, propertyInfo.ColumnName, strStrippedParamName, op, stripFunctionName);
                    } else {
                        sb.AppendLine("({4}({0}.[{1}], 1) {3} {2})", alias, propertyInfo.ColumnName, strStrippedParamName, op, stripFunctionName);
                    }
                    break;
                case FilterDataType.StringStrippedFromHtmlLength:
                    if (propertyInfo == null) { return null; }
                    var strStrippedForLength = propertyCondition.Value.To<int>();
                    var strStrippedForLengthParamName = parametrizedQuery.AddParameter($"@{parseParameter.ParameterPrefix}_{paramLevelPart}_{parametrizedQuery.ParameterCount}", strStrippedForLength);
                    var stripForLengthFunctionName = SqlAssemblyFunctions.LengthOfStringStrippedFromHtmlTags.SqlFunctionName;
                    if (strStrippedForLength == 0 && !isNegating) {
                        sb.AppendLine("({0}.[{1}] IS NULL OR {4}({0}.[{1}], 1) {3} {2})", alias, propertyInfo.ColumnName, strStrippedForLengthParamName, op, stripForLengthFunctionName);
                    } else {
                        sb.AppendLine("({4}({0}.[{1}], 1) {3} {2})", alias, propertyInfo.ColumnName, strStrippedForLengthParamName, op, stripForLengthFunctionName);
                    }
                    break;
                case FilterDataType.Date:
                    if (propertyInfo == null || propertyCondition?.Value == null) { return null; }
                    try {
                        var dateValue = JsonConvert.DeserializeObject<FilterDateValue>(propertyCondition.Value.ToString());
                        var date = dateValue.GetDateTime();
                        if (date == null) { return null; }
                        var dateForParam = (date == default(DateTime) || date.IsBaseDate()) ? DateTimeHelpers.BaseDate : date.Value;
                        var dateParamName = parametrizedQuery.AddParameter($"@{parseParameter.ParameterPrefix}_{paramLevelPart}_{parametrizedQuery.ParameterCount}", dateForParam);
                        if ((date == default(DateTime) || date.IsBaseDate()) && propertyCondition.OperatorValue.IsEqualToOperator() && !isNegating) {
                            sb.AppendLine("({0}.[{1}] IS NULL OR {0}.[{1}] {3} {2})", alias, propertyInfo.ColumnName, dateParamName, op);
                        } else {
                            sb.AppendLine("({0}.[{1}] {3} {2})", alias, propertyInfo.ColumnName, dateParamName, op);
                        }
                    } catch (Exception) {
                        return null;
                    }
                    break;
                case FilterDataType.Decimal:
                    if (propertyInfo == null) { return null; }
                    var decCondValue = propertyCondition.Value.To<decimal>();
                    var decParamName = parametrizedQuery.AddParameter($"@{parseParameter.ParameterPrefix}_{paramLevelPart}_{parametrizedQuery.ParameterCount}", decCondValue);
                    if (decCondValue == 0 && propertyCondition.OperatorValue.IsEqualToOperator() && !isNegating) {
                        sb.AppendLine("({0}.[{1}] IS NULL OR {0}.[{1}] {3} {2})", alias, propertyInfo.ColumnName, decParamName, op);
                    } else {
                        sb.AppendLine("({0}.[{1}] {3} {2})", alias, propertyInfo.ColumnName, decParamName, op);
                    }
                    break;
                case FilterDataType.Select:
                    if (propertyInfo == null) { return null; }
                    var objectValues = propertyCondition.Values.Select(v => v.Value).ToList();
                    var tempSelectSb = ParseSelect(parametrizedQuery, paramLevelPart, level, index, 0, propertyCondition.Value, objectValues,
                                                   alias, propertyInfo, parseParameter, isNegating);
                    if (sb.Length != 0) {
                        sb.Append(tempSelectSb);
                    } else {
                        sb = tempSelectSb;
                    }
                    break;
                case FilterDataType.LinkedSelect:
                    if (propertyCondition.Values.IsEmpty()) {
                        return null;
                    }
                    var tempSb = new StringBuilder();
                    if (isNegating) {
                        tempSb.Append(" NOT ");
                    }
                    tempSb.Append(" ( ");
                    List<FilterPossiblePropertyLabel> possibleProperties;
                    EntityTypeInfo typeInfoToUse;
                    if (preselectedValue.HasValue && preselectedValue.Value != 0) {
                        var entityPossibleValue = entity.PreselectionValues.FirstOrDefault(pv => pv.Id == preselectedValue.Value);
                        if (entityPossibleValue == null) { return null; }
                        var entityProp = entityPossibleValue.Properties.FirstOrDefault(p => p.Property == conditionProperty);
                        if (entityProp == null) { return null; }
                        possibleProperties = entityProp.PossibleProperties;
                        typeInfoToUse = entityTypeInfo;
                    } else {
                        if (propertyInfo == null) { return null; }
                        possibleProperties = property.PossibleProperties;
                        typeInfoToUse = EntityConverter.GetEntityTypeInfo(propertyInfo.PropertyType.IsSubclassOf(typeof(BaseChild)) ? propertyInfo.PropertyType : propertyInfo.Parent.EntityType);
                    }
                    for (var i = 0; i < possibleProperties.Count; i++) {
                        var possibleProperty = possibleProperties[i];
                        var possProp = possibleProperty.Property;
                        var filterValue = propertyCondition.Values.FirstOrDefault(fv => fv.Property.EqualsIgnoreCase(possProp));
                        if (filterValue == null) {
                            if (i == 0) { return null; }
                            continue;
                        }
                        var subPropertyInfo = typeInfoToUse.GetPropertyInfo(possProp, null);
                        if (subPropertyInfo == null) { continue; }
                        var selectParseResult = ParseSelect(parametrizedQuery, paramLevelPart, level, index, i, filterValue.Value, filterValue.Values,
                                                            alias, subPropertyInfo, parseParameter, false);
                        if (i != 0) {
                            tempSb.AppendLine(" {0} ", FilterLogicalGrouping.And.ToString().ToUpper());
                        }
                        tempSb.Append(selectParseResult);
                        if (filterValue.Values != null) {
                            /* the "last" possible property filter always contains an array of values. Value is used for filters above.
                               for example: a filter up to group 3
                               group 1: value 5
                               group 2: value 6
                               group 3: values [1, 2]
                            */
                            break;
                        }
                    }
                    tempSb.Append(")");
                    sb = tempSb;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (preselectionFilterProperty.HasValue() && ((preselectedValue.HasValue && preselectedValue.Value != 0) || entity.PreselectedValuePlaceholder.HasValue())) {
                var temp = new StringBuilder();
                temp.AppendLine(" ( ");
                temp.Append(sb);
                temp.AppendLine();
                temp.AppendLine(" {0} ", FilterLogicalGrouping.And.ToString().ToUpper());
                sb = new StringBuilder(preSelectionFilterQuery.FormatWith(temp));
            }
            if (property.CustomQueryPart != null) {
                sb.AppendLine(" ) ");
            }
            existsSb.Append(sb);
            if (createExists) {
                if (requiresNotExistsCheck) {
                    existsSb.AppendLine("     ) ");
                }
                existsSb.AppendLine("     ) ");
                existsSb.AppendLine(" ) ");
            }
            sb = existsSb;
            result.QueryPart = sb;

            return result;
        }

        private static bool RequiresNotExistsCheck(FilterOperator operatorValue, FilterDataType dataType, object conditionValue, List<FilterValue> propertyConditionValues) {
            switch (dataType) {
                case FilterDataType.Integer:
                case FilterDataType.Decimal:
                    var number = conditionValue.To<decimal?>();
                    if (!number.HasValue) {
                        return false;
                    }
                    switch (operatorValue) {
                        case FilterOperator.EqualTo:
                            return number == 0;
                        case FilterOperator.NotEqualTo:
                            return number != 0;
                        case FilterOperator.GreaterThan:
                            return number < 0;
                        case FilterOperator.GreaterThanOrEqualTo:
                            return number <= 0;
                        case FilterOperator.LessThanOrEqualTo:
                        case FilterOperator.LessThan:
                            return true;
                        default:
                            return false;
                    }
                case FilterDataType.String:
                    var text = conditionValue.To<string>();
                    switch (operatorValue) {
                        case FilterOperator.EqualTo:
                            return text.IsNullOrEmpty();
                        case FilterOperator.NotEqualTo:
                            return false;
                        case FilterOperator.Contains:
                            return text.IsNullOrEmpty();
                        case FilterOperator.NotContains:
                            return text.HasValue();
                        default:
                            return false;
                    }
                case FilterDataType.StringStrippedFromHtmlLength:
                case FilterDataType.StringLength:
                    var length = conditionValue.To<int?>();
                    if (!length.HasValue) {
                        return false;
                    }
                    switch (operatorValue) {
                        case FilterOperator.LengthEqualTo:
                            return length == 0;
                        case FilterOperator.LengthNotEqualTo:
                            return length != 0;
                        case FilterOperator.LengthGreaterThan:
                            return length < 0;
                        case FilterOperator.LengthGreaterThanOrEqualTo:
                            return length <= 0;
                        case FilterOperator.LengthLessThan:
                        case FilterOperator.LengthLessThanOrEqualTo:
                            return true;
                        default:
                            return false;
                    }
                case FilterDataType.Date: {
                    try {
                        var dateValue = JsonConvert.DeserializeObject<FilterDateValue>(conditionValue.To<string>());
                        var date = dateValue.GetDateTime();
                        if (!date.HasValue) {
                            return false;
                        }
                        switch (operatorValue) {
                            case FilterOperator.EqualTo:
                                return date == DateTimeHelpers.BaseDate;
                            case FilterOperator.NotEqualTo:
                                return date != DateTimeHelpers.BaseDate;
                            case FilterOperator.GreaterThan:
                                return false;
                            case FilterOperator.GreaterThanOrEqualTo:
                                return date == DateTimeHelpers.BaseDate;
                            case FilterOperator.LessThanOrEqualTo:
                            case FilterOperator.LessThan:
                                return true;
                            default:
                                return false;
                        }
                    } catch (Exception e) {
                        return false;
                    }
                }
                case FilterDataType.Select: {
                    if (operatorValue == FilterOperator.NotIn) {
                        return true;
                    }
                    try {
                        var selectObjectValues = propertyConditionValues.Select(v => v.Value).ToList();
                        if (selectObjectValues.HasAny()) {
                            var (_, seenDefault) = ParseMultiSelectValues(selectObjectValues);
                            return seenDefault;
                        }
                        var singleSelectCondValue = conditionValue.To<int>();
                        return singleSelectCondValue == 0;
                    } catch (Exception e) {
                        return false;
                    }
                }
            }
            return false;
        }

        private static StringBuilder ParseSelect(ParametrizedQuery parametrizedQuery, string paramPart, int level, int index, int subIndex,
                                                 object value, List<object> values, string alias,
                                                 EntityPropertyInfo propertyInfo, FilterContainerParseParameter parseParameter, bool isNegating) {
            var sb = new StringBuilder();
            if (isNegating) {
                sb.AppendLine(" ( NOT ");
            }
            if (values.HasAny()) {
                var (multiSelectValues, seenDefault) = ParseMultiSelectValues(values);
                if (multiSelectValues.Count > 50) {
                    var multiSelectTempTable = parametrizedQuery.AddTempTable(multiSelectValues);
                    if (seenDefault) {
                        sb.AppendLine("({0}.[{1}] IS NULL OR {0}.[{1}] IN (SELECT {3}.tempValue FROM {2} {3}))", alias, propertyInfo.ColumnName, multiSelectTempTable.TableName, $"t{level}_{index}_{subIndex}");
                    } else {
                        sb.AppendLine("({0}.[{1}] IN (SELECT {3}.tempValue FROM {2} {3}))", alias, propertyInfo.ColumnName, multiSelectTempTable.TableName, $"t{level}_{index}_{subIndex}");
                    }
                } else {
                    var multiSelectParams = new List<string>(multiSelectValues.Count);
                    for (var i = 0; i < multiSelectValues.Count; i++) {
                        var multiSelectValue = multiSelectValues[i];
                        var msvParamName = parametrizedQuery.AddParameter($"@{parseParameter.ParameterPrefix}_{paramPart}_{parametrizedQuery.ParameterCount}_{subIndex}_{i}", multiSelectValue);
                        multiSelectParams.Add(msvParamName);
                    }
                    if (seenDefault) {
                        sb.AppendLine("({0}.[{1}] IS NULL OR {0}.[{1}] IN ({2}))", alias, propertyInfo.ColumnName, string.Join(",", multiSelectParams));
                    } else {
                        sb.AppendLine("({0}.[{1}] IN ({2}))", alias, propertyInfo.ColumnName, string.Join(",", multiSelectParams));
                    }
                }
            } else {
                var singleSelectCondValue = value.To<int>();
                var singleSelectParamName = parametrizedQuery.AddParameter($"@{parseParameter.ParameterPrefix}_{paramPart}_{parametrizedQuery.ParameterCount}_{subIndex}", singleSelectCondValue);
                if (singleSelectCondValue == 0) {
                    sb.AppendLine("({0}.[{1}] IS NULL OR {0}.[{1}] = {2})", alias, propertyInfo.ColumnName, singleSelectParamName);
                } else {
                    sb.AppendLine("({0}.[{1}] = {2})", alias, propertyInfo.ColumnName, singleSelectParamName);
                }
            }
            if (isNegating) {
                sb.AppendLine(" OR ({0}.[{1}] IS NULL)", alias, propertyInfo.ColumnName);
                sb.AppendLine(" ) ");
            }
            return sb;
        }

        private static (List<int> multiSelectValues, bool seenDefault) ParseMultiSelectValues(List<object> values) {
            var multiSelectValues = new List<int>();
            var seenDefault = false;
            foreach (var v in values) {
                if (v == null) { continue; }
                var valueStr = v.ToString();
                int item;
                if (valueStr.Contains("{")) {
                    var filterValue = JsonConvert.DeserializeObject<FilterValue>(valueStr);
                    item = filterValue != null ? filterValue.Value.To<int>() : v.To<int>();
                } else {
                    item = v.To<int>();
                }
                if (item == 0) { seenDefault = true; }
                multiSelectValues.Add(item);
            }
            return (multiSelectValues, seenDefault);
        }
    }
}