using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;

namespace BECOSOFT.Data.Repositories {
    internal static class RepositoryHelper {
        internal static Expression<Func<TEntity, bool>> GenerateDeleteExpression<TEntity>(IEnumerable<int> primaryKeyIDs, IEnumerable<int> foreignKeyIDs, 
                                                                                          Expression<Func<TEntity, int>> foreignKeyExpression) where TEntity : BaseEntity {
            var primaryKeyIdsToKeep = (primaryKeyIDs?.Distinct()).ToSafeList();
            var distinctForeignKeyIDs = (foreignKeyIDs?.Distinct()).ToSafeList();
            if (primaryKeyIdsToKeep.IsEmpty() && distinctForeignKeyIDs.IsEmpty()) {
                return null;
            }
            var listType = typeof(List<int>);
            var containsMethod = listType.GetMethod("Contains", new[] { typeof(int) });
            if (containsMethod == null) {
                throw new ArgumentException($"Method 'Contains' does not exist on {listType.Name}", nameof(containsMethod));
            }
            var parameter = Expression.Parameter(typeof(TEntity), "e");

            var foreignKeyProperty = "";
            if (foreignKeyExpression.Body.NodeType == ExpressionType.MemberAccess) {
                var me = (MemberExpression) foreignKeyExpression.Body;
                var entityTypInfo = EntityConverter.GetEntityTypeInfo(typeof(TEntity));
                var entityPropertyInfo = entityTypInfo.GetPropertyInfo(me.Member.Name, null);
                if (entityPropertyInfo != null) {
                    foreignKeyProperty = entityPropertyInfo.PropertyName;
                }
            }
            if (foreignKeyProperty.IsNullOrWhiteSpace()) {
                throw new ArgumentException("Invalid expression passed. The expression body must be a MemberExpression", nameof(foreignKeyExpression));
            }
            Expression<Func<TEntity, bool>> deleteExpression = null;
            if (distinctForeignKeyIDs.HasAny()) {
                deleteExpression = Expression.Lambda<Func<TEntity, bool>>(
                    Expression.Call(
                        Expression.Constant(distinctForeignKeyIDs), containsMethod, 
                        Expression.Property(parameter, foreignKeyProperty)), parameter);
            }
            if (primaryKeyIdsToKeep.HasAny()) {
                deleteExpression = deleteExpression.AndAlso(e => !primaryKeyIdsToKeep.Contains(e.Id));
            }

            return deleteExpression;
        }
    }
}
