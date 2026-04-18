using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace BECOSOFT.Data.Query {
    internal class PropertyTranslator : ExpressionVisitor {
        private StringBuilder _sb;
        private EntityTypeInfo _typeInfo;

        /// <summary>
        /// Translates a property selector
        /// </summary>
        /// <param name="selector">The property selector</param>
        /// <returns>The translated query</returns>
        public string Translate<T, TProp>(Expression<Func<T, TProp>> selector) {
            _sb = new StringBuilder();
            _typeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            Visit(selector);
            return _sb.ToString();
        }

        protected override Expression VisitMethodCall(MethodCallExpression m) {
            if (m.Method.DeclaringType == typeof(Sql)) {
                return (new QueryTranslator()).HandleSqlFunctionInternal(m, _sb, this);
            }

            throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
        }

        protected override Expression VisitConstant(ConstantExpression c) {
            var q = c.Value as IQueryable;

            if (q == null && c.Value == null) {
                _sb.Append("NULL");
            } else if (q == null) {
                var type = c.Value.GetType();
                AppendValue(Type.GetTypeCode(type), c.Value, type);
            }
            return c;
        }

        protected override Expression VisitNew(NewExpression node) {
            var c = node.Constructor.Invoke(node.Arguments.Select(a => ((ConstantExpression) a).Value).ToArray());
            AppendValue(Type.GetTypeCode(node.Type), c);
            return node;
        }

        

        /// <inheritdoc />
        protected override Expression VisitMember(MemberExpression m) {
            if (m.Expression == null) {
                if (m.NodeType == ExpressionType.MemberAccess) {
                    var cleanNode = GetMemberConstant(m);
                    return Visit(cleanNode);
                }
                return m;
            }
            if (m.Expression.NodeType == ExpressionType.Parameter) {
                AppendMember(m);
                return m;
            }
            if (m.Expression.NodeType == ExpressionType.MemberAccess) {
                if (m.Expression.Type.IsSubclassOf(typeof(BaseChild))) {
                    AppendMember(m);
                    return m;
                }
                var parentExpression = m.Expression as MemberExpression;
                if (parentExpression != null && _typeInfo.LinkedEntityProperties.Any(l => l.BaseEntityType == parentExpression.Type)) {
                    AppendLinkedEntityMember(m, parentExpression);
                    return m;
                }
            }
            switch (m.Expression.NodeType) {
                case ExpressionType.Constant:
                case ExpressionType.MemberAccess: {
                    var cleanNode = GetMemberConstant(m);
                    return Visit(cleanNode);
                }
                default: {
                    throw new NotSupportedException($"The member '{m.Member.Name}' is not supported");
                }
            }
        }

        private void AddConstantExpressionValue(object value, Type type) {
            if (type.IsEnum) {
                var converter = Converter.GetDelegate(type.GetEnumUnderlyingType());
                var convertedValue = converter(value);
                _sb.Append($"{convertedValue}");
            } else {
                var nullableType = Nullable.GetUnderlyingType(type);
                if (nullableType != null) {
                    var converter = Converter.GetDelegate(nullableType);
                    var convertedValue = converter(value);
                    _sb.Append($"{convertedValue}");
                } else {
                    _sb.Append($"{value}");
                }
            }
        }

        private void AppendValue(TypeCode typeCode, object value, Type type = null) {
            switch (typeCode) {
                case TypeCode.Boolean:
                    var castedValue = value.To<bool>();
                    _sb.Append($"{(castedValue ? "1" : "0")}");
                    break;

                case TypeCode.String:
                    _sb.Append($"'{value}'");
                    break;
                case TypeCode.DateTime:
                    var dt = (DateTime) value;
                    var isDate = dt == dt.Date;
                    if (isDate) {
                        dt = dt.Date;
                    }

                    _sb.Append(isDate ? $"{dt.ToSqlDate()}" : $"{dt.ToSqlDateTime()}");
                    break;
                case TypeCode.Object:
                    if (type.IsNumeric()) {
                        AddConstantExpressionValue(value, type);
                        break;
                    }
                    throw new NotSupportedException($"The constant for '{value}' is not supported");
                default:
                    if (type.IsNumeric()) {
                        AddConstantExpressionValue(value, type);
                    } else {
                        _sb.Append($"{value}");
                    }
                    break;
            }
        }

        private void AppendLinkedEntityMember(MemberExpression m, MemberExpression parentExpression) {
            var linkedEntityType = EntityConverter.GetEntityTypeInfo(parentExpression.Type);
            var linkedEntityProperty = linkedEntityType.GetPropertyInfo(m.Member.Name, null);
            if (linkedEntityProperty == null) {
                return;
            }
            var linkedEntitiesTree = EntityConverter.GetLinkedEntitiesTree(_typeInfo.EntityType, true);
            var linkedEntityTreeNode = linkedEntitiesTree.FirstOrDefault(e => e.Value.EntityTypeInfo.EntityType == linkedEntityType.EntityType);
            if (linkedEntityTreeNode == null) {
                return;
            }
            AppendMember(m);
        }

        private void AppendMember(MemberExpression m) {
            var typeInfo = EntityConverter.GetEntityTypeInfo(m.Expression.Type);
            var propInfo = typeInfo.GetPropertyInfo(m.Member.Name, null);
            var prefix = string.Empty;
            if (m.Expression.NodeType == ExpressionType.Parameter) {
                prefix = (m.Expression as ParameterExpression).Name;
            } else if(m.Expression.NodeType == ExpressionType.MemberAccess) {
                var memberAccess = (m.Expression as MemberExpression);
                prefix = (memberAccess.Expression as ParameterExpression).Name;
            }
            if (propInfo != null) {
                _sb.Append(prefix).Append(".[").Append(propInfo.ColumnName).Append("]");//TODO: .FormatWith(_currentTablePart)
                return;
            }
            _sb.Append(prefix).Append(".[").Append(m.Member.Name).Append("]");
        }

        private static ConstantExpression GetMemberConstant(MemberExpression node) {
            object value;

            if (node.Member.MemberType == MemberTypes.Field) {
                value = GetFieldValue(node);
            } else if (node.Member.MemberType == MemberTypes.Property) {
                value = GetPropertyValue(node);
            } else {
                throw new NotSupportedException();
            }

            return Expression.Constant(value, node.Type);
        }

        private static object GetFieldValue(MemberExpression node) {
            var fieldInfo = (FieldInfo) node.Member;
            var instance = node.Expression == null ? null : TryEvaluate(node.Expression).Value;
            return fieldInfo.GetValue(instance);
        }

        private static object GetPropertyValue(MemberExpression node) {
            var propertyInfo = (PropertyInfo) node.Member;
            var instance = node.Expression == null ? null : TryEvaluate(node.Expression).Value;
            return propertyInfo.GetValue(instance, null);
        }

        private static ConstantExpression TryEvaluate(Expression expression) {
            if (expression.NodeType == ExpressionType.Constant) {
                return (ConstantExpression) expression;
            }
            if (expression.NodeType == ExpressionType.MemberAccess) {
                return GetMemberConstant((MemberExpression) expression);
            }
            if (expression.NodeType == ExpressionType.Parameter) {
            }
            throw new NotSupportedException();
        }
    }
}