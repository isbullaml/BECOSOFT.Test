using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Models.Base;

namespace BECOSOFT.Data.Repositories.Interfaces {
    public interface IRepository<T> : IReadonlyRepository<T> where T : BaseEntity {
        void Delete(T entity, string tablePart = null);
        void Save(T entity, string tablePart = null);
        void Save(IEnumerable<T> entities, string tablePart = null);
        void UpdateProperty(T entity, IEnumerable<EntityPropertyInfo> selectedProperties, string tablePart = null);
        void UpdateProperty(IEnumerable<T> entities, IEnumerable<EntityPropertyInfo> properties, string tablePart = null);
        void UpdateProperty<TProp>(int id, EntityPropertyInfo property, TProp value, string tablePart = null);
        void UpdateProperty<TProp>(IEnumerable<int> id, EntityPropertyInfo property, TProp value, string tablePart = null);
        /// <summary>
        /// Performs a delete by <see cref="id"/> (Primary key of the table defined by <see cref="T"/>).
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tablePart"></param>
        void Delete(int id, string tablePart = null);
        void Delete(IEnumerable<int> id, string tablePart = null);
        void DeleteBy(Expression<Func<T, bool>> where, string tablePart = null);
        DeleteResult CanDelete(int id, string tablePart = null);
        DeleteResult CanDelete(IEnumerable<int> entityIDs, string tablePart = null);
        //bool Exists(IKeyValueList<string, object> propertiesToCheck, string tablePart = null, Operator whereJoin = Operator.Or);
    }

    public interface IDeleteNotInRepository {
    }
}