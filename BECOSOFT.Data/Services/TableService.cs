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
    public abstract class TableService<T, TDefining> : ReadonlyTableService<T, TDefining>, ITableService<T, TDefining>
        where T : TableConsumingEntity<TDefining>
        where TDefining : TableDefiningEntity {

        private readonly IRepository<T> _repository;
        protected readonly IValidator<T, TDefining> Validator;

        protected TableService(IRepository<T> repository, IRepository<TDefining> definingRepository, IValidator<T, TDefining> validator)
            : base(repository, definingRepository) {
            _repository = repository;
            Validator = validator;
        }

        // todo: niet meer virtual
        public ValidationResult<T> Save(TDefining definition, T entity) {
            var entities = new List<T> { entity };
            var result = Save(definition, entities);
            return result.Results.First();
        }

        public MultiValidationResult<T> Save(TDefining definition, IEnumerable<T> entities) {
            var tableSaveContainer = new SaveContainer<T, TDefining>(definition, entities.ToSafeList(), null, null);
            return Save(tableSaveContainer);
        }

        public virtual MultiValidationResult<T> Save(ISaveContainer<T, TDefining> saveContainer) {
            PreValidateDefinition(saveContainer.Definition);
            var entityList = saveContainer.Entities.ToSafeList();
            var validationContainer = saveContainer.ToValidationContainer();
            var result = Validator.Validate(validationContainer);
            if (!result.IsValid()) {
                return result;
            }
            var tableName = GetTableName(saveContainer.Definition);
            _repository.Save(entityList, tableName);
            return result;
        }

        public EntityDeleteResult Delete(TDefining definition, int id) {
            var ids = new List<int> { id };
            var entityDeleteResults = Delete(definition, ids);
            return entityDeleteResults.GetEntityResult(id);
        }

        public virtual DeleteResult Delete(TDefining definition, IEnumerable<int> ids) {
            PreValidateDefinition(definition);
            var idList = ids.ToSafeList();
            var tableName = GetTableName(definition);
            var deleteResult = _repository.CanDelete(idList, tableName);
            if (!deleteResult.AreAllDeleteable) {
                return deleteResult;
            }
            DeleteAction(definition, idList);
            return deleteResult;
        }

        public void DeleteAction(TDefining definition, int id) {
            var ids = new List<int> { id };
            DeleteAction(definition, ids);
        }

        public virtual void DeleteAction(TDefining definition, IEnumerable<int> ids) {
            PreValidateDefinition(definition);
            var tableName = GetTableName(definition);
            _repository.Delete(ids, tableName);
        }

        public ValidationResult<T> Validate(TDefining definition, T entity) {
            return Validator.Validate(definition, entity);
        }

        public MultiValidationResult<T> Validate(TDefining definition, IEnumerable<T> entities) {
            return Validator.Validate(definition, entities);
        }

        public bool IsValid(TDefining definition, T entity) {
            return Validate(definition, entity).IsValid();
        }

        public ValidationResult<T> UpdateProperty(TDefining definition, T entity, params Expression<Func<T, object>>[] properties) {
            PreValidateDefinition(definition);
            var result = UpdateProperty(definition, new List<T> { entity }, properties);
            return result.Results.FirstOrDefault();
        }

        public MultiValidationResult<T> UpdateProperty(TDefining definition, IEnumerable<T> entities, params Expression<Func<T, object>>[] properties) {
            PreValidateDefinition(definition);
            var entityList = entities.ToSafeList();
            var validation = Validator.ValidateProperties(definition, entityList, properties);
            if (!validation.IsValid()) {
                return validation;
            }
            var propertyInfoList = properties.Select(p => p.GetProperty()).ToList();
            if (propertyInfoList.Any(p => p == null)) {
                return validation;
            }
            _repository.UpdateProperty(entityList, propertyInfoList, definition.TableName);

            return validation;
        }
    }
    //public abstract class TableService<T, TDefining> : TableService<T, TDefining, SaveContainer<T, TDefining>>
    //    where T : TableConsumingEntity<TDefining>
    //    where TDefining : TableDefiningEntity {
    //    protected TableService(IRepository<T> repository,
    //                           IRepository<TDefining> definingRepository,
    //                           IValidator<T, TDefining> validator)
    //        : base(repository, definingRepository, validator) {
    //    }
    //}

}