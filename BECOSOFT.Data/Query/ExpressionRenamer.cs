using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Extensions;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Query {
    /// <summary>
    /// Represents a renamer for an expression
    /// </summary>
    public class ExpressionRenamer : ExpressionVisitor {
        private ParameterExpression _parameterExpression;

        /// <summary>
        /// Renames an expression
        /// </summary>
        /// <param name="expression">The expression to rename</param>
        /// <param name="parameterExpression">The parameterexpression</param>
        /// <returns>The renamed expression</returns>
        public Expression Rename(Expression expression, ParameterExpression parameterExpression) {
            _parameterExpression = parameterExpression;
            return Visit(expression);
        }

        /// <inheritdoc />
        protected override Expression VisitParameter(ParameterExpression node) {
            if (node.Type.IsInterfaceImplementationOf<IEntity>()) {
                return _parameterExpression ?? Expression.Parameter(node.Type, "e");
            }
            return node;
        }
    }
}