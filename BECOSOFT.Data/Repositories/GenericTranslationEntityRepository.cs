using BECOSOFT.Data.Context;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Services.Interfaces;

namespace BECOSOFT.Data.Repositories {
    public class GenericTranslationEntityRepository<T> : TranslationEntityRepository<T> where T : TranslationEntity {
        public GenericTranslationEntityRepository(IDbContextFactory dbContextFactory,
                                                  IDatabaseCommandFactory databaseCommandFactory)
            : base(dbContextFactory, databaseCommandFactory) {
        }
    }
}