using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Validation {
    public interface IValidator<T> : IBaseValidator where T : class, IValidatable {
        /// <summary>
        /// Validates a single entity
        /// </summary>
        /// <param name="entity">Entity to validate</param>
        /// <returns>A validation result for the given entity</returns>
        ValidationResult<T> Validate(T entity);

        /// <summary>
        /// Validates a single entity
        /// </summary>
        /// <param name="entity">Entity to validate</param>
        /// <param name="options"></param>
        /// <returns>A validation result for the given entity</returns>
        ValidationResult<T> Validate(T entity, ISaveOptions options);

        /// <summary>
        /// Validates each provided entity
        /// </summary>
        /// <param name="entities">Collection of entities</param>
        /// <returns>A collection of validation results for the given entities</returns>
        MultiValidationResult<T> Validate(IEnumerable<T> entities);

        /// <summary>
        /// Validates each provided entity
        /// </summary>
        /// <param name="entities">Collection of entities</param>
        /// <param name="options"></param>
        /// <returns>A collection of validation results for the given entities</returns>
        MultiValidationResult<T> Validate(IEnumerable<T> entities, ISaveOptions options);

        MultiValidationResult<T> Validate(IValidationContainer<T> container);

        MultiValidationResult<T> ValidateProperties(IEnumerable<T> entities, params Expression<Func<T, object>>[] properties);

        /// <summary>
        /// Overridable function <see cref="AddIDSetsToContainer"/> allows the implementing <see cref="Validator{T}"/> to add additional ID's to fetch in the <see cref="PrimaryKeyContainer"/>
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="primaryKeyContainer"></param>
        void AddIDSetsToContainer(List<T> entities, IPrimaryKeyContainer primaryKeyContainer);
    }

    public interface IValidator<T, in TDefining> : IBaseValidator where T : TableConsumingEntity<TDefining>, IValidatable where TDefining : TableDefiningEntity {
        /// <summary>
        /// Validates a single entity
        /// </summary>
        /// <param name="definition">Table part</param>
        /// <param name="entity">Entity to validate</param>
        /// <returns>A validation result for the given entity</returns>
        ValidationResult<T> Validate(TDefining definition, T entity);

        /// <summary>
        /// Validates a single entity
        /// </summary>
        /// <param name="definition">Table part</param>
        /// <param name="entity">Entity to validate</param>
        /// <param name="options"></param>
        /// <returns>A validation result for the given entity</returns>
        ValidationResult<T> Validate(TDefining definition, T entity, ISaveOptions options);

        /// <summary>
        /// Validates each provided entity
        /// </summary>
        /// <param name="definition">Table part</param>
        /// <param name="entities">Collection of entities</param>
        /// <returns>A collection of validation results for the given entities</returns>
        MultiValidationResult<T> Validate(TDefining definition, IEnumerable<T> entities);

        /// <summary>
        /// Validates each provided entity
        /// </summary>
        /// <param name="definition">Table part</param>
        /// <param name="entities">Collection of entities</param>
        /// <param name="options"></param>
        /// <returns>A collection of validation results for the given entities</returns>
        MultiValidationResult<T> Validate(TDefining definition, IEnumerable<T> entities, ISaveOptions options);

        MultiValidationResult<T> Validate(IValidationContainer<T, TDefining> container);

        MultiValidationResult<T> ValidateProperties(TDefining definition, IEnumerable<T> entities, params Expression<Func<T, object>>[] properties);

        /// <summary>
        /// Overridable function <see cref="AddIDSetsToContainer"/> allows the implementing <see cref="Validator{T}"/> to add additional ID's to fetch in the <see cref="PrimaryKeyContainer"/>
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="entities"></param>
        /// <param name="primaryKeyContainer"></param>
        void AddIDSetsToContainer(TDefining definition, List<T> entities, IPrimaryKeyContainer primaryKeyContainer);
    }
}