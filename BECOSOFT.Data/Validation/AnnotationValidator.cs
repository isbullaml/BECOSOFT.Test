using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Validation.Attributes;
using BECOSOFT.Utilities.Collections;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace BECOSOFT.Data.Validation {
    internal static class AnnotationValidator<T> where T : class, IValidatable {
        internal static void Perform(T entity, IErrorList errors) {
            var validator = AnnotationValidator.Get(typeof(T));

            PerformValidation(entity, errors, validator, null);
        }
        internal static void Perform(T entity, IReadOnlyList<EntityPropertyInfo> propertyInfo, IErrorList errors) {
            var validator = AnnotationValidator.Get(typeof(T));

            var set = propertyInfo.ToSafeHashSet();
            PerformValidation(entity, errors, validator, set);
        }

        private static void PerformValidation(object entity, IErrorList errors, AnnotationTypeValidator validator, HashSet<EntityPropertyInfo> propertyInfoHashSet) {
            if (entity == null) { return; }
            foreach (var propertyToValidate in validator.PropertiesWithValidationAttributes) {
                if (propertyInfoHashSet != null && !propertyInfoHashSet.Contains(propertyToValidate.Key)) {
                    continue;
                }
                var getter = propertyToValidate.Key.Getter;
                var value = getter(entity);

                var propertyValidator = propertyToValidate.Value;
                if (propertyValidator.HasRequiredAttribute) {
                    if (!propertyValidator.RequiredAttribute.IsValid(value)) {
                        var error = propertyValidator.RequiredAttribute.FormatErrorMessage(propertyToValidate.Key.PropertyName);
                        errors.Add(propertyToValidate.Key.PropertyName, error);
                        continue;
                    }
                }
                if (propertyValidator.HasNotNullAttribute) {
                    if (!propertyValidator.NotNullAttribute.IsValid(value)) {
                        var error = propertyValidator.NotNullAttribute.FormatErrorMessage(propertyToValidate.Key.PropertyName);
                        errors.Add(propertyToValidate.Key.PropertyName, error);
                        continue;
                    }
                }
                if (propertyValidator.HasAnnotationTypeValidator) {
                    PerformValidation(value, errors, propertyValidator.AnnotationTypeValidator, propertyInfoHashSet);
                }
                foreach (var validationAttribute in propertyValidator.ValidationAttributes) {
                    if (validationAttribute.IsValid(value)) { continue; }
                    var error = validationAttribute.FormatErrorMessage(propertyToValidate.Key.PropertyName);
                    errors.Add(propertyToValidate.Key.PropertyName, error);
                }
            }
        }
    }

    internal static class AnnotationValidator {
        private static readonly ConcurrentDictionary<Type, AnnotationTypeValidator> AnnotationTypeValidators = new ConcurrentDictionary<Type, AnnotationTypeValidator>();

        internal static AnnotationTypeValidator Get(Type type) {
            return AnnotationTypeValidators.GetOrAdd(type, CreateAnnotationTypeValidator);
        }

        private static AnnotationTypeValidator CreateAnnotationTypeValidator(Type type) {
            var validator = new AnnotationTypeValidator(type);
            return validator;
        }
    }

    internal class AnnotationTypeValidator {
        internal EntityTypeInfo TypeInfo { get; }
        internal KeyValueList<EntityPropertyInfo, PropertyValidator> PropertiesWithValidationAttributes { get; }

        internal AnnotationTypeValidator(Type t) {
            TypeInfo = EntityConverter.GetEntityTypeInfo(t);
            PropertiesWithValidationAttributes = new KeyValueList<EntityPropertyInfo, PropertyValidator>();
            foreach (var propertyInfo in TypeInfo.ValidatableProperties) {
                var validationAttributes = propertyInfo.PropertyInfo.GetCustomAttributes<ValidationAttribute>().ToSafeList();
                if (propertyInfo.IsBaseChild) {
                    var validator = new AnnotationTypeValidator(propertyInfo.PropertyType);
                    if (validator.PropertiesWithValidationAttributes.Values.HasAny()) {
                        PropertiesWithValidationAttributes.Add(propertyInfo, new PropertyValidator(validator, validationAttributes));
                    }
                }
                if (validationAttributes.IsEmpty()) { continue; }
                PropertiesWithValidationAttributes.Add(propertyInfo, new PropertyValidator(validationAttributes));
            }
        }
    }

    internal class PropertyValidator {
        internal RequiredAttribute RequiredAttribute { get; }
        internal bool HasRequiredAttribute => RequiredAttribute != null;
        internal NotNullAttribute NotNullAttribute { get; }
        internal bool HasNotNullAttribute => NotNullAttribute != null;
        internal List<ValidationAttribute> ValidationAttributes { get; }
        internal AnnotationTypeValidator AnnotationTypeValidator { get; }
        internal bool HasAnnotationTypeValidator => AnnotationTypeValidator != null;

        public PropertyValidator(List<ValidationAttribute> validationAttributes) {
            var attr = validationAttributes.FirstOrDefault(a => a is RequiredAttribute);
            if (attr != null) {
                RequiredAttribute = (RequiredAttribute) attr;
            }
            var nnAttr = validationAttributes.FirstOrDefault(a => a is NotNullAttribute);
            if (nnAttr != null) {
                NotNullAttribute = (NotNullAttribute) nnAttr;
            }
            ValidationAttributes = validationAttributes.Where(a => !(a is RequiredAttribute) && !(a is NotNullAttribute)).ToSafeList();
        }

        public PropertyValidator(AnnotationTypeValidator annotationTypeValidator, List<ValidationAttribute> validationAttributes)
            : this(validationAttributes) {
            AnnotationTypeValidator = annotationTypeValidator;
        }
    }
}