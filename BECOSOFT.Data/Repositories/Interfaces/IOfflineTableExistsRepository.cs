using BECOSOFT.Data.Attributes;
using BECOSOFT.Data.Models.Base;
using System;

namespace BECOSOFT.Data.Repositories.Interfaces {
    public interface IOfflineTableExistsRepository : IBaseRepository {
        /// <summary>
        /// Check if the specified table (defined by the <see cref="TableAttribute"/> on <see cref="type"/> exists.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tablePart"></param>
        bool TableExists(Type type, string tablePart = null);
        /// <summary>
        /// Check if the specified table (defined by the <see cref="TableAttribute"/> on <see cref="T"/> exists.
        /// </summary>
        /// <param name="tablePart"></param>
        bool TableExists<T, TDefining>(string tablePart) where T : TableConsumingEntity<TDefining> where TDefining : TableDefiningEntity;
        /// <summary>
        /// Check if the specified table (defined by the <see cref="TableAttribute"/> on <see cref="T"/> exists.
        /// </summary>
        bool TableExists<T>() where T : BaseEntity;
    }
}