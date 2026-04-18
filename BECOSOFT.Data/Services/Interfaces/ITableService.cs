using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Validation;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Services.Interfaces {
    public interface ITableService<T, TDefining> : IReadonlyTableService<T, TDefining>
        where T : TableConsumingEntity<TDefining>
        where TDefining : TableDefiningEntity {
        ValidationResult<T> Save(TDefining definition, T entity);
        MultiValidationResult<T> Save(TDefining definition, IEnumerable<T> entities);
        MultiValidationResult<T> Save(ISaveContainer<T, TDefining> saveContainer);
        EntityDeleteResult Delete(TDefining definition, int id);
        DeleteResult Delete(TDefining definition, IEnumerable<int> ids);
        void DeleteAction(TDefining definition, int id);
        void DeleteAction(TDefining definition, IEnumerable<int> ids);
        ValidationResult<T> Validate(TDefining definition, T entity);
        MultiValidationResult<T> Validate(TDefining definition, IEnumerable<T> entities);
        bool IsValid(TDefining definition, T entity);
        ValidationResult<T> UpdateProperty(TDefining definition, T entity, params Expression<Func<T, object>>[] properties);
        MultiValidationResult<T> UpdateProperty(TDefining definition, IEnumerable<T> entities, params Expression<Func<T, object>>[] properties);
    }
    //public interface ITableService<T, TDefining> : ITableService<T, TDefining, ISaveContainer<T, TDefining>>
    //    where T : TableConsumingEntity<TDefining>
    //    where TDefining : TableDefiningEntity {
    //}
}
