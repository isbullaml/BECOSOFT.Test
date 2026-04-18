using BECOSOFT.Data.Helpers;
using System;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Models {
    public class OrderDefinition<T> {
        /// <summary>
        /// Value indicating whether the items should be sorted ascending
        /// </summary>
        public bool IsAsc { get; set; }
        /// <summary>
        /// ORDER BY Expression
        /// </summary>
        public Expression<Func<T, object>> OrderByExpression { get; set; }

        public OrderDefinition(Expression<Func<T, object>> orderByExpression, bool isAsc = true) {
            OrderByExpression = orderByExpression;
            IsAsc = isAsc;
        }

        public OrderDefinition(string propertyName, bool isAsc = true) {
            try {
                OrderByExpression = PropertyExpressionParserManager.GetPropertySelector<T>(propertyName);
                IsAsc = isAsc;
            } catch (Exception e) {
                //TODO: Invalid property?
            }
        }
    }
}
