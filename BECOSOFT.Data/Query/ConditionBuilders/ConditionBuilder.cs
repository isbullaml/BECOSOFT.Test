using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace BECOSOFT.Data.Query.ConditionBuilders {
    public class ConditionBuilder : IConditionOperaterBuilder, ISetConditionBuilder {
        private readonly StringBuilder _query;
        private int _parentheseCount;
        private bool _firstPart;

        public ConditionBuilder() {
            _query = new StringBuilder();
            _parentheseCount = 0;
            _firstPart = true;
        }

        public bool IsSet => _query.Length > 0;

        public ISetConditionBuilder Between(string minParameter, string maxParameter) {
            _query.Append($" BETWEEN {minParameter} AND {maxParameter}");
            return this;
        }

        public ISetConditionBuilder EqualTo(string parameter) {
            _query.Append($" = {parameter}");
            return this;
        }

        public ISetConditionBuilder EqualToNull() {
            _query.Append(" IS NULL");
            return this;
        }
        
        public ISetConditionBuilder GreaterThan(string parameter) {
            _query.Append($" > {parameter}");
            return this;
        }

        public ISetConditionBuilder GreaterThanOrEqualTo(string parameter) {
            _query.Append($" >= {parameter}");
            return this;
        }

        public ISetConditionBuilder In(IEnumerable<string> parameters) {
            _query.Append($" IN ({string.Join(",", parameters)})");
            return this;
        }

        public ISetConditionBuilder In(string tempTable, string column) {
            _query.Append($" IN (SELECT x.{column} FROM {tempTable} x)");
            return this;
        }

        public ISetConditionBuilder In(string subQuery) {
            _query.Append($" IN ({subQuery})");
            return this;
        }

        public ISetConditionBuilder NotIn(IEnumerable<string> parameters) {
            _query.Append($" NOT IN ({string.Join(",", parameters)})");
            return this;
        }

        public ISetConditionBuilder NotIn(string subQuery) {
            _query.Append($" NOT IN ({subQuery})");
            return this;
        }

        public ISetConditionBuilder LessThan(string parameter) {
            _query.Append($" < {parameter}");
            return this;
        }

        public ISetConditionBuilder LessThanOrEqualTo(string parameter) {
            _query.Append($" <= {parameter}");
            return this;
        }

        public ISetConditionBuilder Like(string parameter) {
            _query.Append($" LIKE {parameter}");
            return this;
        }

        public ISetConditionBuilder NotEqualTo(string parameter) {
            _query.Append($" <> {parameter}");
            return this;
        }

        public ISetConditionBuilder NotEqualToNull() {
            _query.Append(" IS NOT NULL");
            return this;
        }

        public IConditionOperaterBuilder Append<T>(Expression<Func<T, object>> property) {
            var column = GetColumn(property);
            _query.AppendLine($" {column}");
            _firstPart = false;
            return this;
        }

        public IConditionOperaterBuilder Append(string parameter) {
            _query.AppendLine($" {parameter}");
            _firstPart = false;
            return this;
        }

        public IConditionOperaterBuilder And() {
            _query.AppendLine(" AND");
            return this;
        }

        public IConditionOperaterBuilder Or() {
            _query.AppendLine(" OR");
            return this;
        }

        public IConditionOperaterBuilder Not() {
            _query.AppendLine(" NOT");
            return this;
        }

        public IConditionOperaterBuilder And<T>(Expression<Func<T, object>> property) {
            var column = GetColumn(property);
            if (_firstPart) {
                _query.AppendLine($" {column}");
                _firstPart = false;
            } else {
                _query.AppendLine($" AND {column}");
            }

            return this;
        }

        public IConditionOperaterBuilder And(string parameter) {
            if (_firstPart) {
                _query.AppendLine($" {parameter}");
                _firstPart = false;
            } else {
                _query.AppendLine($" AND {parameter}");
            }

            return this;
        }

        public IConditionOperaterBuilder AndIsNull<T, TNull>(Expression<Func<T, object>> property, Expression<Func<TNull, object>> nullProperty) {
            var column = GetColumn(property);
            var nullColumn = GetColumn(nullProperty);
            if (_firstPart) {
                _query.AppendLine($" ISNULL({column}, {nullColumn})");
                _firstPart = false;
            } else {
                _query.AppendLine($" AND ISNULL({column}, {nullColumn})");
            }

            return this;
        }

        public IConditionOperaterBuilder Or<T>(Expression<Func<T, object>> property) {
            var column = GetColumn(property);
            if (_firstPart) {
                _query.AppendLine($" {column}");
                _firstPart = false;
            } else {
                _query.AppendLine($" OR {column}");
            }

            return this;
        }

        public IConditionOperaterBuilder OrIsNull<T, TNull>(Expression<Func<T, object>> property, Expression<Func<TNull, object>> nullProperty) {
            var column = GetColumn(property);
            var nullColumn = GetColumn(nullProperty);
            if (_firstPart) {
                _query.AppendLine($" ISNULL({column}, {nullColumn})");
                _firstPart = false;
            } else {
                _query.AppendLine($" OR ISNULL({column}, {nullColumn})");
            }

            return this;
        }

        public IConditionOperaterBuilder Not<T>(Expression<Func<T, object>> property) {
            var column = GetColumn(property);
            if (_firstPart) {
                _query.AppendLine($" {column}");
                _firstPart = false;
            } else {
                _query.AppendLine($" NOT {column}");
            }

            return this;
        }

        public IConditionOperaterBuilder AndOpenParentheses<T>(Expression<Func<T, object>> property) {
            var column = GetColumn(property);
            if (_firstPart) {
                _query.AppendLine($" ({column}");
                _firstPart = false;
            } else {
                _query.AppendLine($" AND ({column}");
            }

            _parentheseCount++;
            return this;
        }

        public IConditionOperaterBuilder OrOpenParentheses<T>(Expression<Func<T, object>> property) {
            var column = GetColumn(property);
            if (_firstPart) {
                _query.AppendLine($" ({column}");
                _firstPart = false;
            } else {
                _query.AppendLine($" OR ({column}");
            }

            _parentheseCount++;
            return this;
        }

        public IConditionOperaterBuilder NotOpenParentheses<T>(Expression<Func<T, object>> property) {
            var column = GetColumn(property);
            if (_firstPart) {
                _query.AppendLine($" ({column}");
                _firstPart = false;
            } else {
                _query.AppendLine($" NOT ({column}");
            }

            _parentheseCount++;
            return this;
        }

        public IConditionOperaterBuilder And(string tableAlias, string column) {
            var fullColumn = $"{tableAlias}.{column}";
            if (_firstPart) {
                _query.AppendLine($" {fullColumn}");
                _firstPart = false;
            } else {
                _query.AppendLine($" AND {fullColumn}");
            }

            return this;
        }

        public IConditionOperaterBuilder Or(string tableAlias, string column) {
            var fullColumn = $"{tableAlias}.{column}";
            if (_firstPart) {
                _query.AppendLine($" {fullColumn}");
                _firstPart = false;
            } else {
                _query.AppendLine($" OR {fullColumn}");
            }

            return this;
        }

        public IConditionOperaterBuilder Not(string tableAlias, string column) {
            var fullColumn = $"{tableAlias}.{column}";
            if (_firstPart) {
                _query.AppendLine($" {fullColumn}");
                _firstPart = false;
            } else {
                _query.AppendLine($" NOT {fullColumn}");
            }

            return this;
        }

        public IConditionOperaterBuilder AndOpenParentheses(string tableAlias, string column) {
            var fullColumn = $"{tableAlias}.{column}";
            if (_firstPart) {
                _query.AppendLine($" ({fullColumn}");
                _firstPart = false;
            } else {
                _query.AppendLine($" AND ({fullColumn}");
            }

            _parentheseCount++;
            return this;
        }

        public IConditionOperaterBuilder OrOpenParentheses(string tableAlias, string column) {
            var fullColumn = $"{tableAlias}.{column}";
            if (_firstPart) {
                _query.AppendLine($" ({fullColumn}");
                _firstPart = false;
            } else {
                _query.AppendLine($" OR ({fullColumn}");
            }

            _parentheseCount++;
            return this;
        }

        public IConditionOperaterBuilder NotOpenParentheses(string tableAlias, string column) {
            var fullColumn = $"{tableAlias}.{column}";
            if (_firstPart) {
                _query.AppendLine($" ({fullColumn}");
                _firstPart = false;
            } else {
                _query.AppendLine($" NOT ({fullColumn}");
            }

            _parentheseCount++;
            return this;
        }

        public IConditionOperaterBuilder OpenParentheses() {
            _query.AppendLine(" (");
            _parentheseCount++;
            return this;
        }

        public ISetConditionBuilder CloseParentheses() {
            _query.Append(")");
            _parentheseCount--;
            return this;
        }

        public string ToSql() {
            if (_parentheseCount != 0) {
                throw new Exception($"Invalid part, parantheses aren't correct (${_parentheseCount}).");
            }

            if (_query.Length == 0) {
                return string.Empty;
            }

            return $"WHERE {_query}";
        }

        private string GetColumn<T>(Expression<Func<T, object>> property) {
            var propertyTranslator = new PropertyTranslator();
            return propertyTranslator.Translate(property);
        }
    }
}
