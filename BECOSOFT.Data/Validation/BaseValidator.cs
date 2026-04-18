using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Extensions;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Data.Validation.Attributes;
using BECOSOFT.Utilities.Annotations;
using BECOSOFT.Utilities.Collections;
using BECOSOFT.Utilities.Comparers;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Validation {
    [UsedImplicitly]
    [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public abstract class BaseValidator<T> : IBaseValidator where T : class, IValidatable {
        private readonly ILogger _logger;
        protected readonly IPrimaryKeyRepository PrimaryKeyRepository;
        private readonly bool _isBaseResultType;
        private readonly string _entityTypeName;

        protected BaseValidator(ILogger logger, IPrimaryKeyRepository primaryKeyRepository) {
            _logger = logger;
            PrimaryKeyRepository = primaryKeyRepository;
            var type = typeof(T);
            if (typeof(BaseResult).IsAssignableFrom(type)) {
                _isBaseResultType = true;
            }
            _entityTypeName = type.ToString();
        }

        protected ValidationResult<T> Validate<TDefining>(TDefining definition, T entity, ISaveOptions options) where TDefining : TableDefiningEntity {
            var entityList = new List<T> { entity };
            var result = Validate(definition, entityList, options);
            return result.Results.FirstOrDefault();
        }

        protected MultiValidationResult<T> Validate<TDefining>(TDefining definition, IEnumerable<T> entities, ISaveOptions options) where TDefining : TableDefiningEntity {
            var entityList = entities.ToSafeList();
            if (entityList.IsEmpty()) {
                return MultiValidationResult<T>.Empty();
            }
            var primaryKeyContainer = GetPrimaryKeyContainer(definition, entityList, true);
            var validationContainer = new ValidationContainer<T, TDefining>(definition, entityList, primaryKeyContainer, options);
            return Validate(validationContainer);
        }

        public MultiValidationResult<T> Validate<TDefining>(IValidationContainer<T, TDefining> container) where TDefining : TableDefiningEntity {
            Check.IsValidTableConsuming<T>(container.Definition?.TableName);
            var entities = container.Entities.ToSafeList();
            if (entities.IsEmpty()) {
                return MultiValidationResult<T>.Empty();
            }
            if (container.PrimaryKeyContainer == null) {
                var primaryKeyContainer = GetPrimaryKeyContainer(container.Definition, entities, true);
                container = new ValidationContainer<T, TDefining>(container.Definition, entities, primaryKeyContainer, container.Options);
            }
            for (var index = 0; index < entities.Count; index++) {
                var entity = entities[index];
                var errors = container.ErrorList[index];
                AnnotationValidator<T>.Perform(entity, errors);
            }
            ValidateLinkedEntityProperties(container);
            InternalExtraValidation(container);
            var result = GetMultiValidationResult(container, container.Definition?.TableName);
            return result;
        }

        public MultiValidationResult<T> ValidateProperties<TDefining>(TDefining definition, IEnumerable<T> entities, ISaveOptions options,
                                                                      params Expression<Func<T, object>>[] properties) 
            where TDefining : TableDefiningEntity {
            Check.IsValidTableConsuming<T>(definition?.TableName);
            var entityList = entities.ToSafeList();
            if (entityList.IsEmpty()) {
                return MultiValidationResult<T>.Empty();
            }
            var primaryKeyContainer = GetPrimaryKeyContainer(definition, entityList, false);
            var validationContainer = new ValidationContainer<T, TDefining>(definition, entityList, primaryKeyContainer, options);
            return ValidateProperties(validationContainer, properties);
        }

        private MultiValidationResult<T> ValidateProperties<TDefining>(ValidationContainer<T, TDefining> container,
                                                                       params Expression<Func<T, object>>[] properties) 
            where TDefining : TableDefiningEntity {

            var propertyInfoList = properties.Select(p => p.GetProperty()).Where(p => p != null).ToList();
            if (_logger.IsDebugEnabled) {
                var entityType = typeof(T);
                _logger.Debug("Validating properties '{0}' of type {1}", string.Join(", ", propertyInfoList.Select(p => p.PropertyName)), entityType.Name);
            }
            for (var index = 0; index < container.Entities.Count; index++) {
                var entity = container.Entities[index];
                var errors = container.ErrorList[index];
                AnnotationValidator<T>.Perform(entity, propertyInfoList, errors);
            }
            ValidateLinkedEntityProperties(container, propertyInfoList);
            var result = GetMultiValidationResult(container, container.Definition?.TableName);
            return result;
        }
        
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        private PrimaryKeyContainer GetPrimaryKeyContainer<TDefining>(TDefining definition, List<T> entities, bool traverseLinkedTree) where TDefining : TableDefiningEntity {
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            var tree = EntityConverter.GetLinkedEntitiesTree(entityTypeInfo, false);
            var primaryKeyContainer = new PrimaryKeyContainer();
            ProcessLinkedEntityValidationAttributes(definition, entities, entityTypeInfo, primaryKeyContainer);
            if (traverseLinkedTree) {
                foreach (var node in tree.Flatten()) {
                    if (node.Parent == null) { continue; }
                    if (!node.Value.EntityPropertyInfo.AreLinkedEntities && !node.Value.EntityPropertyInfo.IsLinkedEntity) { continue; }
                    var nodeTypeInfo = node.Value.EntityTypeInfo;
                    var getterList = new List<EntityPropertyInfo> { node.Value.EntityPropertyInfo };
                    var nodeToCheck = node.Parent;
                    while (nodeToCheck?.Parent != null) {
                        getterList.Add(nodeToCheck.Value.EntityPropertyInfo);
                        nodeToCheck = nodeToCheck.Parent;
                    }
                    List<object> objects = null;
                    for (var i = getterList.Count - 1; i >= 0; i--) {
                        var propertyInfo = getterList[i];
                        var getter = propertyInfo.Getter;
                        var isEnumerable = propertyInfo.AreLinkedEntities;
                        var b = i == getterList.Count - 1 ? entities.Cast<object>() : objects.Where(o => o != null).ToList();
                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                        if (isEnumerable) {
                            objects = b.SelectMany(e => ((IEnumerable)getter(e)).Cast<object>()).ToList();
                        } else {
                            objects = b.Select(e => getter(e)).ToList();
                        }
                    }

                    ProcessLinkedEntityValidationAttributes(definition, objects, nodeTypeInfo, primaryKeyContainer);
                }
            }
            InternalAddIDSetsToContainer(definition, entities, primaryKeyContainer);
            if (PrimaryKeyRepository == null || primaryKeyContainer.IsEmpty()) {
                return primaryKeyContainer;
            }
            return PrimaryKeyRepository.GetIDs(primaryKeyContainer);
        }

        private static void ProcessLinkedEntityValidationAttributes<TDefining>(TDefining definition, IEnumerable entities, 
                                                                               EntityTypeInfo entityTypeInfo, PrimaryKeyContainer primaryKeyContainer)
            where TDefining : TableDefiningEntity {
          
            var properties = entityTypeInfo.Properties.Where(p => p.ValidationAttribute is LinkedEntityValidationAttribute).ToList();
            if (properties.IsEmpty()) { return; }

            var primaryKeyInfo = entityTypeInfo.PrimaryKeyInfo;
            var pkGetter = primaryKeyInfo?.Getter;

            var propertyDict = new Dictionary<Type, HashSet<int>>();
            foreach (var entity in entities) {
                var pkValue = pkGetter?.Invoke(entity).To<int>();
                foreach (var propertyInfo in properties) {
                    if (pkValue.HasValue) {
                        if (pkValue.Value != 0 && !propertyInfo.Updateable) {
                            continue;
                        }
                    }
                    var attr = (LinkedEntityValidationAttribute)propertyInfo.ValidationAttribute;
                    var propID = propertyInfo.Getter(entity).To<int>();
                    if (propID <= 0) { continue; }
                    HashSet<int> set;
                    if (!propertyDict.TryGetValue(attr.LinkedEntityType, out set)) {
                        set = new HashSet<int>();
                        propertyDict.Add(attr.LinkedEntityType, set);
                    }
                    set.Add(propID);
                    var propTypeInfo = EntityConverter.GetEntityTypeInfo(attr.LinkedEntityType);
                    primaryKeyContainer.Add(attr.LinkedEntityType, set, propTypeInfo.IsTableConsuming ? definition?.TableName : null);
                }
            }
        }

        protected MultiValidationResult<T> GetMultiValidationResult(IValidationContainer<T> container, string tablePart) {
            var hasTablePart = !tablePart.IsNullOrWhiteSpace();
            var entities = container.Entities;
            var errorList = container.ErrorList;
            var result = new List<ValidationResult<T>>(entities.Count);
            if (_logger.IsDebugEnabled && entities.HasAny()) {
                var entity = entities[0];
                var entityDecription = _isBaseResultType ? (entity as BaseResult).ToValidationLogString() : entity?.GetType().Name ?? "null";
                if (hasTablePart) {
                    _logger.Debug("Validation of {3} entities {0} for tablePart '{2}' with type: {1}", entityDecription, _entityTypeName, tablePart, entities.Count);
                } else {
                    _logger.Debug("Validation of {2} entities {0} with type: {1}", entityDecription, entity, entities.Count);
                }
            }
            for (var index = 0; index < entities.Count; index++) {
                var entity = entities[index];
                var errors = errorList[index];
                result.Add(new ValidationResult<T>(entity, errors));
                if (_logger.IsDebugEnabled && errors.HasAny()) {
                    var entityDecription = _isBaseResultType ? (entity as BaseResult).ToValidationLogString() : entity?.ToString() ?? "null";
                    if (hasTablePart) {
                        _logger.Debug("Validation of entity {0} for tablePart '{2}' with type: {1}", entityDecription, _entityTypeName, tablePart);
                    } else {
                        _logger.Debug("Validation of entity {0} with type: {1}", entityDecription, entity);
                    }
                    _logger.Debug("Validation finished with {0} errors for entity at index {1}:", errors.Count, index);

                    foreach (var error in errors) {
                        _logger.Debug("errorKey: {0}, value: {1}", error.Property ?? "null", error.Error ?? "null");
                    }
                } else if (_logger.IsTraceEnabled) {
                    _logger.Trace("Validation finished with {0} errors for entity at index {1}:", errors.Count, index);
                }
            }
            return new MultiValidationResult<T>(result, container.PrimaryKeyContainer);
        }

        private void ValidateLinkedEntityProperties<TDefining>(IValidationContainer<T, TDefining> container,
                                                               List<EntityPropertyInfo> propertiesToUpdate = null) where TDefining : TableDefiningEntity {
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            var shouldValidateUpdateableProperties = propertiesToUpdate.HasAny();
            var properties = (shouldValidateUpdateableProperties ? propertiesToUpdate : entityTypeInfo.Properties)
                             .Where(p => p.ValidationAttribute is LinkedEntityValidationAttribute).ToList();

            if (properties.IsEmpty()) { return; }
            var primaryKeyInfo = entityTypeInfo.PrimaryKeyInfo;
            var pkGetter = primaryKeyInfo?.Getter;

            var propertyDict = new Dictionary<Type, HashSet<int>>();
            var linkedEntityContainerResult = container.PrimaryKeyContainer;
            foreach (var property in properties) {
                var attr = (LinkedEntityValidationAttribute)property.ValidationAttribute;
                var propTypeInfo = EntityConverter.GetEntityTypeInfo(attr.LinkedEntityType);
                var existingIDs = linkedEntityContainerResult.TryGetIDs(attr.LinkedEntityType, propTypeInfo.IsTableConsuming ? container.Definition?.TableName : null);
                propertyDict[attr.LinkedEntityType] = existingIDs;
            }

            for (var index = 0; index < container.Entities.Count; index++) {
                var entity = container.Entities[index];
                var pkValue = pkGetter?.Invoke(entity).To<int>();
                var errorlist = container.ErrorList[index];
                foreach (var property in properties) {
                    if (pkValue.HasValue) {
                        if (pkValue.Value != 0 && !property.Updateable) {
                            var validationAttr = (LinkedEntityValidationAttribute)property.ValidationAttribute;
                            if (!validationAttr.SkipOnUpdate && shouldValidateUpdateableProperties) {
                                errorlist.Add(property.PropertyName, Resources.General_Validation_NonUpdateableProperty);
                            }
                            continue;
                        }
                    }
                    var attr = (LinkedEntityValidationAttribute)property.ValidationAttribute;
                    var propID = property.Getter(entity).To<int>();
                    if (propID == 0) {
                        if (!attr.AllowZeroID) {
                            var linkedPropName = attr.GetPropertyResource() ?? attr.LinkedEntityType.Name;
                            errorlist.Add(property.PropertyName, Resources.General_Validation_LinkedEntity_Missing
                                                                          .FormatWith(entityTypeInfo.EntityType.Name, linkedPropName));
                        }
                    } else {
                        var foundIDs = propertyDict.TryGetValueWithDefault(attr.LinkedEntityType);
                        if (!foundIDs.Contains(propID)) {
                            var linkedPropName = attr.GetPropertyResource() ?? attr.LinkedEntityType.Name;
                            errorlist.Add(property.PropertyName, Resources.General_Validation_LinkedEntity_NonExistent
                                                                          .FormatWith(entityTypeInfo.EntityType.Name, linkedPropName));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Allows the overriding class to specify how the validation for multiple entities needs to be implemented.
        /// If it is not implemented, then the single entity ExtraValidation gets called for each entity
        /// </summary>
        /// <param name="container">Contains the entities to validate and the primary key container</param>
        protected void InternalExtraValidation<TDefining>(IValidationContainer<T, TDefining> container) where TDefining : TableDefiningEntity {
            if (Check<T>.IsTableConsuming) {
                ExtraValidation(container);
            } else {
                ExtraValidation(container as IValidationContainer<T>);
            }
        }

        /// <summary>
        /// Allows the overriding class to specify how the validation for multiple entities needs to be implemented.
        /// If it is not implemented, then the single entity ExtraValidation gets called for each entity
        /// </summary>
        /// <param name="container">Contains the entities to validate and the primary key container</param>
        protected virtual void ExtraValidation<TDefining>(IValidationContainer<T, TDefining> container) where TDefining : TableDefiningEntity {
        }

        /// <summary>
        /// Allows the overriding class to specify how the validation for multiple entities needs to be implemented.
        /// If it is not implemented, then the single entity ExtraValidation gets called for each entity
        /// </summary>
        /// <param name="container">Contains the entities to validate and the primary key container</param>
        protected virtual void ExtraValidation(IValidationContainer<T> container) {
        }

        /// <summary>
        /// Add additional ID's to fetch in the <see cref="PrimaryKeyContainer"/>
        /// </summary>
        protected void InternalAddIDSetsToContainer<TDefining>(TDefining definition, List<T> entities, IPrimaryKeyContainer primaryKeyContainer) where TDefining : TableDefiningEntity {
            if (Check<T>.IsTableConsuming) {
                AddIDSetsToContainer(definition, entities, primaryKeyContainer);
            } else {
                AddIDSetsToContainer(entities, primaryKeyContainer);
            }
        }

        /// <summary>
        /// Overridable function <see cref="AddIDSetsToContainer{TDefining}"/> allows the implementing <see cref="Validator{T}"/> to add additional ID's to fetch in the <see cref="PrimaryKeyContainer"/>
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="entities"></param>
        /// <param name="primaryKeyContainer"></param>
        public virtual void AddIDSetsToContainer<TDefining>(TDefining definition, List<T> entities, IPrimaryKeyContainer primaryKeyContainer) where TDefining : TableDefiningEntity {
            
        }

        /// <summary>
        /// Overridable function <see cref="AddIDSetsToContainer"/> allows the implementing <see cref="Validator{T}"/> to add additional ID's to fetch in the <see cref="PrimaryKeyContainer"/>
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="primaryKeyContainer"></param>
        public virtual void AddIDSetsToContainer(List<T> entities, IPrimaryKeyContainer primaryKeyContainer) {
            
        }

        /// <summary>
        /// Validates an <see cref="int"/> property. 
        /// If the property value is zero then the result is Missing, if the property value does not exist then the result is NonExistent.
        /// </summary>
        /// <param name="property">Property value to check</param>
        /// <param name="existsFunc">Function to perform is the value has a value</param>
        /// <returns>a <see cref="PropertyValidationResult"/></returns>
        internal PropertyValidationResult ValidateIntProperty(int property, Func<int, bool> existsFunc) {
            return property == 0 ? PropertyValidationResult.Missing : ValidateIntPropertyExists(property, existsFunc);
        }

        internal PropertyValidationResult ValidateIntPropertyExists(int property, Func<int, bool> existsFunc) {
            return !existsFunc(property) ? PropertyValidationResult.NonExistent : PropertyValidationResult.Valid;
        }

        /// <summary>
        /// Adds the provided error message to the error list based on the provided <see cref="PropertyValidationResult"/>.
        /// </summary>
        /// <param name="result">the validation result</param>
        /// <param name="errors">error list of the current owning entity of the property</param>
        /// <param name="name">name of the property</param>
        /// <param name="missing">error message when the value is missing</param>
        /// <param name="nonExistent">error message when the value is non existent</param>
        internal void AddValidationResult(PropertyValidationResult result,
                                          IErrorList errors,
                                          string name, string missing,
                                          string nonExistent) {
            switch (result) {
                case PropertyValidationResult.Missing:
                    if (missing.IsNullOrEmpty()) { return; }
                    errors.Add(name, missing);
                    break;
                case PropertyValidationResult.NonExistent:
                    if (nonExistent.IsNullOrEmpty()) { return; }
                    errors.Add(name, nonExistent);
                    break;
            }
        }

        protected static bool HasSingleNamedError<TEntity>(BaseEntity entity, string errorPropertyName,
                                                           ValidationResult<TEntity> validationResult) where TEntity : class {
            return entity.Id == 0 && validationResult.ContainsProperty(errorPropertyName) &&
                   validationResult.Errors.Count == 1;
        }

        protected static bool HasSingleNamedError<TEntity>(string errorPropertyName,
                                                           ValidationResult<TEntity> validationResult) where TEntity : BaseEntity {
            var entity = validationResult.ValidatedEntity;
            return entity.Id == 0 && validationResult.ContainsProperty(errorPropertyName) &&
                   validationResult.Errors.Count == 1;
        }

        /// <summary>
        /// Creates a <see cref="ValidationEntityLookupDictionary{TKey}"/> for a LinkedEntities-property based on the <see cref="selector"/>.
        /// This allows getting the index of the main entity <see cref="T"/> based on it's LinkedEntities <see cref="TLinked"/>.
        /// </summary>
        /// <typeparam name="TLinked">The type of the LinkedEntities</typeparam>
        /// <param name="entities">The list of main entities</param>
        /// <param name="selector">The selector to get the <see cref="ObserverList{TLinked}"/> on each entity</param>
        internal ValidationEntityLookupDictionary<TLinked> LinkedEntityToIndexedKeyValueList<TLinked>(IReadOnlyList<T> entities, Func<T, TLinked> selector) where TLinked : class {
            var indexLookup = new ValidationEntityLookupDictionary<TLinked>();
            for (var i = 0; i < entities.Count; i++) {
                var entity = entities[i];
                var linkedEntity = selector(entity);
                indexLookup.Add(linkedEntity, i, 0);
            }

            return indexLookup;
        }

        /// <summary>
        /// Creates a <see cref="ValidationEntityLookupDictionary{TKey}"/> for a LinkedEntities-property based on the <see cref="selector"/>.
        /// This allows getting the index of the main entity <see cref="T"/> based on it's LinkedEntities <see cref="TLinked"/>.
        /// </summary>
        /// <typeparam name="TLinked">The type of the LinkedEntities</typeparam>
        /// <param name="entities">The list of main entities</param>
        /// <param name="selector">The selector to get the <see cref="ObserverList{T}"/> on each entity</param>
        internal ValidationEntityLookupDictionary<TLinked> LinkedEntitiesToIndexedKeyValueList<TLinked>(IReadOnlyList<T> entities, Func<T, ObserverList<TLinked>> selector) 
            where TLinked : class {
            var indexLookup = new ValidationEntityLookupDictionary<TLinked>();
            for (var indexOfMainEntity = 0; indexOfMainEntity < entities.Count; indexOfMainEntity++) {
                var entity = entities[indexOfMainEntity];
                var list = selector(entity);
                for (var indexWithinLinkedEntities = 0; indexWithinLinkedEntities < list.Count; indexWithinLinkedEntities++) {
                    var linkedEntity = list[indexWithinLinkedEntities];
                    indexLookup.Add(linkedEntity, indexOfMainEntity, indexWithinLinkedEntities);
                }
            }

            return indexLookup;
        }

        internal void ProcessValidationResult<TLinked>(ValidationEntityLookupDictionary<TLinked> entityIndexLookup,
                                                       MultiValidationResult<TLinked> validationResult,
                                                       IReadOnlyList<IErrorList> mainEntityErrors,
                                                       string genericError,
                                                       bool fromLinkedEntities,
                                                       Func<ValidationResult<TLinked>, int, bool> skipWhen = null) where TLinked : class {
            if (validationResult.IsValid()) { return; }

            var mainValidationErrors = new Dictionary<int, ValidationError>();
            var resultsToCheck = new KeyValueList<ValidationResult<TLinked>, (int IndexOfMainEntity, int IndexWithinLinkedEntity)>();
            foreach (var invalidResult in validationResult.InvalidResults) {
                var indices = entityIndexLookup.GetIndices(invalidResult.ValidatedEntity);
                if (skipWhen != null && skipWhen(invalidResult, indices.IndexOfMainEntity)) {
                    continue;
                }
                resultsToCheck.Add(invalidResult, indices);
                var errorList = mainEntityErrors[indices.IndexOfMainEntity];
                var validationError = errorList.Add(string.Empty, genericError);
                mainValidationErrors[indices.IndexOfMainEntity] = validationError;
            }
            if (resultsToCheck.IsEmpty()) { return; }
            foreach (var resultToCheck in resultsToCheck) {
                var invalidResult = resultToCheck.Key;
                var indices = resultToCheck.Value;
                var validationError = mainValidationErrors[indices.IndexOfMainEntity];
                var linkedEntityIndex = fromLinkedEntities ? indices.IndexWithinLinkedEntity : -1;
                validationError.SubErrors.Add(linkedEntityIndex, invalidResult.Errors);
            }
        }

        internal void ValidateLinkedEntity<TLinked>(IValidationContainer<T> container,
                                                    Func<T, TLinked> selector,
                                                    Func<IValidationContainer<TLinked>, MultiValidationResult<TLinked>> validationFunc,
                                                    string genericError,
                                                    Func<ValidationResult<TLinked>, int, bool> skipWhen = null) 
            where TLinked : class {
            var indexLookup = LinkedEntityToIndexedKeyValueList(container.Entities, selector);
            if (indexLookup.IsEmpty) {
                return;
            }
            var linkedEntities = indexLookup.GetEntities();
            var subOptions = container.Options?.GetSubOptions(typeof(TLinked));
            var validationContainer = container.ToEntitySubContainer(linkedEntities, subOptions);
            var result = validationFunc(validationContainer);
            ProcessValidationResult(indexLookup, result, container.ErrorList, genericError, false, skipWhen);
        }

        internal void ValidateLinkedEntities<TLinked>(IValidationContainer<T> container, 
                                                      Func<T, ObserverList<TLinked>> selector,
                                                      Func<IValidationContainer<TLinked>, MultiValidationResult<TLinked>> validationFunc,
                                                      string genericError,
                                                      Func<ValidationResult<TLinked>, int, bool> skipWhen = null) 
            where TLinked : class {                   
            var indexLookup = LinkedEntitiesToIndexedKeyValueList(container.Entities, selector);
            if (indexLookup.IsEmpty) {
                return;
            }
            var linkedEntities = indexLookup.GetEntities();
            var subOptions = container.Options?.GetSubOptions(typeof(TLinked));
            var validationContainer = container.ToEntitySubContainer(linkedEntities, subOptions);
            var result = validationFunc(validationContainer);
            ProcessValidationResult(indexLookup, result, container.ErrorList, genericError, true, skipWhen);
        }
    }

    public class ValidationEntityLookupDictionary<TKey> where TKey : class {
        private readonly Dictionary<TKey, int> _indexOfMainEntity = new Dictionary<TKey, int>(ReferenceEqualityComparer<TKey>.Instance);
        private readonly Dictionary<TKey, int> _indexWithinLinkedEntities = new Dictionary<TKey, int>(ReferenceEqualityComparer<TKey>.Instance);
        private readonly List<TKey> _entities = new List<TKey>();

        public void Add(TKey key, int indexOfMainEntity, int indexWithinLinkedEntity) {
            _indexOfMainEntity[key] = indexOfMainEntity;
            _indexWithinLinkedEntities[key] = indexWithinLinkedEntity;
            _entities.Add(key);
        }

        public (int IndexOfMainEntity, int IndexWithinLinkedEntity) GetIndices(TKey key) {
            var indexOfMainEntity = _indexOfMainEntity.TryGetValueWithDefault(key, -1);
            var indexWithinLinkedEntities = _indexWithinLinkedEntities.TryGetValueWithDefault(key, -1);
            return (indexOfMainEntity, indexWithinLinkedEntities);
        }

        public IReadOnlyList<TKey> GetEntities() {
            return _entities;
        }

        public bool IsEmpty => _indexOfMainEntity.IsEmpty();
    }
}