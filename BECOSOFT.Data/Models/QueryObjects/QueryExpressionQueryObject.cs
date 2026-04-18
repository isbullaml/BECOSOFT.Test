using BECOSOFT.Data.Query;
using System;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Models.QueryObjects {
    /// <inheritdoc />
    /// <summary>
    /// <see cref="QueryObject" /> for queries containing an <see cref="IQueryExpression" />
    /// </summary>
    public class QueryExpressionQueryObject : QueryObject {
        /// <summary>
        /// The query-expression
        /// </summary>
        internal IQueryExpression QueryExpression { get; set; }

        public QueryExpressionQueryObject(IQueryExpression queryExpression = null, string tablePart = null, bool distinct = false) : base(tablePart, distinct) {
            QueryExpression = queryExpression;
        }

        public static QueryExpressionQueryObject FromWhere<T>(Expression<Func<T, bool>> where = null, string tablePart = null, bool distinct = false) {
            var queryExpression = new QueryExpression<T>(where);
            return new QueryExpressionQueryObject(queryExpression, tablePart, distinct);
        }
    }
}