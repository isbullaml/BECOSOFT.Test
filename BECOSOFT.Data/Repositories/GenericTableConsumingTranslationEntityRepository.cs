using BECOSOFT.Data.Context;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Services.Interfaces;

namespace BECOSOFT.Data.Repositories {
    public class GenericTableConsumingTranslationEntityRepository<T, TDefining> : TableConsumingTranslationEntityRepository<T, TDefining>
        where T : TableConsumingTranslationEntity<TDefining> where TDefining : TableDefiningEntity {
        public GenericTableConsumingTranslationEntityRepository(IDbContextFactory dbContextFactory,
                                                                IDatabaseCommandFactory databaseCommandFactory) 
            : base(dbContextFactory, databaseCommandFactory) {
        }
    }
}