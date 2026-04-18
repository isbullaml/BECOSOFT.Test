using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Utilities.Extensions;
using NLog;

namespace BECOSOFT.Data.Validation {
    public sealed class GenericTableConsumingValidator<T, TDefining> : TableConsumingValidator<T, TDefining>
        where T : TableConsumingEntity<TDefining>, IValidatable
        where TDefining : TableDefiningEntity {

        private static readonly ILogger Logger = LogManager.GetLogger(typeof(GenericTableConsumingValidator<T, TDefining>).GetGenericName(false));

        internal GenericTableConsumingValidator(IPrimaryKeyRepository primaryKeyRepository) : base(Logger, primaryKeyRepository) {
        }
    }
}