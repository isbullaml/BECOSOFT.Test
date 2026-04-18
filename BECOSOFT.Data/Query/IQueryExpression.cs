using System.Linq.Expressions;

namespace BECOSOFT.Data.Query {
    /// <summary>
    /// A class representing a query-expression
    /// </summary>
    public interface IQueryExpression {
        /// <summary>
        /// Converts the current <see cref="IQueryExpression"/> to an <see cref="Expression"/>
        /// </summary>
        /// <returns>The converted expression</returns>
        Expression ToExpression();
    }
}