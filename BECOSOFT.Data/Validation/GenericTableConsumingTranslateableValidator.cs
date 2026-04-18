using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Utilities.Extensions;
using NLog;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Data.Validation {
    public sealed class GenericTableConsumingTranslateableValidator<T, TU, TDefining> : TableConsumingTranslateableValidator<T, TU, TDefining>
        where T : TableConsumingTranslateableEntity<TU, TDefining>, IValidatable 
        where TU : TableConsumingTranslationEntity<TDefining> 
        where TDefining : TableDefiningEntity {
        private readonly IValidator<TU, TDefining> _translationValidator;

        private static readonly ILogger Logger = LogManager.GetLogger(typeof(GenericTableConsumingTranslateableValidator<T, TU, TDefining>).GetGenericName(false));

        internal GenericTableConsumingTranslateableValidator(IValidator<TU, TDefining> translationValidator,
                                                             IPrimaryKeyRepository primaryKeyRepository) 
            : base(translationValidator, Logger, primaryKeyRepository) {
            _translationValidator = translationValidator;
        }

        public override void AddIDSetsToContainer(TDefining definition, List<T> entities, IPrimaryKeyContainer primaryKeyContainer) {
            _translationValidator.AddIDSetsToContainer(definition, entities.SelectMany(e => e.Translations).ToList(), primaryKeyContainer);
        }
    }
}