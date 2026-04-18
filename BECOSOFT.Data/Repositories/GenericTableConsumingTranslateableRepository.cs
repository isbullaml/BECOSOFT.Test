using BECOSOFT.Data.Context;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Data.Services.Interfaces;

namespace BECOSOFT.Data.Repositories {
    public sealed class GenericTableConsumingTranslateableRepository<T, TTranslation, TDefining> : TableConsumingTranslateableRepository<T, TTranslation, TDefining>
        where T : TableConsumingTranslateableEntity<TTranslation, TDefining>
        where TTranslation : TableConsumingTranslationEntity<TDefining>
        where TDefining : TableDefiningEntity {
        public GenericTableConsumingTranslateableRepository(IDbContextFactory dbContextFactory,
                                                            IDatabaseCommandFactory databaseCommandFactory, 
                                                            ITableConsumingTranslationEntityRepository<TTranslation, TDefining> translationRepository)
            : base(dbContextFactory, databaseCommandFactory, translationRepository) {
        }
    }
}