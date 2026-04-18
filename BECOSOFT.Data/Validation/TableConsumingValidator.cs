using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Validation {
    public abstract class TableConsumingValidator<T, TDefining> : BaseValidator<T>, IValidator<T, TDefining>
        where T : TableConsumingEntity<TDefining>, IValidatable
        where TDefining : TableDefiningEntity {

        protected TableConsumingValidator(ILogger logger, IPrimaryKeyRepository primaryKeyRepository)
            : base(logger, primaryKeyRepository) {
        }

        public ValidationResult<T> Validate(TDefining definition, T entity) {
            return Validate(definition, entity, null);
        }

        public ValidationResult<T> Validate(TDefining definition, T entity, ISaveOptions options) {
            return Validate<TDefining>(definition, entity, options);
        }

        public MultiValidationResult<T> Validate(TDefining definition, IEnumerable<T> entities) {
            return Validate(definition, entities, null);
        }

        public MultiValidationResult<T> Validate(TDefining definition, IEnumerable<T> entities, ISaveOptions options) {
            return Validate<TDefining>(definition, entities, options);
        }

        public MultiValidationResult<T> Validate(IValidationContainer<T, TDefining> container) {
            return Validate<TDefining>(container);
        }

        public MultiValidationResult<T> ValidateProperties(TDefining definition, IEnumerable<T> entities,
                                                           params Expression<Func<T, object>>[] properties) {
            return ValidateProperties<TDefining>(definition, entities, null, properties);
        }

        protected sealed override void ExtraValidation<TPlaceholderDefining>(IValidationContainer<T, TPlaceholderDefining> container) {
            // generic juggling
            ExtraValidation(container as IValidationContainer<T, TDefining>);
        }

        protected virtual void ExtraValidation(IValidationContainer<T, TDefining> container) {
        }

        protected sealed override void ExtraValidation(IValidationContainer<T> container) {
            throw new NotImplementedException();
        }


        public sealed override void AddIDSetsToContainer<TPlaceholderDefining>(TPlaceholderDefining definition, List<T> entities, IPrimaryKeyContainer primaryKeyContainer) {
            // generic juggling
            AddIDSetsToContainer(definition as TDefining, entities, primaryKeyContainer);
        }

        public virtual void AddIDSetsToContainer(TDefining definition, List<T> entities, IPrimaryKeyContainer primaryKeyContainer) {
        }

        public sealed override void AddIDSetsToContainer(List<T> entities, IPrimaryKeyContainer primaryKeyContainer) {
            throw new NotImplementedException();
        }

        internal void ValidateLinkedEntity<TLinked>(IValidationContainer<T, TDefining> container,
                                                    Func<T, TLinked> selector,
                                                    Func<IValidationContainer<TLinked, TDefining>, MultiValidationResult<TLinked>> validationFunc,
                                                    string genericError,
                                                    Func<ValidationResult<TLinked>, int, bool> skipWhen = null)
            where TLinked : class {
            var indexLookup = LinkedEntityToIndexedKeyValueList(container.Entities, selector);
            if (indexLookup.IsEmpty) {
                return;
            }
            var linkedEntities = indexLookup.GetEntities();
            var subOptions = container.Options?.GetSubOptions(typeof(TLinked));
            var validationContainer = container.ToSubContainer(linkedEntities, subOptions);
            var result = validationFunc(validationContainer);
            ProcessValidationResult(indexLookup, result, container.ErrorList, genericError, false, skipWhen);
        }

        internal void ValidateLinkedEntities<TLinked>(IValidationContainer<T, TDefining> container,
                                                      Func<T, ObserverList<TLinked>> selector,
                                                      Func<IValidationContainer<TLinked, TDefining>, MultiValidationResult<TLinked>> validationFunc,
                                                      string genericError,
                                                      Func<ValidationResult<TLinked>, int, bool> skipWhen = null)
            where TLinked : class {
            var indexLookup = LinkedEntitiesToIndexedKeyValueList(container.Entities, selector);
            if (indexLookup.IsEmpty) {
                return;
            }
            var linkedEntities = indexLookup.GetEntities();
            var subOptions = container.Options?.GetSubOptions(typeof(TLinked));
            var validationContainer = container.ToSubContainer(linkedEntities, subOptions);
            var result = validationFunc(validationContainer);
            ProcessValidationResult(indexLookup, result, container.ErrorList, genericError, true, skipWhen);
        }
    }
}