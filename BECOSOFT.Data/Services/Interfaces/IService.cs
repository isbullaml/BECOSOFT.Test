using System.Collections.Generic;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Validation;
using System;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Services.Interfaces {
    public interface IService<T> : IReadonlyService<T> where T : BaseEntity {
        ValidationResult<T> Save(T entity);
        MultiValidationResult<T> Save(IEnumerable<T> entities);
        MultiValidationResult<T> Save(ISaveContainer<T> saveContainer);
        /// <summary>
        /// Delete an entity by <see cref="id"/> (Primary key of the table defined by <see cref="T"/>).
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        EntityDeleteResult Delete(int id);
        DeleteResult Delete(IEnumerable<int> ids);
        void DeleteAction(int id);
        ValidationResult<T> Validate(T entity);
        MultiValidationResult<T> Validate(IEnumerable<T> entities);
        bool IsValid(T entity);
        ValidationResult<T> UpdateProperty(T entity, params Expression<Func<T, object>>[] properties);
        MultiValidationResult<T> UpdateProperty(IEnumerable<T> entities, params Expression<Func<T, object>>[] properties);
    }
}
