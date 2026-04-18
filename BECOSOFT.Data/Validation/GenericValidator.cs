using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Utilities.Extensions;
using NLog;

namespace BECOSOFT.Data.Validation {
    /// <summary>
    /// Generic validator for a <see cref="BaseEntity"/> class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class GenericValidator<T> : Validator<T> where T : class, IValidatable {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(GenericValidator<T>).GetGenericName(false));

        internal GenericValidator(IPrimaryKeyRepository primaryKeyRepository) 
            : base(Logger, primaryKeyRepository) {
        }
    }

    /// <summary>
    /// Generic validator for 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TU"></typeparam>
    public sealed class GenericValidator<T, TU> : Validator<T, TU>
        where T : TranslateableEntity<TU>, IValidatable
        where TU : TranslationEntity, IValidatable {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(GenericValidator<T, TU>).GetGenericName(false));

        internal GenericValidator(IValidator<TU> translationValidator, IPrimaryKeyRepository primaryKeyRepository) 
            : base(translationValidator, Logger, primaryKeyRepository) {
        }
    }
}