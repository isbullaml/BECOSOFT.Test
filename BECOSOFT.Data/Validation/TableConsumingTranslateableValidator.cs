using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Utilities.Comparers;
using BECOSOFT.Utilities.Extensions.Collections;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Validation {
    public abstract class TableConsumingTranslateableValidator<T, TTranslation, TDefining> : BaseValidator<T>, IValidator<T, TDefining>
        where T : TableConsumingTranslateableEntity<TTranslation, TDefining>, IValidatable 
        where TTranslation : TableConsumingTranslationEntity<TDefining> 
        where TDefining : TableDefiningEntity {

        private readonly IValidator<TTranslation, TDefining> _translationValidator;

        protected TableConsumingTranslateableValidator(IValidator<TTranslation, TDefining> translationValidator, 
                                                       ILogger logger, 
                                                       IPrimaryKeyRepository primaryKeyRepository)
            : base(logger, primaryKeyRepository) {
            _translationValidator = translationValidator;
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
            return ValidateProperties(definition, entities, null, properties);
        }

        public MultiValidationResult<T> ValidateProperties(TDefining definition, IEnumerable<T> entities, ISaveOptions options,
                                                           params Expression<Func<T, object>>[] properties) {
            return ValidateProperties<TDefining>(definition, entities, options, properties);
        }

        protected sealed override void ExtraValidation<TPlaceholderDefining>(IValidationContainer<T, TPlaceholderDefining> container) {
            var entities = container.Entities;
            var errorList = container.ErrorList;
            for (var i = 0; i < entities.Count; i++) {
                var entity = entities[i];
                var errors = errorList[i];
                if (!entity.Translations.HasAny()) {
                    errors.Add(nameof(entity.Translations), Resources.General_Translations_AtleastOneRequired);
                } else {
                    if (entity.Translations.GroupBy(t => t.LanguageID).Any(t => t.Count() > 1)) {
                        errors.Add(nameof(entity.Translations), Resources.General_Translations_DuplicateLanguage);
                    }
                    PerformTranslationValidation(container.Definition as TDefining, entity, errors, container.PrimaryKeyContainer);
                }
            }

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

        private void PerformTranslationValidation(TDefining definition, T entity, IErrorList errors, IReadonlyPrimaryKeyContainer container) {
            var validationContainer = new ValidationContainer<TTranslation, TDefining>(definition, entity.Translations.ToSafeList(), container, null);
            var translationValidationResult = _translationValidator.Validate(validationContainer);
            if (translationValidationResult.IsValid()) {
                return;
            }
            var translationPerIndex = entity.Translations.Select((t, i) => (t, i))
                                            .ToDictionary(x => x.t, x => x.i, ObjectReferenceEqualityComparer.Instance);
            // todo: convert to validation errror / validation sub error
            var errorsToAdd = new Dictionary<int, HashSet<ValidationError>>();
            foreach (var invalidResult in translationValidationResult.InvalidResults) {
                var errs = new HashSet<ValidationError>();
                foreach (var errorResult in invalidResult.Errors) {
                    errs.Add(errorResult);
                }
                var indexOfTranslation = translationPerIndex.TryGetValueWithDefault(invalidResult.ValidatedEntity);
                errorsToAdd.Add(indexOfTranslation, errs);
            }
            foreach (var errorResult in errorsToAdd) {
                foreach (var validationError in errorResult.Value) {
                    errors.Add($"{nameof(entity.Translations)}[{errorResult.Key}].{validationError.Property}", validationError.Error);
                }
            }
        }

        protected virtual void PerformValidation(IReadOnlyList<T> entities, IReadOnlyList<IErrorList> errors) {
        }
    }
}