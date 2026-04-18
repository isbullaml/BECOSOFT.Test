using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Context;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Models.QueryObjects;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Data.Services.Interfaces;

namespace BECOSOFT.Data.Repositories {
    public abstract class TranslationEntityRepository<T> : Repository<T>, ITranslationEntityRepository<T>
        where T : TranslationEntity {
        protected TranslationEntityRepository(IDbContextFactory dbContextFactory,
                                              IDatabaseCommandFactory databaseCommandFactory) 
            : base(dbContextFactory, databaseCommandFactory) {
        }

        public IPagedList<T> GetByParentID(long parentID) {
            var queryObject = QueryExpressionQueryObject.FromWhere<T>(ac => ac.ParentID == parentID);
            return Query(queryObject);
        }
    }
}