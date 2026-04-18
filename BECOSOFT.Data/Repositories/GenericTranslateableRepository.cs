using BECOSOFT.Data.Context;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Data.Services.Interfaces;

namespace BECOSOFT.Data.Repositories {
    public sealed class GenericTranslateableRepository<T, TTranslation> : TranslateableRepository<T, TTranslation>
        where T : TranslateableEntity<TTranslation>
        where TTranslation : TranslationEntity {
        public GenericTranslateableRepository(IDbContextFactory dbContextFactory,
                                              IDatabaseCommandFactory databaseCommandFactory,
                                              ITranslationEntityRepository<TTranslation> translationRepository)
            : base(dbContextFactory, databaseCommandFactory, translationRepository) {
        }
    }
}