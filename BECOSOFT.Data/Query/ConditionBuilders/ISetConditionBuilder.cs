using System;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Query.ConditionBuilders {
    public interface ISetConditionBuilder {
        IConditionOperaterBuilder Append<T>(Expression<Func<T, object>> property);
        IConditionOperaterBuilder Append(string parameter);
        IConditionOperaterBuilder And();
        IConditionOperaterBuilder Or();
        IConditionOperaterBuilder Not();
        IConditionOperaterBuilder And<T>(Expression<Func<T, object>> property);
        IConditionOperaterBuilder And(string parameter);
        IConditionOperaterBuilder Or<T>(Expression<Func<T, object>> property);
        IConditionOperaterBuilder Not<T>(Expression<Func<T, object>> property);
        IConditionOperaterBuilder AndOpenParentheses<T>(Expression<Func<T, object>> property);
        IConditionOperaterBuilder OrOpenParentheses<T>(Expression<Func<T, object>> property);
        IConditionOperaterBuilder NotOpenParentheses<T>(Expression<Func<T, object>> property);
        IConditionOperaterBuilder And(string tableAlias, string column);
        IConditionOperaterBuilder Or(string tableAlias, string column);
        IConditionOperaterBuilder Not(string tableAlias, string column);
        IConditionOperaterBuilder AndOpenParentheses(string tableAlias, string column);
        IConditionOperaterBuilder OrOpenParentheses(string tableAlias, string column);
        IConditionOperaterBuilder NotOpenParentheses(string tableAlias, string column);
        IConditionOperaterBuilder OpenParentheses();
        ISetConditionBuilder CloseParentheses();
        string ToSql();
    }
}
