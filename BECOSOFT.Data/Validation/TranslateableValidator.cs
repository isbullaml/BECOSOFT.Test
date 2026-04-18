using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Utilities.Comparers;
using BECOSOFT.Utilities.Extensions.Collections;
using NLog;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Data.Validation {
    public abstract class Validator<T, TTranslation> : Validator<T>
        where T : TranslateableEntity<TTranslation>
        where TTranslation : TranslationEntity {
        private readonly IValidator<TTranslation> _translationValidator;
        protected virtual bool AtLeastOneTranslationRequired => true;

        protected Validator(IValidator<TTranslation> translationValidator, ILogger logger,
                            IPrimaryKeyRepository primaryKeyRepository) : base(logger, primaryKeyRepository) {
            _translationValidator = translationValidator;
        }

        protected sealed override void ExtraValidation(IValidationContainer<T> container) {
            var entities = container.Entities;
            var errorList = container.ErrorList;
            var languageIDs = new HashSet<int>(0);
            if (TranslateableValidatorHelper.LanguagePrimaryKeyType != null) {
                var tempLangIDs = container.PrimaryKeyContainer.TryGetIDs(TranslateableValidatorHelper.LanguagePrimaryKeyType.Type);
                if (tempLangIDs != null) {
                    languageIDs = tempLangIDs;
                }
            }
            for (var i = 0; i < entities.Count; i++) {
                var entity = entities[i];
                var errors = errorList[i];
                if (entity.Translations.IsEmpty()) {
                    if (AtLeastOneTranslationRequired) {
                        errors.Add(nameof(entity.Translations), Resources.General_Translations_AtleastOneRequired);
                    }
                } else {
                    if (entity.Translations.GroupBy(t => t.LanguageID).Any(t => t.Count() > 1)) {
                        errors.Add(nameof(entity.Translations), Resources.General_Translations_DuplicateLanguage);
                    }
                    PerformTranslationValidation(entity, errors, container.PrimaryKeyContainer);
                    PerformTranslationLanguageValidation(entity, errors, languageIDs);
                }
            }
            PerformValidation(container);
        }

        private static void PerformTranslationLanguageValidation(T entity, IErrorList errors, HashSet<int> languageIDs) {
            if (languageIDs.IsEmpty()) { return; }
            if (entity.Translations.IsEmpty()) { return; }
            for (var i = 0; i < entity.Translations.Count; i++) {
                var translation = entity.Translations[i];
                if (languageIDs.Contains(translation.LanguageID)) { continue; }
                ISubErrorList errorList = null;
                foreach (var error in errors) {
                    if (error.Property.Equals(nameof(entity.Translations))) {
                        errorList = error.SubErrors;
                    }
                }
                if (errorList == null) {
                    errorList = new SubErrorList();
                    errors.Add(nameof(entity.Translations), Resources.General_Validation_OneOrMoreErrors, errorList);
                }
                var subErrorList = errorList.FirstOrDefault(errs => errs.Index == i)?.Errors;
                if (subErrorList == null) {
                    subErrorList = new ErrorList();
                    errorList.Add(i, subErrorList);
                }
                if (translation.LanguageID == 0) {
                    var err = Resources.General_Translations_Language_Missing;
                    subErrorList.Add(nameof(translation.LanguageID), err);
                } else {
                    var err = Resources.General_Translations_Language_NonExistent;
                    subErrorList.Add(nameof(translation.LanguageID), err);
                }
            }
        }

        public override void AddIDSetsToContainer(List<T> entities, IPrimaryKeyContainer primaryKeyContainer) {
            var translations = entities.SelectMany(e => e.Translations).ToSafeList();
            if (TranslateableValidatorHelper.LanguagePrimaryKeyType != null) {
                primaryKeyContainer.Add(TranslateableValidatorHelper.LanguagePrimaryKeyType, translations.Select(t => t.LanguageID));
            }
            _translationValidator.AddIDSetsToContainer(translations, primaryKeyContainer);
        }

        private void PerformTranslationValidation(T entity, IErrorList errors, IReadonlyPrimaryKeyContainer container) {
            var translationContainer = new ValidationContainer<TTranslation>(entity.Translations.ToSafeList(), container, null);
            var translationValidationResult = _translationValidator.Validate(translationContainer);
            if (translationValidationResult.IsValid()) {
                return;
            }
            var translationPerIndex = entity.Translations.Select((t, i) => (t, i))
                                            .ToDictionary(x => x.t, x => x.i, ObjectReferenceEqualityComparer.Instance);
            // todo: convert to validation errror / validation sub error
            var errorsToAdd = new Dictionary<int, ErrorList>();
            foreach (var invalidResult in translationValidationResult.InvalidResults) {
                var errs = new ErrorList();
                foreach (var errorResult in invalidResult.Errors) {
                    errs.Add(errorResult);
                }
                var indexOfTranslation = translationPerIndex.TryGetValueWithDefault(invalidResult.ValidatedEntity);
                errorsToAdd.Add(indexOfTranslation, errs);
            }
            foreach (var errorResult in errorsToAdd) {
                var translationErrors = new SubErrorList(errorResult.Value.Count) {
                    { errorResult.Key, errorResult.Value }
                };
                errors.Add(nameof(entity.Translations), Resources.General_Validation_OneOrMoreErrors, translationErrors);
            }
        }

        protected virtual void PerformValidation(IValidationContainer<T> container) {
        }

    }

    public static class TranslateableValidatorHelper {
        public static PrimaryKeyType LanguagePrimaryKeyType { get; set; }

    }
}