using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Repositories {
    public static class RepositoryExtensions {
        /// <summary>
        /// Execute a DELETE statement with the provided <see cref="primaryKeyIDs"/> and <see cref="foreignKeyID"/>. If <see cref="foreignKeyID"/> = 0, nothing happens and the function quits.
        /// Otherwise a DELETE statement is executed.
        /// If there are <see cref="primaryKeyIDs"/> present, the DELETE statement is of format "DELETE FROM x WHERE x.ID IN (<see cref="primaryKeyIDs"/>) AND x.<see cref="foreignKeyExpression"/> NOT IN (<see cref="foreignKeyID"/>)".
        /// If there are no <see cref="primaryKeyIDs"/> present, the DELETE statement is of format "DELETE FROM x WHERE x.<see cref="foreignKeyExpression"/> NOT IN (<see cref="foreignKeyID"/>)".
        /// </summary>
        /// <typeparam name="TRepository"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="repository">Repository to execute the DELETE statement on.</param>
        /// <param name="primaryKeyIDs">Collection of primary key IDs of the <see cref="TEntity"/>.</param>
        /// <param name="foreignKeyID">Foreign key ID that won't be deleted.</param>
        /// <param name="foreignKeyExpression">Expression to retrieve the property of the foreign key.</param>
        /// <param name="tablePart">Optional table part</param>
        public static void DeleteNotIn<TRepository, TEntity>(this TRepository repository,
                                                             IEnumerable<int> primaryKeyIDs, int foreignKeyID,
                                                             Expression<Func<TEntity, int>> foreignKeyExpression, string tablePart = null)
            where TRepository : IRepository<TEntity> where TEntity : BaseEntity {
            if (foreignKeyID == 0) { return; }
            var foreignKeyIDs = new List<int> { foreignKeyID };
            DeleteNotIn(repository, primaryKeyIDs, foreignKeyIDs, foreignKeyExpression, tablePart);
        }
        /// <summary>
        /// Execute a DELETE statement with the provided <see cref="primaryKeyIDs"/> and <see cref="foreignKeyIDs"/>. If <see cref="primaryKeyIDs"/> is empty and <see cref="foreignKeyIDs"/> is empty, nothing happens and the function quits.
        /// Otherwise a DELETE statement is executed.
        /// If there are <see cref="primaryKeyIDs"/> present, the DELETE statement is of format "DELETE FROM x WHERE x.ID IN (<see cref="primaryKeyIDs"/>) AND x.<see cref="foreignKeyExpression"/> NOT IN (<see cref="foreignKeyIDs"/>)".
        /// If there are no <see cref="primaryKeyIDs"/> present, the DELETE statement is of format "DELETE FROM x WHERE x.<see cref="foreignKeyExpression"/> NOT IN (<see cref="foreignKeyIDs"/>)".
        /// </summary>
        /// <typeparam name="TRepository"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="repository">Repository to execute the DELETE statement on.</param>
        /// <param name="primaryKeyIDs">Collection of primary key IDs of the <see cref="TEntity"/>.</param>
        /// <param name="foreignKeyIDs">Collection of foreign key IDs that won't be deleted.</param>
        /// <param name="foreignKeyExpression">Expression to retrieve the property of the foreign key.</param>
        /// <param name="tablePart">Optional table part</param>
        public static void DeleteNotIn<TRepository, TEntity>(this TRepository repository,
                                                             IEnumerable<int> primaryKeyIDs, IEnumerable<int> foreignKeyIDs,
                                                             Expression<Func<TEntity, int>> foreignKeyExpression, string tablePart = null)
            where TRepository : IRepository<TEntity> where TEntity : BaseEntity {
            var deleteExpression = RepositoryHelper.GenerateDeleteExpression(primaryKeyIDs, foreignKeyIDs, foreignKeyExpression);
            if (deleteExpression == null) { return; }
            repository.DeleteBy(deleteExpression, tablePart);
        }
    }
}