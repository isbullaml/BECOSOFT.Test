using BECOSOFT.Data.Context;
using BECOSOFT.Data.Extensions;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Extensions;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace BECOSOFT.Data.Query.JoinBuilders {
    public class JoinBuilder<T> : JoinBuilder, ITypedJoinConditionBuilder<T>, ISetJoinBuilder<T> where T : IEntity {
        public JoinBuilder(ISetJoinBuilder joinBuilder) {
            var type = joinBuilder.GetType();
            var field = type.GetField("Query", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var value = field?.GetValue(joinBuilder);
            Query = value as StringBuilder;
        }

        public ISetJoinBuilder<T> On<TRight>(Expression<Func<T, object>> leftKeySelector, Expression<Func<TRight, object>> rightKeySelector) {
            var tableAlias = leftKeySelector.Parameters[0].Name;
            var leftColumn = GetColumn(leftKeySelector);
            var rightColumn = GetColumn(rightKeySelector);
            Query.AppendLine($" {tableAlias} ON {leftColumn} = {rightColumn}");
            return this;
        }

        public ISetJoinBuilder<T> On(Expression<Func<T, object>> leftKeySelector, string right) {
            var tableAlias = leftKeySelector.Parameters[0].Name;
            var leftColumn = GetColumn(leftKeySelector);
            Query.AppendLine($" {tableAlias} ON {leftColumn} = {right}");
            return this;
        }

        public ITypedJoinConditionBuilder<T> LeftJoin(string tablePart = null) {
            var tableName = tablePart.IsNullOrWhiteSpace() ? Entity.GetFullTable<T>() : Entity.GetFullTable<T>(tablePart);
            Query.Append($"LEFT JOIN {tableName}");
            return this;
        }

        public ITypedJoinConditionBuilder<T> InnerJoin(string tablePart = null) {
            var tableName = tablePart.IsNullOrWhiteSpace() ? Entity.GetFullTable<T>() : Entity.GetFullTable<T>(tablePart);
            Query.Append($"INNER JOIN {tableName}");
            return this;
        }

        public ISetJoinBuilder AndOn<TRight>(Expression<Func<T, object>> leftKeySelector, Expression<Func<TRight, object>> rightKeySelector) {
            var leftColumn = GetColumn(rightKeySelector);
            var rightColumn = GetColumn(leftKeySelector);
            Query.AppendLine($" AND {leftColumn} = {rightColumn}");
            return this;
        }

        public ISetJoinBuilder OrOn<TRight>(Expression<Func<T, object>> leftKeySelector, Expression<Func<TRight, object>> rightKeySelector) {
            var leftColumn = GetColumn(leftKeySelector);
            var rightColumn = GetColumn(rightKeySelector);
            Query.AppendLine($" AND {leftColumn} = {rightColumn}");
            return this;
        }
    }

    public class JoinBuilder : IJoinConditionBuilder, ISetJoinBuilder {
        protected StringBuilder Query;

        public JoinBuilder() {
            Query = new StringBuilder();
        }

        public ITypedJoinConditionBuilder<T> LeftJoin<T>(string tablePart = null) where T : IEntity {
            var tableName = tablePart.IsNullOrWhiteSpace() ? Entity.GetFullTable<T>() : Entity.GetFullTable<T>(tablePart);
            Query.Append($"LEFT JOIN {tableName}");
            return new JoinBuilder<T>(this);
        }

        public ITypedJoinConditionBuilder<T> InnerJoin<T>(string tablePart = null) where T : IEntity {
            var tableName = tablePart.IsNullOrWhiteSpace() ? Entity.GetFullTable<T>() : Entity.GetFullTable<T>(tablePart);
            Query.Append($"INNER JOIN {tableName}");
            return new JoinBuilder<T>(this);
        }

        public IJoinConditionBuilder LeftJoin(string tableName, string alias) {
            Query.Append($"LEFT JOIN {tableName} {alias}");
            return this;
        }

        public IJoinConditionBuilder InnerJoin(string tableName, string alias) {
            Query.Append($"INNER JOIN {tableName} {alias}");
            return this;
        }

        public ISetJoinBuilder<TLeft> On<TLeft>(Expression<Func<TLeft, object>> leftKeySelector, string rightColumn) where TLeft : IEntity {
            var leftColumn = GetColumn(leftKeySelector);
            Query.AppendLine($" ON {leftColumn} = {rightColumn}");
            return new JoinBuilder<TLeft>(this);
        }

        public ISetJoinBuilder On(string left, string right) {
            Query.AppendLine($" ON {left} = {right}");
            return this;
        }

        public ISetJoinBuilder AndOn<TLeft>(Expression<Func<TLeft, object>> leftKeySelector, string parameter) {
            var leftColumn = GetColumn(leftKeySelector);
            Query.AppendLine($" AND {leftColumn} = {parameter}");
            return this;
        }

        public ISetJoinBuilder OrOn<TLeft>(Expression<Func<TLeft, object>> leftKeySelector, string parameter) {
            var leftColumn = GetColumn(leftKeySelector);
            Query.AppendLine($" OR {leftColumn} = {parameter}");
            return this;
        }

        public ISetJoinBuilder AndOn(string left, string right) {
            Query.AppendLine($" AND {left} = {right}");
            return this;
        }

        public ISetJoinBuilder OrOn(string left, string right) {
            Query.AppendLine($" OR {left} = {right}");
            return this;
        }

        public string ToSql() {
            if (Query.Length == 0) {
                return string.Empty;
            }

            return Query.ToString();
        }

        protected string GetColumn<T>(Expression<Func<T, object>> property) {
            if (property.Body.NodeType == ExpressionType.Convert) {
                var unaryExpression = (property.Body as UnaryExpression);
                if (unaryExpression?.Operand is MethodCallExpression body) {
                    var result = HandleSqlFunction(body);

                    if (!result.IsNullOrWhiteSpace()) {
                        return result;
                    }
                }
            }

            var column = property.GetProperty().ColumnName;
            var tableAlias = property.Parameters[0].Name;
            return $"{tableAlias}.{column}";
        }

        protected string HandleSqlFunction(MethodCallExpression m) {
            var methodName = m.Method.Name.ToUpper();
            switch (methodName) {
                case "ISNULL":
                case "NULLIF":
                    var arg0 = m.Arguments[0];
                    var arg1 = m.Arguments[1];
                    return $"{methodName}({arg0}, {arg1})";
                default:
                    return string.Empty;
            }
        }
    }

    public interface ITypedJoinConditionBuilder<TLeft> where TLeft : IEntity {
        ISetJoinBuilder<TLeft> On<TRight>(Expression<Func<TLeft, object>> leftKeySelector, Expression<Func<TRight, object>> rightKeySelector);
        ISetJoinBuilder<TLeft> On(Expression<Func<TLeft, object>> leftKeySelector, string right);
    }

    public interface IJoinConditionBuilder {
        ISetJoinBuilder<TLeft> On<TLeft>(Expression<Func<TLeft, object>> leftKeySelector, string rightColumn) where TLeft : IEntity;
        ISetJoinBuilder On(string left, string right);
    }

    public interface ISetJoinBuilder {
        ITypedJoinConditionBuilder<T> LeftJoin<T>(string tablePart = null) where T : IEntity;
        ITypedJoinConditionBuilder<T> InnerJoin<T>(string tablePart = null) where T : IEntity;
        IJoinConditionBuilder LeftJoin(string tableName, string alias);
        IJoinConditionBuilder InnerJoin(string tableName, string alias);
        ISetJoinBuilder AndOn<TLeft>(Expression<Func<TLeft, object>> leftKeySelector, string parameter);
        ISetJoinBuilder OrOn<TLeft>(Expression<Func<TLeft, object>> leftKeySelector, string parameter);
        ISetJoinBuilder AndOn(string left, string right);
        ISetJoinBuilder OrOn(string left, string right);
        string ToSql();
    }

    public interface ISetJoinBuilder<TLeft> : ISetJoinBuilder where TLeft : IEntity {
        ITypedJoinConditionBuilder<TLeft> LeftJoin(string tablePart = null);
        ITypedJoinConditionBuilder<TLeft> InnerJoin(string tablePart = null);
        ISetJoinBuilder AndOn<TRight>(Expression<Func<TLeft, object>> leftKeySelector, Expression<Func<TRight, object>> rightKeySelector);
        ISetJoinBuilder OrOn<TRight>(Expression<Func<TLeft, object>> leftKeySelector, Expression<Func<TRight, object>> rightKeySelector);
    }
}