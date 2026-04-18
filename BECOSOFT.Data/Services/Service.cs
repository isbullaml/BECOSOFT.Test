using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Extensions;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.Data.Validation;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Services {
    public abstract class Service<T> : ReadonlyService<T>, IService<T> where T : BaseEntity {
        private readonly IRepository<T> _repository;
        protected readonly IValidator<T> Validator;

        protected Service(IRepository<T> repository, IValidator<T> validator) : base(repository) {
            _repository = repository;
            Validator = validator;
        }

        public ValidationResult<T> Save(T entity) {
            Check.IsNotTableConsuming<T>();
            var entities = new List<T> { entity };
            var result = Save(entities);
            return result.Results.First();
        }

        public MultiValidationResult<T> Save(IEnumerable<T> entities) {
            var saveContainer = new SaveContainer<T>(entities.ToSafeList(), null, null);
            return Save(saveContainer);
        }

        public virtual MultiValidationResult<T> Save(ISaveContainer<T> saveContainer) {
            Check.IsNotTableConsuming<T>();
            var entityList = saveContainer.Entities.ToSafeList();
            var validationContainer = saveContainer.ToEntityValidationContainer();
            var result = Validator.Validate(validationContainer);
            if (!result.IsValid()) {
                return result;
            }
            _repository.Save(entityList);
            return result;
        }

        public EntityDeleteResult Delete(int id) {
            var idList = new List<int> { id };
            var deleteResult = Delete(idList);
            return deleteResult.GetEntityResult(id);
        }

        public virtual DeleteResult Delete(IEnumerable<int> ids) {
            Check.IsNotTableConsuming<T>();
            var idList = ids.ToSafeList();
            var deleteResult = _repository.CanDelete(idList);
            if (!deleteResult.AreAllDeleteable) {
                return deleteResult;
            }
            DeleteAction(idList);
            return deleteResult;
        }

        public void DeleteAction(int id) {
            var ids = new List<int> { id };
            DeleteAction(ids);
        }

        public virtual void DeleteAction(IEnumerable<int> ids) {
            Check.IsNotTableConsuming<T>();
            _repository.Delete(ids);
        }

        public ValidationResult<T> Validate(T entity) {
            Check.IsNotTableConsuming<T>();
            return Validator.Validate(entity);
        }

        public MultiValidationResult<T> Validate(IEnumerable<T> entities) {
            Check.IsNotTableConsuming<T>();
            return Validator.Validate(entities);
        }

        public bool IsValid(T entity) {
            Check.IsNotTableConsuming<T>();
            return Validate(entity).IsValid();
        }

        public ValidationResult<T> UpdateProperty(T entity, params Expression<Func<T, object>>[] properties) {
            var result = UpdateProperty(new List<T> { entity }, properties);
            return result.Results.FirstOrDefault();
        }

        public MultiValidationResult<T> UpdateProperty(IEnumerable<T> entities, params Expression<Func<T, object>>[] properties) {
            var entityList = entities.ToSafeList();
            var validation = Validator.ValidateProperties(entityList, properties);
            if (!validation.IsValid()) {
                return validation;
            }
            var propertyInfoList = properties.Select(p => p.GetProperty()).ToList();
            if (propertyInfoList.Any(p => p == null)) {
                return validation;
            }
            _repository.UpdateProperty(entityList, propertyInfoList);

            return validation;
        }
    }
}