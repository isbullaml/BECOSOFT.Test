using BECOSOFT.Data.Converters;
using BECOSOFT.Utilities.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Helpers {
    public class PropertyExpressionParser<TEntity> : PropertyExpressionParser {
        private readonly ConcurrentDictionary<string, Expression<Func<TEntity, object>>> PropertySelectors = new ConcurrentDictionary<string, Expression<Func<TEntity, object>>>();
        private readonly EntityTypeInfo _entityTypeInfo;

        internal PropertyExpressionParser() {
            _entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(TEntity));
        }

        public override Expression<Func<T, object>> GetPropertySelector<T>(string propertyName) {
            try {
                var result = PropertySelectors.GetOrAdd(propertyName, CreatePropertySelector);
                return result as Expression<Func<T, object>>;
            } catch (Exception e) {
                //TODO: Invalid property?
                return null;
            }
        }

        private Expression<Func<TEntity, object>> CreatePropertySelector(string propertyName) {
            var argParam = Expression.Parameter(_entityTypeInfo.EntityType, "t");
            Expression nameProperty;
            var propertyInfo = _entityTypeInfo.GetPropertyInfo(propertyName, null);
            if (propertyInfo != null) {
                nameProperty = Expression.Property(argParam, propertyInfo.PropertyName);
                if (propertyInfo.PropertyType.IsEnum || propertyInfo.PropertyType == typeof(DateTime?) || propertyInfo.PropertyType == typeof(DateTime) || propertyInfo.PropertyType.IsValueType) {
                    nameProperty = Expression.Convert(nameProperty, typeof(object));
                }
            } else {
                nameProperty = GetLinkedEntitySelector(argParam, _entityTypeInfo, propertyName);
            }

            return Expression.Lambda<Func<TEntity, object>>(nameProperty, argParam);
        }

        private MemberExpression GetLinkedEntitySelector(ParameterExpression argParam, EntityTypeInfo entityTypeInfo, string propertyName) {
            var result = TryParseExpression(argParam, propertyName, entityTypeInfo.LinkedEntityProperties);
            if (result != null) {
                return result;
            }

            result = TryParseExpression(argParam, propertyName, entityTypeInfo.InverseLinkedEntityProperties);
            if (result != null) {
                return result;
            }

            result = TryParseExpression(argParam, propertyName, entityTypeInfo.LinkedEntitiesProperties);
            if (result != null) {
                return result;
            }

            result = TryParseExpression(argParam, propertyName, entityTypeInfo.LinkedBaseChildProperties);
            if (result != null) {
                return result;
            }

            result = TryParseExpression(argParam, propertyName, entityTypeInfo.LinkedBaseResultProperties);
            if (result != null) {
                return result;
            }

            result = TryParseExpression(argParam, propertyName, entityTypeInfo.LinkedBaseResultsProperties);
            return result;
        }

        private static MemberExpression TryParseExpression(ParameterExpression argParam, string propertyName, List<EntityPropertyInfo> linkedList) {
            foreach (var linkedProperty in linkedList) {
                var linkedEntityTypeInfo = EntityConverter.GetEntityTypeInfo(linkedProperty.BaseEntityType);
                var propertyInfo = linkedEntityTypeInfo.GetPropertyInfo(propertyName, null);
                if (propertyInfo != null) {
                    var linkedPropertySelector = Expression.Property(argParam, linkedProperty.PropertyName);
                    if (linkedProperty.PropertyType.IsGenericList()) {
                        var firstSelector = Expression.Property(linkedPropertySelector, "Item", Expression.Constant(0));
                        return Expression.Property(firstSelector, propertyInfo.PropertyName);
                    }
                    return Expression.Property(linkedPropertySelector, propertyInfo.PropertyName);
                }
            }

            return null;
        }
    }

    public abstract class PropertyExpressionParser {
        public abstract Expression<Func<T, object>> GetPropertySelector<T>(string propertyName);
    }

    public static class PropertyExpressionParserManager {
        private static readonly ConcurrentDictionary<Type, PropertyExpressionParser> Parsers = new ConcurrentDictionary<Type, PropertyExpressionParser>();

        public static PropertyExpressionParser<T> GetExpressionParser<T>() {
            var type = typeof(T);
            var parser = Parsers.GetOrAdd(type, (_) => new PropertyExpressionParser<T>());
            return (PropertyExpressionParser<T>) parser;
        }

        public static Expression<Func<T, object>> GetPropertySelector<T>(string propertyName) {
            var parser = GetExpressionParser<T>();
            return parser.GetPropertySelector<T>(propertyName);
        }
    }
}
