using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Validation {
    public abstract class Validator<T> : BaseValidator<T>, IValidator<T> where T : class, IValidatable {
        protected Validator(ILogger logger, IPrimaryKeyRepository primaryKeyRepository)
            : base(logger, primaryKeyRepository) {
        }

        public ValidationResult<T> Validate(T entity) {
            return Validate(entity, null);
        }

        public ValidationResult<T> Validate(T entity, ISaveOptions options) {
            return Validate<TableDefiningEntity>(null, entity, options);
        }

        public MultiValidationResult<T> Validate(IEnumerable<T> entities) {
            return Validate(entities, null);
        }

        public MultiValidationResult<T> Validate(IEnumerable<T> entities, ISaveOptions options) {
            return Validate<TableDefiningEntity>(null, entities, options);
        }

        public MultiValidationResult<T> Validate(IValidationContainer<T> container) {
            return Validate<TableDefiningEntity>(container.AsTableDefining());
        }

        public MultiValidationResult<T> ValidateProperties(IEnumerable<T> entities, 
                                                           params Expression<Func<T, object>>[] properties) {
            return ValidateProperties<TableDefiningEntity>(null, entities, null, properties);
        }

        protected sealed override void ExtraValidation<TDefining>(IValidationContainer<T, TDefining> container) {
            throw new NotImplementedException();
        }

        public sealed override void AddIDSetsToContainer<TDefining>(TDefining definition, List<T> entities, IPrimaryKeyContainer primaryKeyContainer) {
            throw new NotImplementedException();
        }
    }
}