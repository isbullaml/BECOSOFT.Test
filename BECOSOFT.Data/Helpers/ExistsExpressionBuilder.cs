using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Extensions;
using System;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Helpers {
    /// <summary>
    /// An expression-builder for building the WHERE-part to check
    /// if an entity exists
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExistsExpressionBuilder<T> where T : BaseEntity {
        private Expression<Func<T, bool>> _expression;
        private bool _reverseID;
        private readonly long _entityID;

        /// <summary>
        /// The constructor.
        /// This will create an expression-builder that already contains the ID-check
        /// </summary>
        /// <param name="id">The ID of the entity</param>
        public ExistsExpressionBuilder(long id = 0) {
            _reverseID = false;
            _entityID = id;
        }

        /// <summary>
        /// The constructor.
        /// This will create an expression-builder that already contains the ID-check
        /// </summary>
        /// <param name="id">The ID of the entity</param>
        /// <param name="comparison">Extra conditions</param>
        public ExistsExpressionBuilder(long id, Expression<Func<T, bool>> comparison) {
            _reverseID = false;
            _entityID = id;
            _expression = comparison;
        }

        /// <summary>
        /// Adds an OR to the expression if the <see cref="condition"/> is met.
        /// This will invert the ID-check.
        /// </summary>
        /// <param name="comparison"></param>
        /// <param name="condition"></param>
        public void Or(Expression<Func<T, bool>> comparison, bool condition = true) {
            if (!condition) { return; }
            _reverseID = true;
            _expression = _expression.OrElse(comparison);
        }

        /// <summary>
        /// Adds an AND to the expression if the <see cref="condition"/> is met.
        /// This will invert the ID-check.
        /// </summary>
        /// <param name="comparison"></param>
        /// <param name="condition"></param>
        public void And(Expression<Func<T, bool>> comparison, bool condition = true) {
            if (!condition) { return; }
            _reverseID = true;
            _expression = _expression.AndAlso(comparison);
        }

        /// <summary>
        /// Retrieve the built expression.
        /// </summary>
        /// <returns></returns>
        public Expression<Func<T, bool>> ToExpression() {
            var expression = _expression;
            if (expression == null) { return ent => ent.Id == _entityID; }
            if (_reverseID) {
                if (_entityID != 0) {
                    expression = _expression.AndAlso(ent => ent.Id != _entityID);
                }
            } else {
                if (_entityID != 0) {
                    expression = _expression.AndAlso(ent => ent.Id == _entityID);
                }
            }
            return expression;
        }
    }
}