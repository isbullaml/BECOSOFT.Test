using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Models.Mapping.Filters;
using BECOSOFT.Utilities.Models.Promotions;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BECOSOFT.Utilities.Promotions {
    public static class FilterConditionHelper {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public static Func<T, bool> GenerateFunction<T>(PromotionConditionWrapper conditionWrapper) {
            if (conditionWrapper?.FilterConditionContainer?.Condition == null || conditionWrapper.PromotionArticleContainer == null) {
                return arg => false;
            }

            var parseParameter = new FilterContainerParseParameter {
                Container = conditionWrapper.PromotionArticleContainer,
                RootCondition = conditionWrapper.FilterConditionContainer.Condition,
            };

            return GenerateFunction<T>(parseParameter);
        }

        public static Func<T, bool> GenerateFunction<T>(FilterContainerParseParameter parameter) {
            try {
                var condition = ParseCondition<T>(parameter);
                return condition.Compile();
            } catch (Exception e) {
                Logger.Error(e);
                return arg => false;
            }
        }

        private static Expression<Func<T, bool>> ParseCondition<T>(FilterContainerParseParameter parameter) {
            var rootCondition = parameter.RootCondition;
            var result = ParseCondition<T>(rootCondition, parameter);
            return result;
        }

        private static Expression<Func<T, bool>> ParseCondition<T>(FilterCondition parentCondition, FilterContainerParseParameter parseParameter) {
            var groupingCondition = parentCondition as FilterGroupingCondition;
            var propertyCondition = parentCondition as FilterPropertyCondition;
            if (groupingCondition == null && propertyCondition == null) {
                return null;
            }

            if (groupingCondition != null) {
                Expression<Func<T, bool>> conditionExpression = null;
                foreach (var condition in groupingCondition.Conditions) {
                    var tempExpression = ParseCondition<T>(condition, parseParameter);
                    if (groupingCondition.LogicalGroupingValue == FilterLogicalGrouping.Or) {
                        conditionExpression = conditionExpression.OrElse(tempExpression);
                    } else {
                        conditionExpression = conditionExpression.AndAlso(tempExpression);
                    }
                }

                return conditionExpression;
            }

            if (!propertyCondition.IsValid()) { return null; }

            var container = parseParameter.Container;
            var entity = container.FilterOptions.FirstOrDefault(e => e.Entity.EqualsIgnoreCase(propertyCondition.Entity));
            var property = entity?.Properties.FirstOrDefault(p => p.Property.EqualsIgnoreCase(propertyCondition.Property));
            if (property == null) { return null; }

            var type = Type.GetType(property.ParentClass);
            if (type == null) {
                return null;
            }

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var propertyInfo = properties.FirstOrDefault(p => p.Name.EqualsIgnoreCase(property.Property));
            if (propertyInfo == null) {
                return null;
            }

            if (container.MainType == null) {
                return null;
            }

            var instance = Expression.Parameter(container.MainType, "t");
            MemberExpression subPropertyExpression = null;
            if (type != container.MainType) {
                var subProperties = container.MainType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var subProperty = subProperties.FirstOrDefault(p => p.PropertyType == type);
                if (subProperty == null) {
                    return null;
                }

                subPropertyExpression = Expression.Property(instance, subProperty);
            }

            var dataType = property.DataTypeValue;
            if (propertyCondition.OperatorValue.IsLengthOperator()) {
                dataType = FilterDataType.StringLength;
            }

            var parentSelector = (Expression)subPropertyExpression ?? instance;
            switch (dataType) {
                case FilterDataType.Boolean:
                    return HandleBooleanValue<T>(propertyInfo, propertyCondition, parentSelector, instance);
                case FilterDataType.Integer:
                    return HandleIntegerValue<T>(propertyInfo, propertyCondition, parentSelector, instance);
                case FilterDataType.StringLength:
                    return HandleStringLengthValue<T>(propertyInfo, propertyCondition, parentSelector, instance);
                case FilterDataType.String:
                    return HandleStringValue<T>(propertyInfo, propertyCondition, parentSelector, instance);
                case FilterDataType.Date:
                    return HandleDateValue<T>(propertyInfo, propertyCondition, parentSelector, instance);
                case FilterDataType.Decimal:
                    return HandleDecimalValue<T>(propertyInfo, propertyCondition, parentSelector, instance);
                case FilterDataType.Select:
                    return HandleSelectValue<T>(propertyInfo, propertyCondition, parentSelector, instance);
                case FilterDataType.Search:
                    return HandleSelectValue<T>(propertyInfo, propertyCondition, parentSelector, instance);
                case FilterDataType.LinkedSelect:
                    var isNegating = propertyCondition.OperatorValue.IsNegating();
                    var parentProperty = properties.FirstOrDefault(p => p.Name.EqualsIgnoreCase(property.Property));
                    if (parentProperty == null) {
                        return null;
                    }

                    parentSelector = Expression.Property(instance, parentProperty);
                    return HandleLinkedSelectValue<T>(property, propertyInfo, propertyCondition, parentSelector, instance, isNegating);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Expression<Func<T, bool>> HandleBooleanValue<T>(PropertyInfo propertyInfo, FilterPropertyCondition propertyCondition,
                                                                       Expression parentSelector, ParameterExpression instance) {
            if (propertyInfo == null) { return null; }
            var conditionValue = propertyCondition.Value.To<bool>();
            var propertyExpression = Expression.Property(parentSelector, propertyInfo);
            var valueExpression = Expression.Constant(conditionValue, typeof(bool));

            BinaryExpression binaryExpression;
            switch (propertyCondition.OperatorValue) {
                case FilterOperator.Is:
                    binaryExpression = Expression.Equal(propertyExpression, valueExpression);
                    break;
                default:
                    return null;
            }

            return Expression.Lambda<Func<T, bool>>(binaryExpression, instance);
        }

        private static Expression<Func<T, bool>> HandleIntegerValue<T>(PropertyInfo propertyInfo, FilterPropertyCondition propertyCondition,
                                                                       Expression parentSelector, ParameterExpression instance) {
            if (propertyInfo == null) { return null; }
            var conditionValue = propertyCondition.Value.To<int>();
            var propertyExpression = Expression.Property(parentSelector, propertyInfo);
            var valueExpression = Expression.Constant(conditionValue, typeof(int));

            Expression binaryExpression;
            switch (propertyCondition.OperatorValue) {
                case FilterOperator.EqualTo:
                    binaryExpression = Expression.Equal(propertyExpression, valueExpression);
                    break;
                case FilterOperator.NotEqualTo:
                    binaryExpression = Expression.NotEqual(propertyExpression, valueExpression);
                    break;
                case FilterOperator.GreaterThan:
                    binaryExpression = Expression.GreaterThan(propertyExpression, valueExpression);
                    break;
                case FilterOperator.GreaterThanOrEqualTo:
                    binaryExpression = Expression.GreaterThanOrEqual(propertyExpression, valueExpression);
                    break;
                case FilterOperator.LessThan:
                    binaryExpression = Expression.LessThan(propertyExpression, valueExpression);
                    break;
                case FilterOperator.LessThanOrEqualTo:
                    binaryExpression = Expression.LessThanOrEqual(propertyExpression, valueExpression);
                    break;
                default:
                    return null;
            }

            return Expression.Lambda<Func<T, bool>>(binaryExpression, instance);
        }

        private static Expression<Func<T, bool>> HandleDecimalValue<T>(PropertyInfo propertyInfo, FilterPropertyCondition propertyCondition,
                                                                       Expression parentSelector, ParameterExpression instance) {
            if (propertyInfo == null) { return null; }
            var conditionValue = propertyCondition.Value.To<decimal>();
            var propertyExpression = Expression.Property(parentSelector, propertyInfo);
            var valueExpression = Expression.Constant(conditionValue, typeof(decimal));

            Expression binaryExpression;
            switch (propertyCondition.OperatorValue) {
                case FilterOperator.EqualTo:
                    binaryExpression = Expression.Equal(propertyExpression, valueExpression);
                    break;
                case FilterOperator.NotEqualTo:
                    binaryExpression = Expression.NotEqual(propertyExpression, valueExpression);
                    break;
                case FilterOperator.GreaterThan:
                    binaryExpression = Expression.GreaterThan(propertyExpression, valueExpression);
                    break;
                case FilterOperator.GreaterThanOrEqualTo:
                    binaryExpression = Expression.GreaterThanOrEqual(propertyExpression, valueExpression);
                    break;
                case FilterOperator.LessThan:
                    binaryExpression = Expression.LessThan(propertyExpression, valueExpression);
                    break;
                case FilterOperator.LessThanOrEqualTo:
                    binaryExpression = Expression.LessThanOrEqual(propertyExpression, valueExpression);
                    break;
                default:
                    return null;
            }

            return Expression.Lambda<Func<T, bool>>(binaryExpression, instance);
        }

        private static Expression<Func<T, bool>> HandleSelectValue<T>(PropertyInfo propertyInfo, FilterPropertyCondition propertyCondition,
                                                                      Expression parentSelector, ParameterExpression instance) {
            if (propertyInfo == null) { return null; }
            List<int> conditionValue;
            if (propertyCondition.Value is List<int> test) {
                conditionValue = test;
            } else {
                if (propertyCondition.Value is List<object> list) {
                    if (list.HasAny() && list[0].ToString().Contains("{")) {
                        conditionValue = new List<int>(list.Count);
                        foreach (var item in list) {
                            var value = JsonConvert.DeserializeObject<FilterValue>(item.ToString());
                            conditionValue.Add(value.Value.To<int>());
                        }
                    } else {
                        conditionValue = list.Select(t => t.To<int>()).ToList();
                    }
                } else if (propertyCondition.Values.HasAny()) {
                    conditionValue = propertyCondition.Values.Select(p => p.Value.To<int>()).ToSafeList();
                } else {
                    conditionValue = propertyCondition.Value.To<string>().ToSplitList<int>();
                }
            }

            var propertyExpression = Expression.Property(parentSelector, propertyInfo);
            MethodCallExpression checkExpression;
            var listType = typeof(List<int>);
            var valueExpression = Expression.Constant(conditionValue, listType);
            if (propertyInfo.PropertyType.IsGenericList()) {
                var enumerableType = typeof(Enumerable);
                var genericIntersectMethod = enumerableType.GetMethods().Where(m => m.Name == "Intersect" && m.GetParameters().Length == 2).OrderBy(m => m.GetGenericArguments().Length).First();
                var intersectMethod = genericIntersectMethod.MakeGenericMethod(typeof(int));
                var genericAnyMethod = enumerableType.GetMethods().Where(m => m.Name == "Any" && m.GetParameters().Length == 1).OrderBy(m => m.GetGenericArguments().Length).First();
                var anyMethod = genericAnyMethod.MakeGenericMethod(typeof(int));
                checkExpression = Expression.Call(anyMethod, Expression.Call(intersectMethod, valueExpression, propertyExpression));
            } else {
                var containsMethod = listType.GetMethod("Contains", new[] { typeof(int) });
                checkExpression = Expression.Call(valueExpression, containsMethod, propertyExpression);
            }

            Expression binaryExpression;
            switch (propertyCondition.OperatorValue) {
                case FilterOperator.In:
                    binaryExpression = checkExpression;
                    break;
                case FilterOperator.NotIn:
                    binaryExpression = Expression.Not(checkExpression);
                    break;
                default:
                    return null;
            }

            return Expression.Lambda<Func<T, bool>>(binaryExpression, instance);
        }

        private static Expression<Func<T, bool>> HandleLinkedSelectValue<T>(FilterProperty property, PropertyInfo propertyInfo, FilterPropertyCondition propertyCondition,
                                                                            Expression parentSelector, ParameterExpression instance, bool isNegating) {
            if (propertyInfo == null) { return null; }

            if (propertyCondition.Values.IsEmpty()) {
                return null;
            }

            var possibleProperties = property.PossibleProperties;
            var typeInfoToUse = propertyInfo.PropertyType;
            Expression<Func<T, bool>> result = null;
            for (var i = 0; i < possibleProperties.Count; i++) {
                var possibleProperty = possibleProperties[i];
                var possProp = possibleProperty.Property;
                var filterValue = propertyCondition.Values.FirstOrDefault(fv => fv.Property.EqualsIgnoreCase(possProp));
                if (filterValue == null) {
                    if (i == 0) { return null; }
                    continue;
                }

                var subPropertyInfo = typeInfoToUse.GetProperty(possProp, BindingFlags.Public | BindingFlags.Instance);
                var subPropertyCondition = new FilterPropertyCondition {
                    OperatorValue = FilterOperator.In,
                    Value = filterValue.Values.HasAny() ? filterValue.Values : (filterValue.Value.To<int>() == 0 ? new List<object>(0) : new List<object> { filterValue.Value })
                };
                var subPropertyExpression = HandleSelectValue<T>(subPropertyInfo, subPropertyCondition, parentSelector, instance);
                result = result.AndAlso(subPropertyExpression);
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

            if (isNegating && result != null) {
                var negatingExpression = Expression.Not(result.Body);
                result = Expression.Lambda<Func<T, bool>>(negatingExpression, result.Parameters[0]);
            }

            return result;
        }

        private static Expression<Func<T, bool>> HandleStringValue<T>(PropertyInfo propertyInfo, FilterPropertyCondition propertyCondition,
                                                                      Expression parentSelector, ParameterExpression instance) {
            if (propertyInfo == null) { return null; }
            var conditionValue = propertyCondition.Value.To<string>();
            var propertyExpression = Expression.Property(parentSelector, propertyInfo);
            var valueExpression = Expression.Constant(conditionValue, typeof(string));
            var stringComparisonParameter = Expression.Constant(StringComparison.InvariantCultureIgnoreCase, typeof(StringComparison));

            Expression binaryExpression;
            switch (propertyCondition.OperatorValue) {
                case FilterOperator.EqualTo:
                    binaryExpression = Expression.Equal(propertyExpression, valueExpression);
                    break;
                case FilterOperator.NotEqualTo:
                    binaryExpression = Expression.NotEqual(propertyExpression, valueExpression);
                    break;
                case FilterOperator.Contains:
                    var containsMethodInfo = typeof(StringExtensions).GetMethod(nameof(StringExtensions.Contains), new[] { typeof(string), typeof(string), typeof(StringComparison) });
                    binaryExpression = Expression.Call(containsMethodInfo, propertyExpression, valueExpression, stringComparisonParameter);
                    break;
                case FilterOperator.NotContains:
                    var notContainsMethodInfo = typeof(StringExtensions).GetMethod(nameof(StringExtensions.Contains), new[] { typeof(string), typeof(string), typeof(StringComparison) });
                    binaryExpression = Expression.Call(notContainsMethodInfo, propertyExpression, valueExpression, stringComparisonParameter);
                    break;
                case FilterOperator.StartsWith:
                    var startsWithMethodInfo = typeof(string).GetMethod(nameof(string.StartsWith), BindingFlags.Public, null, new[] { typeof(string), typeof(StringComparison) }, null);
                    binaryExpression = Expression.Call(propertyExpression, startsWithMethodInfo, valueExpression, stringComparisonParameter);
                    break;
                case FilterOperator.EndsWith:
                    var endsWithMethodInfo = typeof(string).GetMethod(nameof(string.EndsWith), BindingFlags.Public, null, new[] { typeof(string), typeof(StringComparison) }, null);
                    binaryExpression = Expression.Call(propertyExpression, endsWithMethodInfo, valueExpression, stringComparisonParameter);
                    break;
                default:
                    return null;
            }

            return Expression.Lambda<Func<T, bool>>(binaryExpression, instance);
        }

        private static Expression<Func<T, bool>> HandleStringLengthValue<T>(PropertyInfo propertyInfo, FilterPropertyCondition propertyCondition,
                                                                            Expression parentSelector, ParameterExpression instance) {
            if (propertyInfo == null) { return null; }
            var conditionValue = propertyCondition.Value.To<int>();
            var propertyExpression = Expression.Property(parentSelector, propertyInfo);
            var lengthPropertyExpression = Expression.Property(propertyExpression, nameof(string.Length));
            var valueExpression = Expression.Constant(conditionValue, typeof(int));

            Expression binaryExpression;
            switch (propertyCondition.OperatorValue) {
                case FilterOperator.LengthEqualTo:
                    binaryExpression = Expression.Equal(lengthPropertyExpression, valueExpression);
                    break;
                case FilterOperator.LengthNotEqualTo:
                    binaryExpression = Expression.NotEqual(lengthPropertyExpression, valueExpression);
                    break;
                case FilterOperator.LengthGreaterThan:
                    binaryExpression = Expression.GreaterThan(lengthPropertyExpression, valueExpression);
                    break;
                case FilterOperator.LengthGreaterThanOrEqualTo:
                    binaryExpression = Expression.GreaterThanOrEqual(lengthPropertyExpression, valueExpression);
                    break;
                case FilterOperator.LengthLessThan:
                    binaryExpression = Expression.LessThan(lengthPropertyExpression, valueExpression);
                    break;
                case FilterOperator.LengthLessThanOrEqualTo:
                    binaryExpression = Expression.LessThanOrEqual(lengthPropertyExpression, valueExpression);
                    break;
                default:
                    return null;
            }

            return Expression.Lambda<Func<T, bool>>(binaryExpression, instance);
        }

        private static Expression<Func<T, bool>> HandleDateValue<T>(PropertyInfo propertyInfo, FilterPropertyCondition propertyCondition,
                                                                    Expression parentSelector, ParameterExpression instance) {
            if (propertyInfo == null) { return null; }
            var conditionValue = propertyCondition.Value.To<DateTime>();
            var propertyExpression = Expression.Property(parentSelector, propertyInfo);
            var valueExpression = Expression.Constant(conditionValue, typeof(DateTime));

            Expression binaryExpression;
            switch (propertyCondition.OperatorValue) {
                case FilterOperator.EqualTo:
                    binaryExpression = Expression.Equal(propertyExpression, valueExpression);
                    break;
                case FilterOperator.NotEqualTo:
                    binaryExpression = Expression.NotEqual(propertyExpression, valueExpression);
                    break;
                case FilterOperator.GreaterThan:
                    binaryExpression = Expression.GreaterThan(propertyExpression, valueExpression);
                    break;
                case FilterOperator.GreaterThanOrEqualTo:
                    binaryExpression = Expression.GreaterThanOrEqual(propertyExpression, valueExpression);
                    break;
                case FilterOperator.LessThan:
                    binaryExpression = Expression.LessThan(propertyExpression, valueExpression);
                    break;
                case FilterOperator.LessThanOrEqualTo:
                    binaryExpression = Expression.LessThanOrEqual(propertyExpression, valueExpression);
                    break;
                default:
                    return null;
            }

            return Expression.Lambda<Func<T, bool>>(binaryExpression, instance);
        }
    }
}