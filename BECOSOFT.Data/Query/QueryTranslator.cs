using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Models.Global;
using BECOSOFT.Data.Models.QueryData;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace BECOSOFT.Data.Query {
    /// <summary>
    /// Class for translating expressions into a query
    /// </summary>
    internal class QueryTranslator : ExpressionVisitor {
        private StringBuilder _sb;
        private const string ParamPrefix = "PARAM";
        private readonly ServerVersion _serverVersion;

        private int _tempTablesGenerated;

        /// <summary>
        /// The parameters for the query
        /// </summary>
        internal HashSet<ParameterContainer> Parameters = new HashSet<ParameterContainer>();
        /// <summary>
        /// The amount of rows to skip
        /// </summary>
        public int? Skip { get; private set; }
        /// <summary>
        /// The amount of rows to take
        /// </summary>
        public int? Take { get; private set; }
        
        /// <summary>
        /// Indicates that the SELECT should have a DISTINCT specifier
        /// </summary>
        public bool Distinct { get; private set; }
        /// <summary>
        /// The field to order by
        /// </summary>
        public string OrderBy { get; private set; } = string.Empty;
        /// <summary>
        /// The alias of the field to order by
        /// </summary>
        public string AliasedOrderBy { get; private set; } = string.Empty;
        /// <summary>
        /// The order by with the alias
        /// </summary>
        public string SkipTakeAliasedOrderBy { get; private set; } = string.Empty;
        /// <summary>
        /// The linked order by
        /// </summary>
        public string LinkedOrderBy { get; private set; } = string.Empty;

        /// <summary>
        /// The WHERE-clause
        /// </summary>
        public string WhereClause { get; private set; } = string.Empty;

        /// <summary>
        /// The list of temporary tables
        /// </summary>
        public List<TempTable<object>> TempTables { get; set; } = new List<TempTable<object>>();

        /// <summary>
        /// The list of temporary tables that will be bulk inserted
        /// </summary>
        public List<TempTable<object>> BulkCopyTempTables { get; set; } = new List<TempTable<object>>();

        private EntityTypeInfo _typeInfo;
        private string _parameterPrefix;

        /// <summary>
        /// Value indicating whether the query is translated
        /// </summary>
        public bool IsTranslated { get; set; }

        private string _currentParameterPrefix;
        private string _currentTablePart;

        /// <summary>
        /// Initializes <see cref="QueryTranslator"/>. Optionally with <paramref name="serverVersion"/> provided, so version specific translations can be used.
        /// </summary>
        /// <param name="serverVersion"></param>
        public QueryTranslator(ServerVersion serverVersion = null) {
            _serverVersion = serverVersion;
        }
        /// <summary>
        /// Translates a query
        /// </summary>
        /// <param name="info">The info about the query</param>
        /// <param name="parameterPrefix">The prefix for the parameter</param>
        /// <returns>The translated query</returns>
        public string Translate(QueryInfo info, string parameterPrefix = "") {
            var expression = BooleanComplexifier.Process(info.Expression);
            if (expression == null) {
                return null;
            }
            _tempTablesGenerated = 0;
            _currentTablePart = info.TablePart;
            var t = expression.Type.GenericTypeArguments.First();
            _typeInfo = EntityConverter.GetEntityTypeInfo(t);
            _parameterPrefix = parameterPrefix.IsNullOrWhiteSpace() ? "" : parameterPrefix + ".";
            _currentParameterPrefix = _parameterPrefix;
            _sb = new StringBuilder();
            Visit(expression);
            WhereClause = _sb.ToString();
            //BalanceBrackets();
            IsTranslated = true;
            return WhereClause;
        }

        public string ToKeyString() {
            return Skip + "+" + Take + "+" + OrderBy + "+" + AliasedOrderBy + "+" + SkipTakeAliasedOrderBy + "+" + LinkedOrderBy + "+";
        }

        private static Expression StripQuotes(Expression e) {
            while (e.NodeType == ExpressionType.Quote) {
                e = ((UnaryExpression) e).Operand;
            }
            return e;
        }

        /// <inheritdoc />
        protected override Expression VisitMethodCall(MethodCallExpression m) {
            var declaringType = m.Method.DeclaringType;
            if (declaringType == typeof(Sql)) {
                return HandleSqlFunction(m);
            }
            if (IsFreeFieldValueFunction(m)) {
                var memberInfo = GetFreeFieldValueMember(m);
                var parentExpression = memberInfo.Item2?.Expression as MemberExpression;
                AppendMemberExpression(memberInfo.Item2, parentExpression, memberInfo.Item1);
                return m;
            }
            if (declaringType == typeof(Queryable) && m.Method.Name == "Where") {
                Visit(m.Arguments[0]);
                var lambda = (LambdaExpression) StripQuotes(m.Arguments[1]);
                Visit(lambda.Body);
                return m;
            }
            if ((declaringType.IsGenericList() || declaringType == typeof(Enumerable)) &&
                declaringType != typeof(string) &&
                m.Method.Name.Equals("Contains")) {
                GenerateIn(m);
                return m;
            }
            if (declaringType == typeof(Data.Extensions.EnumerableExtensions) && m.Method.Name.Equals("Contains")) {
                var containsTypeArgs = m.Method.GetGenericArguments();
                if (containsTypeArgs.Length == 2
                    && typeof(IBulkCopyable).IsAssignableFrom(containsTypeArgs[0]) 
                    && typeof(IEntity).IsAssignableFrom(containsTypeArgs[1])) {
                    GenerateBulkCopyExists(m);
                    return m;
                }
            }
            if (declaringType == typeof(Enum) && m.Method.Name.Equals("HasFlag")) {
                HandleHasFlag(m);
                return m;
            }
            if (m.Method.Name == "Distinct") {
                Distinct = true;
                Visit(m.Arguments[0]);
                return m;
            }
            if (m.Method.Name == "Take") {
                if (ParseTakeExpression(m)) {
                    var nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            if (m.Method.Name == "Skip") {
                if (ParseSkipExpression(m)) {
                    var nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            if (m.Method.Name == "OrderBy") {
                if (ParseOrderByExpression(m, "ASC")) {
                    var nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            if (m.Method.Name == "OrderByDescending") {
                if (ParseOrderByExpression(m, "DESC")) {
                    var nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            if (m.Method.Name == "ThenBy") {
                if (ParseThenByExpression(m, "ASC")) {
                    var nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            if (m.Method.Name == "ThenByDescending") {
                if (ParseThenByExpression(m, "DESC")) {
                    var nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            if (m.Method.Name == "Any") {
                _sb.Append("("); //start any
                var mArgumentFirst = m.Arguments[0];
                var isEmptyAny = m.Arguments.Count == 1;
                var memberExpression = mArgumentFirst as MemberExpression;
                if (memberExpression == null && (mArgumentFirst.NodeType == ExpressionType.Convert || mArgumentFirst.NodeType == ExpressionType.ConvertChecked)) {
                    var unaryExpr = mArgumentFirst as UnaryExpression;
                    var operandExpr = unaryExpr?.Operand as MemberExpression;
                    if (operandExpr != null && unaryExpr.Type.IsAssignableFrom(unaryExpr.Operand.Type)) {
                        memberExpression = operandExpr;
                    }
                }
                HandleBaseEntityMemberForAny(memberExpression, isEmptyAny);
                if (!isEmptyAny) {
                    var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    Visit(lambda.Body);
                    _sb.Append(")"); // close subselect
                }
                _sb.Append(")"); // close any
                _currentParameterPrefix = _parameterPrefix;
                return m;
            }
            if (m.Method.Name == "All") {
                var mArgumentFirst = m.Arguments[0];
                var memberExpression = mArgumentFirst as MemberExpression;
                if (memberExpression == null && (mArgumentFirst.NodeType == ExpressionType.Convert || mArgumentFirst.NodeType == ExpressionType.ConvertChecked)) {
                    var unaryExpr = mArgumentFirst as UnaryExpression;
                    var operandExpr = unaryExpr?.Operand as MemberExpression;
                    if (operandExpr != null && unaryExpr.Type.IsAssignableFrom(unaryExpr.Operand.Type)) {
                        memberExpression = operandExpr;
                    }
                }
                return HandleAll(m, memberExpression);
            }
            if (ParseLikeOperationExpressions(m)) {
                return m;
            }
            if (ParseEqualsExpression(m)) {
                return m;
            }
            if (declaringType == typeof(Convert)) {
                var nextExpression = m.Arguments[0];
                _sb.Append("CAST("); //start Cast
                Visit(nextExpression);
                _sb.Append(" AS ");
                var size = 0;
                var precision = 0;
                var scale = 0;
                if (m.Method.ReturnType == typeof(string) || m.Method.ReturnType == typeof(byte[])) {
                    size = -1;
                }
                if (m.Method.ReturnType.IsDecimal()) {
                    precision = 18;
                    scale = 4;
                }
                _sb.Append(DbTypeConverter.GetStringifiedSqlType(m.Method.ReturnType, size, precision, scale));
                _sb.Append(")"); // close Cast
                return m;
            }
            if (declaringType == typeof(DateTime) && m.Method.Name.StartsWith("Add")) {
                string datePart = null;
                if (m.Method.Name.Equals("AddMinutes")) {
                    datePart = "mi";
                } else if (m.Method.Name.Equals("AddHours")) {
                    datePart = "hh";
                } else if (m.Method.Name.Equals("AddDays")) {
                    datePart = "day";
                } else if (m.Method.Name.Equals("AddMonths")) {
                    datePart = "month";
                } else if (m.Method.Name.Equals("AddYears")) {
                    datePart = "year";
                }
                if (!datePart.IsNullOrWhiteSpace()) {
                    _sb.Append("DATEADD({0}, ", datePart);
                    var nextExpression = m.Arguments[0];
                    Visit(nextExpression);
                    _sb.Append(", ");
                    var member = m.Object as MemberExpression;
                    if (member != null && member.Expression == null) {
                        var constant = GetMemberConstant(member);
                        VisitConstant(constant);
                    } else {
                        Visit(m.Object);
                    }
                    _sb.Append(")");
                    return m;
                }
            }
            if (declaringType?.FullName != null && declaringType.FullName.Equals("Microsoft.VisualBasic.CompilerServices.Operators") && m.Method.Name.Equals("CompareString")) {
                return HandleVisualBasicCompareString(m);
            }
            throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
        }

        private Tuple<string, MemberExpression> GetFreeFieldValueMember(MethodCallExpression m) {
            var fftArg = m.Arguments[0];
            var ce = fftArg as ConstantExpression;
            IFreeFieldType fft;
            if (ce != null) {
                fft = ce.Value as IFreeFieldType;
            } else {
                var ne = fftArg as NewExpression;
                if (ne != null) {
                    fft = (IFreeFieldType) ne.Constructor.Invoke(ne.Arguments.Select(a => ((ConstantExpression) a).Value).ToArray());
                } else {
                    fft = (IFreeFieldType) GetMemberConstant(fftArg as MemberExpression)?.Value;
                }
            }
            FreeFieldColumnType ffColumnType;
            bool useDescription;
            if (fft != null) {
                ffColumnType = fft.ColumnType;
                useDescription = fft.UseDescription;
            } else {
                throw new ArgumentException("Expression is not supported");
            }
            int index;
            ce = m.Arguments[1] as ConstantExpression;
            if (ce != null) {
                index = ce.Value.To<int>();
            } else {
                index = GetMemberConstant(m.Arguments[1] as MemberExpression)?.Value.To<int>() ?? 0;
            }

            string fieldName;
            if (useDescription) {
                var prefix = ffColumnType.GetDescription();
                fieldName = string.Format(prefix, index);
            } else {
                var prefix = ffColumnType.ToString();
                fieldName = prefix + index;
            }
            var parentMember = m?.Object as MemberExpression;
            if (parentMember?.Type != null) {
                var parentTypeInfo = EntityConverter.GetEntityTypeInfo(parentMember?.Type);
                var freeFieldPropInfo = parentTypeInfo.GetPropertyInfo(fieldName, _currentTablePart)?.PropertyInfo;
                if (freeFieldPropInfo != null) {
                    return Tuple.Create(fieldName, Expression.MakeMemberAccess(parentMember, freeFieldPropInfo));
                }
            }
            return Tuple.Create(fieldName, (MemberExpression)null);
        }

        private bool IsFreeFieldValueFunction(Expression e) {
            var m = e as MethodCallExpression;
            if (m == null) { return false; }
            return m.Method.DeclaringType.IsInterfaceImplementationOf<IFreeField>() && m.Method.Name.StartsWith("GetFreeFieldValue");
        }

        private Expression HandleSqlFunction(MethodCallExpression m) {
            return HandleSqlFunctionInternal(m, _sb, this);
        }

        internal Expression HandleSqlFunctionInternal<T>(MethodCallExpression m, StringBuilder sb, T visitor) where T : ExpressionVisitor {
            var methodName = m.Method.Name.ToUpper();
            switch (methodName) {
                case "CAST":
                    var valueToCast = m.Arguments[0];
                    var sqlDbTypeExpression = (m.Arguments[1] as ConstantExpression);
                    if (sqlDbTypeExpression == null) {
                        visitor.Visit(valueToCast);
                        return m;
                    }
                    sb.AppendFormat("{0}(", methodName);
                    visitor.Visit(valueToCast);
                    var sqlDbType = sqlDbTypeExpression.Value.To<SqlDbType>();
                    var size = 0;
                    var precision = 0;
                    var scale = 0;
                    if (m.Arguments.Count == 3) {
                        var sizeExpression = m.Arguments[2] as ConstantExpression;
                        size = sizeExpression?.Value.To<int>() ?? 0;
                    }else if (m.Arguments.Count == 4) {
                        var precisionExpression = m.Arguments[2] as ConstantExpression;
                        precision = precisionExpression?.Value.To<int>() ?? 0;
                        var scaleExpression = m.Arguments[3] as ConstantExpression;
                        scale = scaleExpression?.Value.To<int>() ?? 0;
                    }
                    var dbTypeDefinition = DbTypeConverter.GetStringifiedSqlType(sqlDbType, size, precision, scale);
                    sb.Append(" AS {0})", dbTypeDefinition);
                    return m;
                case "ISNULL":
                case "NULLIF":
                    var original = m.Arguments[0];
                    var replacement = m.Arguments[1];
                    sb.AppendFormat("{0}(", methodName);
                    visitor.Visit(original);
                    sb.Append(", ");
                    visitor.Visit(replacement);
                    sb.Append(")");
                    return m;
                case "DATEPART":
                case "DATEDIFF":
                case "DATEDIFFBIG":
                    var datePart = m.Arguments[0];
                    var datePartConstantExpression = datePart as ConstantExpression;
                    if (datePartConstantExpression?.Value == null || datePartConstantExpression.Value.GetType() != typeof(DatePart)) {
                        throw new NotSupportedException("Invalid DatePart argument");
                    }
                    var datePartValue = datePartConstantExpression.Value.To<DatePart>();
                    var datePartAbbreviation = datePartValue.GetAbbreviation();
                    if (datePartAbbreviation.IsNullOrWhiteSpace()) {
                        throw new NotSupportedException("Invalid DatePart argument");
                    }
                    var date = m.Arguments[1];
                    var correctedMethodName = methodName == "DATEDIFFBIG" ? "DATEDIFF_BIG" : methodName;
                    sb.AppendFormat("{0}({1}, ", correctedMethodName, datePartAbbreviation);
                    visitor.Visit(date);
                    if (methodName.StartsWith("DATEDIFF")) {
                        sb.Append(", ");
                        var endDate = m.Arguments[2];
                        visitor.Visit(endDate);
                    }
                    sb.Append(")");
                    return m;
                case "TRIM":
                    var trimCallValue = m.Arguments[0];
                    var trimSupported = _serverVersion != null && _serverVersion.SupportsTrim;
                    if (!trimSupported) {
                        methodName = "LTRIM(RTRIM";
                    }
                    sb.AppendFormat("{0}(", methodName);
                    visitor.Visit(trimCallValue);
                    sb.Append(")");
                    if (!trimSupported) {
                        sb.Append(")");
                    }
                    return m;
                case "LTRIM":
                case "RTRIM":
                case "LOWER":
                case "UPPER":
                case "LEN":
                case "YEAR":
                case "MONTH":
                case "DAY":
                    var callValue = m.Arguments[0];
                    sb.AppendFormat("{0}(", methodName);
                    visitor.Visit(callValue);
                    sb.Append(")");
                    return m;
                case "LEFT":
                case "RIGHT":
                    var value = m.Arguments[0];
                    var padLength = m.Arguments[1];
                    sb.AppendFormat("{0}(", methodName);
                    visitor.Visit(value);
                    sb.Append(", ");
                    visitor.Visit(padLength);
                    sb.Append(")");
                    return m;
                case "PADLEFT":
                    var valueToLeftPad = m.Arguments[0];
                    var leftPadLength = m.Arguments[1];
                    var leftPadChar = m.Arguments[2];
                    sb.Append("RIGHT(REPLICATE(");
                    visitor.Visit(leftPadChar);
                    sb.Append(", ");
                    visitor.Visit(leftPadLength);
                    sb.Append(") + ");
                    visitor.Visit(valueToLeftPad);
                    sb.Append(", ");
                    visitor.Visit(leftPadLength);
                    sb.Append(")");
                    return m;
                case "PADRIGHT":
                    var valueToRightPad = m.Arguments[0];
                    var rightPadLength = m.Arguments[1];
                    var rightPadChar = m.Arguments[2];
                    sb.Append("LEFT(");
                    visitor.Visit(valueToRightPad);
                    sb.Append(" + REPLICATE(");
                    visitor.Visit(rightPadChar);
                    sb.Append(", ");
                    visitor.Visit(rightPadLength);
                    sb.Append("), ");
                    visitor.Visit(rightPadLength);
                    sb.Append(")");
                    return m;
                default:
                    throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
            }
        }

        private Expression HandleVisualBasicCompareString(MethodCallExpression m) {
            void Action(Expression expr) {
                if (!(expr is MemberExpression memberExpr)) {
                    if (expr is MethodCallExpression methodCallExpr) { VisitMethodCall(methodCallExpr); } else {
                        var likeMember = ((ConstantExpression)expr).Value;
                        var paramName = AddToParameters(typeof(string), likeMember);
                        _sb.Append($"@{paramName}");
                    }
                } else {
                    var name = GetColumnNameFromProperty(memberExpr);
                    if (name == null) {
                        var likeMember = GetMemberConstant(memberExpr).Value;
                        var paramName = AddToParameters(typeof(string), likeMember);
                        _sb.Append($"@{paramName}");
                    } else {
                        if (memberExpr.Expression is MemberExpression parentExpression
                            && (_typeInfo.LinkedEntityProperties.Any(l => l.BaseEntityType == parentExpression.Type)
                                || _typeInfo.InverseLinkedEntityProperties.Any(l => l.BaseEntityType == parentExpression.Type))) {
                            AppendLinkedEntityMember(memberExpr, parentExpression);
                        } else {
                            _sb.Append($"{_currentParameterPrefix}[{name}]");
                        }
                    }
                }
            }

            var leftArg = m.Arguments[0];
            var rightArg = m.Arguments[1];
            _sb.Append("(");
            _sb.AppendFormat("CASE WHEN ");
            Action(leftArg);
            _sb.Append(" = ");
            Action(rightArg);
            _sb.Append(" THEN 0 WHEN ");
            Action(leftArg);
            _sb.Append(" < ");
            Action(rightArg);
            _sb.Append(" THEN -1 WHEN ");
            Action(leftArg);
            _sb.Append(" > ");
            Action(rightArg);
            _sb.Append(" THEN 1 ELSE NULL END");
            _sb.Append(")");
            return m;
        }

        private void GenerateBulkCopyExists(MethodCallExpression m) {
            var member = m.Object ?? m.Arguments[0];
            var memberTypeArgs = m.Method.GetGenericArguments();
            var memberExpression = member as MemberExpression;
            var constantMember = member as ConstantExpression ?? GetMemberConstant(memberExpression);
            var container = constantMember.Value;
            var items = (IEnumerable)container;
            var list = items?.Cast<object>().ToSafeList();
            if (list.IsEmpty()) {
                // replaced EmptyCollectionException with an impossible comparison (1 <> 1). SQL Server optimises this to a constant scan and ignores all other where parts
                _sb.Append(" (1 <> 1) ");
                return;
            }
            var containerBaseType = memberTypeArgs[0];
            var entityType = memberTypeArgs[1];
            var typeInfo = EntityConverter.GetEntityTypeInfo(entityType);

            var tempTableName = TempTableHelper.GetTempTableName(ref _tempTablesGenerated);
            var tempTable = new TempTable<object>(containerBaseType) {
                Values = list,
                TableName = tempTableName,
            };
            BulkCopyTempTables.Add(tempTable);
            var alias = _currentParameterPrefix?.NullIf("") ?? $"{BaseQueryBuilder.BaseLevelAlias}.";
            var whereParts = new List<string>();
            foreach (var bulkCopyableProperty in tempTable.GetIndexedBulkCopyableProperties()) {
                var entityProp = typeInfo.GetPropertyInfo(bulkCopyableProperty.Name, null);
                if (entityProp == null) {
                    throw new NotSupportedException();
                }
                var colName = "[" + entityProp.ColumnName.FormatWith(_currentTablePart) + "]";
                whereParts.Add("{0}{1} = t.[{2}]".FormatWith(alias, colName, bulkCopyableProperty.Name));
            }
            if (whereParts.IsEmpty()) {
                // replaced EmptyCollectionException with an impossible comparison (1 <> 1). SQL Server optimises this to a constant scan and ignores all other where parts
                _sb.Append(" (1 <> 1) ");
                return;
            }
            _sb.AppendFormat("(EXISTS(SELECT 1 FROM {0} t WHERE {1})", tempTableName, string.Join(" AND ", whereParts));
        }

        private void HandleHasFlag(MethodCallExpression m) {
            _sb.Append("(");
            _sb.Append("(");
            Visit(m.Object);
            _sb.Append(" & ");
            Visit(m.Arguments[m.Arguments.Count > 1 ? 1 : 0]);
            _sb.Append(")");
            _sb.Append(" = ");
            Visit(m.Arguments[m.Arguments.Count > 1 ? 1 : 0]);
            _sb.Append(")");
        }

        private void GenerateIn(MethodCallExpression m) {
            var member = m.Object ?? m.Arguments[0];
            var isDistinct = false;
            var memberExpression = member as MemberExpression;
            if (memberExpression == null) {
                if (member.NodeType == ExpressionType.Call) {
                    var methodCallExpr = member as MethodCallExpression;
                    if (methodCallExpr != null) {
                        if (methodCallExpr.Method.Name.Contains("Distinct")) {
                            isDistinct = true;
                            member = methodCallExpr.Object ?? methodCallExpr.Arguments[0];
                        } else {
                            throw new NotSupportedException($"The method '{methodCallExpr.Method.Name}' is not supported");
                        }
                    }
                }
                if (member.NodeType == ExpressionType.Convert || member.NodeType == ExpressionType.ConvertChecked) {
                    var unaryExpr = member as UnaryExpression;
                    var operandExpr = unaryExpr?.Operand as MemberExpression;
                    if (operandExpr != null && unaryExpr.Type.IsAssignableFrom(unaryExpr.Operand.Type)) {
                        memberExpression = operandExpr;
                    }
                }
            }
            var constantMember = member as ConstantExpression ?? GetMemberConstant(memberExpression);
            var container = constantMember.Value;
            var items = (IEnumerable) container;
            var list = (isDistinct ? items?.Cast<object>().Distinct() : items?.Cast<object>()).ToSafeList();
            if (list.IsEmpty()) {
                // replaced EmptyCollectionException with an impossible comparison (1 <> 1). SQL Server optimises this to a constant scan and ignores all other where parts
                _sb.Append(" (1 <> 1) ");
                return;
            }
            _sb.Append("(");
            Visit(m.Arguments[m.Arguments.Count > 1 ? 1 : 0]);
            _sb.Append(" IN ");
            var containerType = container.GetType();
            Type containerBaseType;
            if (containerType.BaseType == typeof(Array)) {
                containerBaseType = containerType.GetElementType();
            } else {
                containerBaseType = containerType.GetGenericArguments().Last();
            }

            var remainingParameters = 1000 - Parameters.Count;
            if (list.Count > remainingParameters) {
                var tempTableName = TempTableHelper.GetTempTableName(ref _tempTablesGenerated);
                var tempTable = new TempTable<object>(containerBaseType) {
                    Values = list,
                    TableName = tempTableName
                };
                if (list.Count >= 3000) {
                    BulkCopyTempTables.Add(tempTable);
                } else {
                    TempTables.Add(tempTable);
                }
                _sb.AppendFormat("(SELECT tempValue FROM {0})", tempTableName);
            } else {
                var paramNames = list.Select(item => $"@{AddToParameters(containerBaseType, item)}").ToList();
                _sb.AppendFormat("({0})", string.Join(", ", paramNames));
            }
            _sb.Append(")");
        }

        private bool ParseEqualsExpression(MethodCallExpression m) {
            if (m.Object == null || ((m.Object.Type != typeof(string) && m.Object.Type != typeof(char)) && !m.Object.Type.IsNumeric())) {
                return false;
            }
            object equalsMember = "";
            var argumentMemberExpression = m.Arguments[0] as MemberExpression;
            if (argumentMemberExpression != null) {
                equalsMember = GetMemberConstant(argumentMemberExpression).Value;
            } else {
                var argumentConstantExpression = m.Arguments[0] as ConstantExpression;
                if (argumentConstantExpression != null) {
                    equalsMember = argumentConstantExpression.Value;
                } else {
                    var unaryConstantExpression = m.Arguments[0] as UnaryExpression;
                    var ctExpression = unaryConstantExpression?.Operand as ConstantExpression;
                    if (ctExpression != null) {
                        equalsMember = ctExpression.Value;
                    } else {
                        throw new NotSupportedException($"Argument '{m.Arguments[0].NodeType}' is not supported.");
                    }
                }
            }

            var memberExpression = m.Object as MemberExpression;
            string name = null;
            var methodCallExpression = m.Object as MethodCallExpression;
            if (methodCallExpression != null) {
                if (IsFreeFieldValueFunction(methodCallExpression)) {
                    var memberInfo = GetFreeFieldValueMember(methodCallExpression);
                    name = memberInfo.Item1;
                    if (memberInfo.Item2 != null) { memberExpression = memberInfo.Item2; }
                }
            }
            if (name == null && memberExpression != null) {
                name = GetColumnNameFromProperty(memberExpression) ?? GetMemberConstant(memberExpression).Value.ToString();
            }
            if (name == null) {
                var ctExpression = m.Object as ConstantExpression;
                name = ctExpression?.Value.ToString() ?? string.Empty;
            }
            var parentExpression = (memberExpression?.Expression ?? methodCallExpression?.Object) as MemberExpression;

            _sb.Append("(");
            var propertyInfo = AppendMemberExpression(memberExpression, parentExpression, name);
            var paramName = AddToParameters(propertyInfo.PropertyType, equalsMember);
            _sb.AppendFormat(" = @{0})", paramName);
            return true;
        }

        private EntityPropertyInfo AppendMemberExpression(MemberExpression memberExpression, MemberExpression parentExpression, string name) {
            EntityTypeInfo typeInfoToUse;
            if (parentExpression == null) {
                typeInfoToUse = _typeInfo;
            } else {
                typeInfoToUse = _typeInfo.EntityType == parentExpression.Type ? _typeInfo : EntityConverter.GetEntityTypeInfo(parentExpression.Type);
            }

            var propertyInfo = typeInfoToUse.GetPropertyInfo(name, null);
            if (propertyInfo == null) {
                throw new NotSupportedException($"Property '{name}' does not exist on {typeInfoToUse.EntityType.Name}");
            }
            if (parentExpression != null && _typeInfo.LinkedEntityProperties.Any(l => l.BaseEntityType == parentExpression.Type)) {
                AppendLinkedEntityMember(memberExpression, parentExpression);
            } else {
                _sb.AppendFormat("{0}[{1}]", _currentParameterPrefix, name);
            }
            return propertyInfo;
        }

        private bool ParseLikeOperationExpressions(MethodCallExpression m) {
            if (m.Object == null || m.Object.Type != typeof(string)) {
                return false;
            }
            var isContains = m.Method.Name.Equals("Contains");
            var percentAtStart = m.Method.Name.Equals("EndsWith") || isContains;
            var percentAtEnd = m.Method.Name.Equals("StartsWith") || isContains;
            var leftLikePart = percentAtStart ? "%" : "";
            var rightLikePart = percentAtEnd ? "%" : "";
            if (!percentAtStart && !percentAtEnd) {
                return false;
            }

            void Action(Expression expr, bool isLeft) {
                if (expr is MethodCallExpression methodCallExpression) {
                    if (IsFreeFieldValueFunction(methodCallExpression)) {
                        var memberInfo = GetFreeFieldValueMember(methodCallExpression);
                        var parentExpression = memberInfo.Item2?.Expression as MemberExpression;
                        AppendMemberExpression(memberInfo.Item2, parentExpression, memberInfo.Item1);
                        return;
                    }
                }
                if (!(expr is MemberExpression memberExpr)) {
                    var likeMember = ((ConstantExpression)expr).Value;
                    var paramName = AddToParameters(typeof(string), $"{(isLeft ? "" : leftLikePart)}{likeMember}{(isLeft ? "" : rightLikePart)}");
                    _sb.AppendFormat("@{0}", paramName);
                } else {
                    var name = GetColumnNameFromProperty(memberExpr);
                    if (name == null) {
                        var likeMember = GetMemberConstant(memberExpr).Value;
                        var paramName = AddToParameters(typeof(string), $"{(isLeft ? "" : leftLikePart)}{likeMember}{(isLeft ? "" : rightLikePart)}");
                        _sb.AppendFormat("@{0}", paramName);
                    } else {
                        if (!isLeft && !leftLikePart.IsNullOrEmpty()) {
                            _sb.AppendFormat("'{0}' + ", leftLikePart);
                        }
                        if (memberExpr.Expression is MemberExpression parentExpression
                            && (_typeInfo.LinkedEntityProperties.Any(l => l.BaseEntityType == parentExpression.Type)
                                || _typeInfo.InverseLinkedEntityProperties.Any(l => l.BaseEntityType == parentExpression.Type))) {
                            AppendLinkedEntityMember(memberExpr, parentExpression);
                        } else { _sb.AppendFormat("{0}[{1}]", _currentParameterPrefix, name); }
                        if (!isLeft && !rightLikePart.IsNullOrEmpty()) {
                            _sb.AppendFormat(" + '{0}'", rightLikePart);
                        }
                    }
                }
            }

            _sb.Append("(");
            Action(m.Object, true);
            _sb.Append(" LIKE ");
            Action(m.Arguments[0], false);
            _sb.Append(")");
            return true;
        }

        /// <inheritdoc />
        protected override Expression VisitUnary(UnaryExpression u) {
            switch (u.NodeType) {
                case ExpressionType.Not:
                    _sb.Append(" NOT ");
                    Visit(u.Operand);
                    break;
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException($"The unary operator '{u.NodeType}' is not supported");
            }
            return u;
        }

        private void AddDateExpression(MemberExpression member, bool isDate) {
            _sb.Append("CAST(");
            VisitMember(member);
            _sb.Append(isDate ? " AS DATE)" : " AS DATETIME)");
        }

        private static bool IsDateExpression(Expression expression) {
            var ce = expression as ConstantExpression;
            DateTime dt;
            if (ce != null) {
                dt = (DateTime) ce.Value;
            } else {
                dt =
                    (DateTime)
                    ((NewExpression) expression).Constructor.Invoke(
                        ((NewExpression) expression).Arguments.Select(a => ((ConstantExpression) a).Value).ToArray());
            }
            return dt == dt.Date;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression b) {
            _sb.Append("(");
            var leftExpressionHasDate = (b.Left.NodeType == ExpressionType.Constant ||
                                         b.Left.NodeType == ExpressionType.New) &&
                                        b.Left.Type == typeof(DateTime);
            var rightExpressionHasDate = (b.Right.NodeType == ExpressionType.Constant ||
                                          b.Right.NodeType == ExpressionType.New) &&
                                         b.Right.Type == typeof(DateTime);
            var isLeftCharExpression = false;
            var isRightCharExpression = false;
            var leftUnary = b.Left as UnaryExpression;
            if (leftUnary != null) {
                if (leftUnary.NodeType == ExpressionType.Convert || leftUnary.NodeType == ExpressionType.ConvertChecked) {
                    var operandType = leftUnary.Operand.Type;
                    isLeftCharExpression = operandType == typeof(char);
                }
            } else {
                isLeftCharExpression |= b.Left.Type == typeof(char);
            }
            var rightUnary = b.Right as UnaryExpression;
            if (rightUnary != null) {
                if (rightUnary.NodeType == ExpressionType.Convert || rightUnary.NodeType == ExpressionType.ConvertChecked) {
                    var operandType = rightUnary.Operand.Type;
                    isRightCharExpression = operandType == typeof(char);
                }
            } else {
                isRightCharExpression = b.Right.Type == typeof(char);
            }
            var isBinaryCharExpression = isLeftCharExpression || isRightCharExpression;
            var isDate = false;
            if (leftExpressionHasDate) {
                isDate = IsDateExpression(b.Left);
            } else if (rightExpressionHasDate) {
                isDate = IsDateExpression(b.Right);
            }
            if (rightExpressionHasDate) {
                AddDateExpression((MemberExpression) b.Left, isDate);
            } else {
                if (!isBinaryCharExpression) {
                    Visit(b.Left);
                } else {
                    if (leftUnary != null) {
                        Visit(leftUnary.Operand);
                    } else {
                        var cExpression = b.Left as ConstantExpression;
                        if (cExpression != null) {
                            var val = Convert.ToChar(cExpression.Value);
                            cExpression = Expression.Constant(val);
                            Visit(cExpression);
                        } else {
                            Visit(b.Left);
                        }
                    }
                }
            }
            switch (b.NodeType) {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    _sb.Append(" AND ");
                    break;

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    _sb.Append(" OR ");
                    break;

                case ExpressionType.Equal:
                    if (IsNullConstant(b.Right)) {
                        _sb.Append(" IS ");
                    } else {
                        _sb.Append(" = ");
                    }
                    break;

                case ExpressionType.NotEqual:
                    if (IsNullConstant(b.Right)) {
                        _sb.Append(" IS NOT ");
                    } else {
                        _sb.Append(" <> ");
                    }
                    break;

                case ExpressionType.LessThan:
                    _sb.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    _sb.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    _sb.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _sb.Append(" >= ");
                    break;
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    _sb.Append(" + ");
                    break;
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    _sb.Append(" - ");
                    break;
                case ExpressionType.Divide:
                    _sb.Append(" / ");
                    break;
                case ExpressionType.Multiply:
                    _sb.Append(" * ");
                    break;
                case ExpressionType.Modulo:
                    _sb.Append(" % ");
                    break;
                default:
                    throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported");
            }

            if (leftExpressionHasDate) {
                AddDateExpression((MemberExpression) b.Right, isDate);
            } else {
                if (!isBinaryCharExpression) {
                    Visit(b.Right);
                } else {
                    if (rightUnary != null) {
                        Visit(rightUnary.Operand);
                    } else {
                        var cExpression = b.Right as ConstantExpression;
                        if (cExpression != null) {
                            var val = Convert.ToChar(cExpression.Value);
                            cExpression = Expression.Constant(val);
                            Visit(cExpression);
                        } else {
                            Visit(b.Right);
                        }
                    }
                }
            }
            _sb.Append(")");
            return b;
        }

        /// <inheritdoc />
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

        private void AddConstantExpressionValue(object value, Type type) {
            string paramName;
            if (type.IsEnum) {
                var converter = Converter.GetDelegate(type.GetEnumUnderlyingType());
                //dynamic dynValue = converter(c.Value);
                //_sb.Append(dynValue.ToString(CultureInfo.InvariantCulture));
                paramName = AddToParameters(type.GetEnumUnderlyingType(), converter(value));
                _sb.AppendFormat("@{0}", paramName);
            } else {
                var nullableType = Nullable.GetUnderlyingType(type);
                if (nullableType != null) {
                    var converter = Converter.GetDelegate(nullableType);
                    //dynamic dynValue = converter(c.Value);
                    //_sb.Append(dynValue.ToString(CultureInfo.InvariantCulture));
                    paramName = AddToParameters(nullableType, converter(value));
                    _sb.AppendFormat("@{0}", paramName);
                } else {
                    //dynamic value = c.Value;
                    //_sb.Append(value.ToString(CultureInfo.InvariantCulture));
                    //var paramName = GetNextParam();
                    paramName = AddToParameters(type, value);
                    _sb.AppendFormat("@{0}", paramName);
                }
            }
        }

        private void AppendValue(TypeCode typeCode, object value, Type type = null) {
            string paramName;
            switch (typeCode) {
                case TypeCode.Boolean:
                    paramName = AddToParameters(typeof(bool), value);
                    _sb.AppendFormat("@{0}", paramName);
                    break;

                case TypeCode.String:
                    paramName = AddToParameters(typeof(string), value);
                    _sb.AppendFormat("@{0}", paramName);
                    break;
                case TypeCode.DateTime:
                    var dt = (DateTime) value;
                    var isDate = dt == dt.Date;
                    if (isDate) {
                        dt = dt.Date;
                    }
                    paramName = AddToParameters(typeof(DateTime), dt);
                    if (isDate) {
                        _sb.AppendFormat("CAST(@{0} AS DATE)", paramName);
                    } else {
                        _sb.AppendFormat("@{0}", paramName);
                    }
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
                        paramName = AddToParameters(type ?? value.GetType(), value);
                        _sb.AppendFormat("@{0}", paramName);
                    }
                    break;
            }
        }

        /// <inheritdoc />
        protected override Expression VisitNew(NewExpression node) {
            var c = node.Constructor.Invoke(node.Arguments.Select(a => ((ConstantExpression) a).Value).ToArray());
            AppendValue(Type.GetTypeCode(node.Type), c);
            return node;
        }

        protected override Expression VisitInvocation(InvocationExpression node) {
            if (node.Expression.NodeType == ExpressionType.MemberAccess && node.Expression is MemberExpression) {
                var methodConstant = GetMemberConstant((MemberExpression) node.Expression);
                var del = methodConstant?.Value as MulticastDelegate;
                if (del != null) {
                    var method = del.Method;
                    var expr = Expression.Call(Expression.Constant(del.Target), method, node.Arguments);
                    var methodParams = method.GetParameters();

                    var value = methodParams.FirstOrDefault();
                }
                var methodArguments = node.Arguments;
            }
            return base.VisitInvocation(node);
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
                if (_typeInfo.PrimaryKeyInfo != null && m.Member.Equals(_typeInfo.PrimaryKeyInfo.PropertyInfo)) {
                    _sb.Append(_currentParameterPrefix)
                       .Append("[")
                       .Append(_typeInfo.PrimaryKeyInfo.ColumnName.FormatWith(_currentTablePart))
                       .Append("]");
                    return m;
                }
                AppendMember(m);
                return m;
            }
            if (m.Expression.NodeType == ExpressionType.MemberAccess) {
                var mExpression = m.Expression as MemberExpression;
                if (!IsExpressionFromInstance(mExpression)) {
                    if (m.Expression.Type.IsSubclassOf(typeof(BaseChild))) {
                        AppendMember(m);
                        return m;
                    }
                    if (mExpression != null) {
                        if (_typeInfo.LinkedEntityProperties.Any(l => l.BaseEntityType == mExpression.Type)) {
                            AppendLinkedEntityMember(m, mExpression);
                            return m;
                        }
                        if (_typeInfo.LinkedBaseResultProperties.Any(l => l.BaseEntityType == mExpression.Type)) {
                            AppendLinkedEntityMember(m, mExpression);
                            return m;
                        }
                    }
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
            var previousPrefix = _currentParameterPrefix;
            _currentParameterPrefix = $"{BaseQueryBuilder.LevelAlias}{linkedEntityTreeNode.Level}_{linkedEntityTreeNode.Value.Index}.";
            AppendMember(m);
            _currentParameterPrefix = previousPrefix;
        }

        private void AppendMember(MemberExpression m) {
            var typeInfo = EntityConverter.GetEntityTypeInfo(m.Expression.Type);
            var propInfo = typeInfo.GetPropertyInfo(m.Member.Name, null);
            if (propInfo != null) {
                _sb.Append(_currentParameterPrefix).Append("[").Append(propInfo.ColumnName.FormatWith(_currentTablePart)).Append("]");
                return;
            }
            _sb.Append(_currentParameterPrefix).Append("[").Append(m.Member.Name).Append("]");
        }

        private Expression HandleAll(MethodCallExpression m, MemberExpression memberExpression) {
            var entityInfo = EntityConverter.GetEntityTypeInfo(memberExpression.Expression.Type);
            var linkedEntitiesTree = EntityConverter.GetLinkedEntitiesTree(entityInfo.EntityType, false);
            var property = entityInfo.GetPropertyInfo(memberExpression.Member.Name, null);

            var node = linkedEntitiesTree.FirstOrDefault(e => e.Value.EntityPropertyInfo.Equals(property));
            if (node == null) { return m; }
            var tableName = node.Value.EntityTypeInfo.TableDefinition.FullTableName;
            if (node.Value.EntityTypeInfo.IsTableConsuming) {
                tableName = string.Format(tableName, _currentTablePart);
            }
            var previousPrefix = _currentParameterPrefix;
            _currentParameterPrefix = $"{BaseQueryBuilder.LevelAlias}{node.Level}_{node.Value.Index}.";
            
            _sb.Append("(");
            _sb.AppendFormat("{0}[{1}] IN ", _parameterPrefix, entityInfo.PrimaryKeyInfo.ColumnName.FormatWith(_currentTablePart));
            _sb.AppendFormat("(SELECT [all].[{0}] ", property.ForeignKeyColumn);
            _sb.Append("FROM (");
            _sb.AppendFormat("SELECT {0}{1}", _currentParameterPrefix, property.ForeignKeyColumn);
            _sb.Append(", SUM(CASE WHEN ");
            var lambda = (LambdaExpression) StripQuotes(m.Arguments[1]);
            Visit(lambda.Body);
            _sb.Append(" THEN 1 ELSE 0 END) AS cntPredicate");
            _sb.AppendFormat(", COUNT({0}{1}) AS cntTotal ", _currentParameterPrefix, property.ForeignKeyColumn);
            _sb.AppendFormat("FROM {0} {1} ", tableName, _currentParameterPrefix.Replace(".", ""));
            _sb.AppendFormat("GROUP BY {0}{1}", _currentParameterPrefix, property.ForeignKeyColumn);
            _sb.AppendFormat(") [all] WHERE [all].cntPredicate = [all].cntTotal)");
            _sb.Append(")");

            _currentParameterPrefix = previousPrefix;
            return m;
        }


        private void HandleBaseEntityMemberForAny(MemberExpression m, bool isEmptyAny) {
            var entityInfo = EntityConverter.GetEntityTypeInfo(m.Expression.Type);
            if (entityInfo.EntityType == _typeInfo.EntityType && m.Member.Equals(_typeInfo.PrimaryKeyInfo.PropertyInfo)) {
                _sb.Append(_currentParameterPrefix).Append("[").Append(_typeInfo.PrimaryKeyInfo.ColumnName.FormatWith(_currentTablePart)).Append("]");
                return;
            }

            var linkedEntitiesTree = EntityConverter.GetLinkedEntitiesTree(entityInfo.EntityType, false);
            foreach (var linkedEntitiesProperty in entityInfo.LinkedEntitiesProperties) {
                if (!m.Member.Name.Equals(linkedEntitiesProperty.PropertyName)) {
                    continue;
                }

                var linkedEntityTreeNode = linkedEntitiesTree.FirstOrDefault(e => e.Value.EntityTypeInfo.EntityType == linkedEntitiesProperty.BaseEntityType);
                if (linkedEntityTreeNode == null) {
                    continue;
                }

                _currentParameterPrefix = $"{BaseQueryBuilder.LevelAlias}{linkedEntityTreeNode.Level}_{linkedEntityTreeNode.Value.Index}.";
                var tableName = linkedEntityTreeNode.Value.EntityTypeInfo.TableDefinition.FullTableName;
                if (linkedEntityTreeNode.Value.EntityTypeInfo.IsTableConsuming) {
                    tableName = string.Format(tableName, _currentTablePart);
                }

                if (isEmptyAny) {
                    _sb.AppendFormat("EXISTS(SELECT 1 FROM {0} {1} WHERE {2}{3} = {4}[{5}]) ", tableName, _currentParameterPrefix.Replace(".", ""),
                                     _currentParameterPrefix, linkedEntitiesProperty.ForeignKeyColumn,
                                     _parameterPrefix, entityInfo.PrimaryKeyInfo.ColumnName.FormatWith(_currentTablePart));
                } else {
                    _sb.AppendFormat("{0}[{1}] IN ", _parameterPrefix, entityInfo.PrimaryKeyInfo.ColumnName.FormatWith(_currentTablePart));
                    _sb.AppendFormat("(SELECT {0}{1} FROM {2} {3} WHERE ", _currentParameterPrefix,
                                     linkedEntitiesProperty.ForeignKeyColumn, tableName,
                                     _currentParameterPrefix.Replace(".", ""));
                }
                return;
            }
            foreach (var linkedEntityProperty in entityInfo.LinkedEntityProperties) {
                if (!m.Member.Name.Equals(linkedEntityProperty.PropertyName)) {
                    continue;
                }

                var linkedEntityTreeNode = linkedEntitiesTree.FirstOrDefault(e => e.Value.EntityTypeInfo.EntityType == linkedEntityProperty.BaseEntityType);
                if (linkedEntityTreeNode == null) {
                    continue;
                }
                _currentParameterPrefix = $"{BaseQueryBuilder.LevelAlias}{linkedEntityTreeNode.Level}_{linkedEntityTreeNode.Value.Index}.";
                var fullTableName = linkedEntityTreeNode.Value.EntityTypeInfo.TableDefinition.FullTableName;
                if (linkedEntityTreeNode.Value.EntityTypeInfo.IsTableConsuming) {
                    fullTableName = string.Format(fullTableName, _currentTablePart);
                }

                if (isEmptyAny) {
                    _sb.AppendFormat("EXISTS(SELECT 1 FROM {0} {1} WHERE {2}{3} = {4}[{5}]) ", fullTableName, _currentParameterPrefix.Replace(".", ""),
                                     _currentParameterPrefix, linkedEntityProperty.ForeignKeyColumn,
                                     _parameterPrefix, entityInfo.PrimaryKeyInfo.ColumnName.FormatWith(_currentTablePart));
                } else {
                    _sb.AppendFormat("{0}[{1}] IN ", _parameterPrefix, entityInfo.PrimaryKeyInfo.ColumnName.FormatWith(_currentTablePart));
                    _sb.AppendFormat("(SELECT {0}{1} FROM {2} {3} WHERE ", _currentParameterPrefix,
                                     linkedEntityProperty.ForeignKeyColumn, fullTableName, _currentParameterPrefix.Replace(".", ""));
                }
                return;
            }
        }

        private static bool IsExpressionFromInstance(MemberExpression memberExpression) {
            if (memberExpression == null) { return false; }
            bool withSuccess;
            GetMemberConstant(memberExpression, false, out withSuccess);
            return withSuccess;
        }

        private static ConstantExpression GetMemberConstant(MemberExpression memberExpression) {
            bool withSuccess;
            return GetMemberConstant(memberExpression, true, out withSuccess);
        }

        private static ConstantExpression GetMemberConstant(MemberExpression node, bool canThrow, out bool withSuccess) {
            object value;

            if (node.Member.MemberType == MemberTypes.Field) {
                value = GetFieldValue(node, canThrow, out withSuccess);
                if (!withSuccess) { return null; }
            } else if (node.Member.MemberType == MemberTypes.Property) {
                value = GetPropertyValue(node, canThrow, out withSuccess);
                if (!withSuccess) { return null; }
            } else {
                if (canThrow) {
                    throw new NotSupportedException();
                }
                withSuccess = false;
                return null;
            }
            withSuccess = true;
            return Expression.Constant(value, node.Type);
        }

        private static object GetFieldValue(MemberExpression node, bool canThrow, out bool withSuccess) {
            var fieldInfo = (FieldInfo) node.Member;
            object instance;
            if (node.Expression == null) {
                instance = null;
            } else {
                var cEx = TryEvaluate(node.Expression, canThrow, out withSuccess);
                if (!withSuccess) { return null; }
                instance = cEx.Value;
            }
            withSuccess = true;
            return fieldInfo.GetValue(instance);
        }

        private static object GetPropertyValue(MemberExpression node, bool canThrow, out bool withSuccess) {
            var propertyInfo = (PropertyInfo) node.Member;
            object instance;
            if (node.Expression == null) {
                instance = null;
            } else {
                var cEx = TryEvaluate(node.Expression, canThrow, out withSuccess);
                if (!withSuccess) { return null; }
                instance = cEx.Value;
            }
            withSuccess = true;
            return propertyInfo.GetValue(instance, null);
        }

        private static ConstantExpression TryEvaluate(Expression expression, bool canThrow, out bool withSuccess) {
            if (expression.NodeType == ExpressionType.Constant) {
                withSuccess = true;
                return (ConstantExpression) expression;
            }
            if (expression.NodeType == ExpressionType.MemberAccess) {
                var result = GetMemberConstant((MemberExpression) expression, canThrow, out withSuccess);
                if (!withSuccess) { return null; }
                return result;
            }
            //if (expression.NodeType == ExpressionType.Parameter) {
            //}
            if (canThrow) {
                throw new NotSupportedException();
            }
            withSuccess = false;
            return null;
        }

        private static bool IsNullConstant(Expression exp) {
            return exp.NodeType == ExpressionType.Constant && ((ConstantExpression) exp).Value == null;
        }

        private bool ParseOrderByExpression(MethodCallExpression expression, string order) {
            var orderByInfo = GetColumnNameFromExpression(expression);
            var name = orderByInfo.Item1;
            var result = GetColumnNameFromProperty(name, order);
            if (result == null) {
                return true;
            }
            name = result.Item1;
            if (name == null) {
                return true;
            }
            if (result.Item2) {
                var bracketedName = name.StartsWith("[", StringComparison.Ordinal) ? name : "[" + name;
                bracketedName = bracketedName.EndsWith("]", StringComparison.Ordinal)
                    ? bracketedName
                    : bracketedName + "]";
                string enumOrderBy = null;
                string enumAliasedOrderBy = null;
                string enumSkipTakeAliasedOrderBy = null;
                if (orderByInfo.Item2.IsEnum) {
                    List<Tuple<int, string>> enumValues;
                    if (order.EqualsIgnoreCase("ASC")) {
                        enumValues = Enum.GetValues(orderByInfo.Item2).Cast<Enum>().Select(e => Tuple.Create(e.To<int>(), e.GetDescription())).OrderBy(e => e.Item2).ToSafeList();
                    } else {
                        enumValues = Enum.GetValues(orderByInfo.Item2).Cast<Enum>().Select(e => Tuple.Create(e.To<int>(), e.GetDescription())).OrderByDescending(e => e.Item2).ToSafeList();
                    }

                    if (!enumValues.IsEmpty()) {
                        var enumOrderByBuilder = new StringBuilder();
                        enumOrderByBuilder.AppendLine(bracketedName);
                        for (var i = 0; i < enumValues.Count; i++) {
                            var enumValue = enumValues[i].Item1;
                            enumOrderByBuilder.AppendLine($"WHEN {enumValue} THEN {i}");
                        }

                        enumOrderByBuilder.AppendLine("END");

                        enumOrderBy = $"CASE {enumOrderByBuilder}";
                        enumAliasedOrderBy = $"CASE {_currentParameterPrefix}{enumOrderByBuilder}";
                        enumSkipTakeAliasedOrderBy = $"CASE {_currentParameterPrefix.Replace(".", "_")}{enumOrderByBuilder.Replace("[", "")}";
                    }
                }
                if (OrderBy.IsNullOrEmpty()) {
                    if (enumOrderBy.IsNullOrWhiteSpace()) {
                        OrderBy = $"{bracketedName} {order}";
                        AliasedOrderBy = $"{_currentParameterPrefix}{bracketedName} {order}";
                        SkipTakeAliasedOrderBy = $"[{_currentParameterPrefix.Replace(".", "_")}{bracketedName.Replace("[", "")} {order}";
                    } else {
                        OrderBy = enumOrderBy;
                        AliasedOrderBy = enumAliasedOrderBy;
                        SkipTakeAliasedOrderBy = enumSkipTakeAliasedOrderBy;
                    }
                } else {
                    if (enumOrderBy.IsNullOrWhiteSpace()) {
                        OrderBy = $"{bracketedName} {order}, {OrderBy}";
                        AliasedOrderBy = $"{_currentParameterPrefix}{bracketedName} {order}, {AliasedOrderBy}";
                        SkipTakeAliasedOrderBy = $"[{_currentParameterPrefix.Replace(".", "_")}{bracketedName.Replace("[", "")} {order}, {SkipTakeAliasedOrderBy}";
                    } else {
                        OrderBy = $"{OrderBy}, {enumOrderBy}";
                        AliasedOrderBy = $"{AliasedOrderBy}, {enumAliasedOrderBy}";
                        SkipTakeAliasedOrderBy = $"{SkipTakeAliasedOrderBy}, {enumSkipTakeAliasedOrderBy}";
                    }
                }
            } else {
                LinkedOrderBy = LinkedOrderBy.IsNullOrWhiteSpace() ? name : $"{LinkedOrderBy}, {name}";
            }
            return true;
        }

        private bool ParseThenByExpression(MethodCallExpression expression, string order) {
            var orderByInfo = GetColumnNameFromExpression(expression);
            var name = orderByInfo.Item1;
            var result = GetColumnNameFromProperty(name, order);
            name = result.Item1;
            if (name == null) {
                return true;
            }
            if (result.Item2) {
                var bracketedName = name.StartsWith("[", StringComparison.Ordinal) ? name : "[" + name;
                bracketedName = bracketedName.EndsWith("]", StringComparison.Ordinal)
                    ? bracketedName
                    : bracketedName + "]";
                string enumOrderBy = null;
                string enumAliasedOrderBy = null;
                string enumSkipTakeAliasedOrderBy = null;
                if (orderByInfo.Item2.IsEnum) {
                    List<Tuple<int, string>> enumValues;
                    if (order.EqualsIgnoreCase("ASC")) {
                        enumValues = Enum.GetValues(orderByInfo.Item2).Cast<Enum>().Select(e => Tuple.Create(e.To<int>(), e.GetDescription())).OrderBy(e => e.Item2).ToSafeList();
                    } else {
                        enumValues = Enum.GetValues(orderByInfo.Item2).Cast<Enum>().Select(e => Tuple.Create(e.To<int>(), e.GetDescription())).OrderByDescending(e => e.Item2).ToSafeList();
                    }

                    if (!enumValues.IsEmpty()) {
                        var enumOrderByBuilder = new StringBuilder();
                        enumOrderByBuilder.AppendLine(bracketedName);
                        for (var i = 0; i < enumValues.Count; i++) {
                            var enumValue = enumValues[i].Item1;
                            enumOrderByBuilder.AppendLine($"WHEN {enumValue} THEN {i}");
                        }

                        enumOrderByBuilder.AppendLine("END");

                        enumOrderBy = $"CASE {enumOrderByBuilder}";
                        enumAliasedOrderBy = $"CASE {_currentParameterPrefix}{enumOrderByBuilder}";
                        enumSkipTakeAliasedOrderBy = $"CASE {_currentParameterPrefix.Replace(".", "_")}{enumOrderByBuilder.Replace("[", "")}";
                    }
                }
                if (OrderBy.IsNullOrEmpty()) {
                    if (enumOrderBy.IsNullOrWhiteSpace()) {
                        OrderBy = $"{bracketedName} {order}";
                        AliasedOrderBy = $"{_currentParameterPrefix}{bracketedName} {order}";
                        SkipTakeAliasedOrderBy = $"[{_currentParameterPrefix.Replace(".", "_")}{bracketedName.Replace("[", "")} {order}";
                    } else {
                        OrderBy = enumOrderBy;
                        AliasedOrderBy = enumAliasedOrderBy;
                        SkipTakeAliasedOrderBy = enumSkipTakeAliasedOrderBy;
                    }
                } else {
                    if (enumOrderBy.IsNullOrWhiteSpace()) {
                        OrderBy = $"{OrderBy}, {bracketedName} {order}";
                        AliasedOrderBy = $"{AliasedOrderBy}, {_currentParameterPrefix}{bracketedName} {order}";
                        SkipTakeAliasedOrderBy = $"{SkipTakeAliasedOrderBy}, [{_currentParameterPrefix.Replace(".", "_")}{bracketedName.Replace("[", "")} {order}";
                    } else {
                        OrderBy = $"{OrderBy}, {enumOrderBy}";
                        AliasedOrderBy = $"{AliasedOrderBy}, {enumAliasedOrderBy}";
                        SkipTakeAliasedOrderBy = $"{SkipTakeAliasedOrderBy}, {enumSkipTakeAliasedOrderBy}";
                    }
                }
            } else {
                LinkedOrderBy = LinkedOrderBy.IsNullOrWhiteSpace() ? name : $"{LinkedOrderBy}, {name}";
            }

            return true;
        }

        private static Tuple<string, Type> GetColumnNameFromExpression(MethodCallExpression expression) {
            var parsed = ParseMethodCallExpressionToBase(expression);
            if (parsed == null) {
                return null;
            }
            var constant = parsed.Item2;
            var body = parsed.Item1;
            string name;
            Type type;
            if (constant != null) {
                name = constant.Value?.ToString() ?? "";
                type = constant.Type;
            } else {
                name = body?.Member.Name ?? "";
                type = ((PropertyInfo) body?.Member).PropertyType;
            }
            return name.IsNullOrWhiteSpace() ? null : Tuple.Create(name, type);
        }

        private static Tuple<MemberExpression, ConstantExpression> ParseMethodCallExpressionToBase(
            MethodCallExpression expression) {
            var unary = (UnaryExpression) expression.Arguments[1];
            var lambdaExpression = (LambdaExpression) unary.Operand;

            lambdaExpression = (LambdaExpression) Evaluator.PartialEval(lambdaExpression);

            ConstantExpression constant = null;
            var body = lambdaExpression.Body as MemberExpression;
            if (body == null) {
                unary = lambdaExpression.Body as UnaryExpression;
                if (unary == null) {
                    constant = lambdaExpression.Body as ConstantExpression;
                    if (constant == null) {
                        var member = lambdaExpression.Body as MemberExpression;
                        if (member != null) {
                            constant = GetMemberConstant(lambdaExpression.Body as MemberExpression);
                        } else {
                            return null;
                        }
                    }
                } else {
                    lambdaExpression = unary.Operand as LambdaExpression;
                    if (lambdaExpression != null) {
                        lambdaExpression = (LambdaExpression) Evaluator.PartialEval(lambdaExpression);
                        body = lambdaExpression.Body as MemberExpression;
                    } else {
                        body = unary.Operand as MemberExpression;
                    }
                    if (body == null) {
                        return null;
                    }
                }
            }
            return Tuple.Create(body, constant);
        }

        private string GetColumnNameFromProperty(MemberExpression memberExpression) {
            var type = memberExpression.Member.DeclaringType.IsAbstract ? memberExpression.Expression.Type : memberExpression.Member.DeclaringType;
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(type);
            var prop = entityTypeInfo.GetPropertyInfo(memberExpression.Member.Name, null);
            return prop?.ColumnName.FormatWith(_currentTablePart);
        }

        private Tuple<string, bool> GetColumnNameFromProperty(string name, string order) {
            var prop = _typeInfo.GetPropertyInfo(name, null);
            if (prop != null) {
                return Tuple.Create("[" + prop.ColumnName.FormatWith(_currentTablePart) + "]", true);
            }
            var linkedEntitiesTree = EntityConverter.GetLinkedEntitiesTree(_typeInfo.EntityType, false);
            foreach (var node in linkedEntitiesTree) {
                prop = node.Value.EntityTypeInfo.GetPropertyInfo(name, null);
                if (prop == null) {
                    continue;
                }
                var joinColumnEntity = _typeInfo.LinkedEntitiesProperties.FirstOrDefault(p => p.BaseEntityType == node.Value.EntityTypeInfo.EntityType);
                if (joinColumnEntity == null) {
                    joinColumnEntity = _typeInfo.LinkedEntityProperties.FirstOrDefault(p => p.BaseEntityType == node.Value.EntityTypeInfo.EntityType);
                }
                if (joinColumnEntity == null) {
                    joinColumnEntity = _typeInfo.LinkedBaseResultsProperties.FirstOrDefault(p => p.BaseEntityType == node.Value.EntityTypeInfo.EntityType);
                }
                if (joinColumnEntity == null) {
                    joinColumnEntity = _typeInfo.LinkedBaseResultProperties.FirstOrDefault(p => p.BaseEntityType == node.Value.EntityTypeInfo.EntityType);
                }
                if (joinColumnEntity == null) {
                    continue;
                }
                var levelPrefix = BaseQueryBuilder.LevelAlias + node.Level + "_" + node.Value.Index;
                var primaryKey = _typeInfo.PrimaryKeyInfo?.ColumnName ?? joinColumnEntity.PrimaryKeyColumn;
                var primaryKeyColumnName = primaryKey.FormatWith(_currentTablePart);
                var propColumnName = prop.ColumnName.FormatWith(_currentTablePart);
                var fullTableName = node.Value.EntityTypeInfo.TableDefinition.FullTableName;
                if (node.Value.EntityTypeInfo.IsTableConsuming) {
                    fullTableName = string.Format(fullTableName, _currentTablePart);
                }
                var q = $"(SELECT TOP(1) {levelPrefix}.[{propColumnName}] FROM {fullTableName} {levelPrefix} " +
                        $"WHERE {levelPrefix}.{joinColumnEntity.ForeignKeyColumn} = {_currentParameterPrefix}{primaryKeyColumnName} " +
                        $"ORDER BY {levelPrefix}.LanguageID, {levelPrefix}.[{propColumnName}] {order}) {order}";
                return Tuple.Create(q, false);
            }
            return null;
        }

        private bool ParseTakeExpression(MethodCallExpression expression) {
            var sizeExpression = (ConstantExpression) expression.Arguments[1];

            int size;
            if (int.TryParse(sizeExpression.Value.ToString(), out size)) {
                Take = size;
                return true;
            }

            return false;
        }

        private bool ParseSkipExpression(MethodCallExpression expression) {
            var sizeExpression = (ConstantExpression) expression.Arguments[1];

            if (!int.TryParse(sizeExpression.Value.ToString(), out var size)) {
                return false;
            }
            Skip = size;
            return true;
        }

        private string GetNextParam() {
            return ParamPrefix + (Parameters.Count + 1);
        }

        private string AddToParameters(Type type, object value) {
            var parameter = new ParameterContainer { Type = type, Value = value };
            if (Parameters.TryGetValue(parameter, out var existingParam)) {
                return existingParam.Name;
            }
            parameter.Name = GetNextParam();
            Parameters.Add(parameter);
            return parameter.Name;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/21382175/how-to-rewrite-expression-x-x-to-x-x-true-and-x-x-to-x-x-true
        /// Expression visitor that expands x => x.BoolProperty to x => x.BoolProperty == true or  x => !x.BoolProperty to x => x.BoolProperty != true
        /// </summary>
        internal class BooleanComplexifier : ExpressionVisitor {
            public static Expression<T> Process<T>(Expression<T> expression) {
                if (expression == null) { return null; }
                return (Expression<T>) new BooleanComplexifier().Visit(expression);
            }
            public static Expression Process(Expression expression) {
                if (expression == null) { return null; }
                return new BooleanComplexifier().Visit(expression);
            }

            private int _bypass;
            protected override Expression VisitBinary(BinaryExpression node) {
                if (_bypass == 0 && node.Type == typeof(bool)) {
                    switch (node.NodeType) {
                        case ExpressionType.And: // bitwise & - different to &&
                        case ExpressionType.Or:  // bitwise | - different to ||
                        case ExpressionType.Equal:
                        case ExpressionType.NotEqual:
                            _bypass++;
                            var result = base.VisitBinary(node);
                            _bypass--;
                            return result;
                    }
                }
                return base.VisitBinary(node);
            }
            protected override Expression VisitUnary(UnaryExpression node) {
                if (_bypass == 0 && node.Type == typeof(bool) && node.Operand is MemberExpression) {
                    switch (node.NodeType) {
                        case ExpressionType.Not:
                            _bypass++;
                            var result = Expression.NotEqual(base.Visit(node.Operand), Expression.Constant(true));
                            _bypass--;
                            return result;
                    }
                }
                return base.VisitUnary(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression m) {
                if (m.Method.Name == "OrderBy" || m.Method.Name == "OrderByDescending" || m.Method.Name == "ThenBy" || m.Method.Name == "ThenByDescending") {
                    return m;
                }
                if (_bypass == 0 && m.Method.DeclaringType == typeof(Sql) && m.Method.ReturnType == typeof(bool)) {
                    return Expression.Equal(m, Expression.Constant(true));
                }

                return base.VisitMethodCall(m);
            }

            protected override Expression VisitMember(MemberExpression node) {
                if (_bypass == 0 && node.Type == typeof(bool)) {
                    return Expression.Equal(base.VisitMember(node), Expression.Constant(true));
                }
                return base.VisitMember(node);
            }
        }

        /// <summary>
        /// Class for evaluating expressions
        /// </summary>
        private static class Evaluator {
            /// <summary>
            /// Performs evaluation & replacement of independent sub-trees
            /// </summary>
            /// <param name="expression">The root of the expression tree.</param>
            /// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
            /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
            public static Expression PartialEval(Expression expression, Func<Expression, bool> fnCanBeEvaluated) {
                return new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
            }

            /// <summary>
            /// Performs evaluation & replacement of independent sub-trees
            /// </summary>
            /// <param name="expression">The root of the expression tree.</param>
            /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
            public static Expression PartialEval(Expression expression) {
                return PartialEval(expression, CanBeEvaluatedLocally);
            }

            private static bool CanBeEvaluatedLocally(Expression expression) {
                return expression.NodeType != ExpressionType.Parameter;
            }


            /// <summary>
            /// Evaluates & replaces sub-trees when first candidate is reached (top-down)
            /// </summary>
            private class SubtreeEvaluator : ExpressionVisitor {
                private readonly HashSet<Expression> _candidates;

                internal SubtreeEvaluator(HashSet<Expression> candidates) {
                    _candidates = candidates;
                }

                internal Expression Eval(Expression exp) {
                    return Visit(exp);
                }

                public override Expression Visit(Expression exp) {
                    if (exp == null) {
                        return null;
                    }

                    if (_candidates.Contains(exp)) {
                        return Evaluate(exp);
                    }
                    return base.Visit(exp);
                }

                private static Expression Evaluate(Expression e) {
                    if (e.NodeType == ExpressionType.Constant) {
                        return e;
                    }

                    var lambda = Expression.Lambda(e);
                    var fn = lambda.Compile();
                    return Expression.Constant(fn.DynamicInvoke(null), e.Type);
                }
            }

            /// <summary>
            /// Performs bottom-up analysis to determine which nodes can possibly
            /// be part of an evaluated sub-tree.
            /// </summary>
            private class Nominator : ExpressionVisitor {
                private readonly Func<Expression, bool> _fnCanBeEvaluated;
                private HashSet<Expression> _candidates;
                private bool _cannotBeEvaluated;

                internal Nominator(Func<Expression, bool> fnCanBeEvaluated) {
                    _fnCanBeEvaluated = fnCanBeEvaluated;
                }

                internal HashSet<Expression> Nominate(Expression expression) {
                    _candidates = new HashSet<Expression>();
                    Visit(expression);
                    return _candidates;
                }

                public override Expression Visit(Expression expression) {
                    if (expression != null) {
                        var saveCannotBeEvaluated = _cannotBeEvaluated;
                        _cannotBeEvaluated = false;
                        base.Visit(expression);

                        if (!_cannotBeEvaluated) {
                            if (_fnCanBeEvaluated(expression)) {
                                _candidates.Add(expression);
                            } else {
                                _cannotBeEvaluated = true;
                            }
                        }
                        _cannotBeEvaluated |= saveCannotBeEvaluated;
                    }
                    return expression;
                }
            }
        }

        public string GetOrderBy(string baseLevelAlias, string columnName) {
            string orderBy;
            if (AliasedOrderBy.IsNullOrWhiteSpace()) {
                if (OrderBy.IsNullOrWhiteSpace()) {
                    if (!LinkedOrderBy.IsNullOrWhiteSpace()) {
                        orderBy = LinkedOrderBy;
                    } else {
                        orderBy = $"{baseLevelAlias}.{columnName}";
                    }
                } else {
                    orderBy = $"{baseLevelAlias}.{columnName}";
                }
            } else {
                orderBy = AliasedOrderBy;
            }
            return orderBy;
        }
    }
}
