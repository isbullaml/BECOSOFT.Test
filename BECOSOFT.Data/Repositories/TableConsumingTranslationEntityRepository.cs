using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Context;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Models.QueryObjects;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Data.Services.Interfaces;

namespace BECOSOFT.Data.Repositories {
    public abstract class TableConsumingTranslationEntityRepository<T, TDefining> 
        : Repository<T>, ITableConsumingTranslationEntityRepository<T, TDefining>
        where T : TableConsumingTranslationEntity<TDefining> 
        where TDefining : TableDefiningEntity {

        protected TableConsumingTranslationEntityRepository(IDbContextFactory dbContextFactory,
                                                            IDatabaseCommandFactory databaseCommandFactory)
            : base(dbContextFactory, databaseCommandFactory) {
        }

        public IPagedList<T> GetByParentID(long parentID, string tablePart) {
            var queryObject = QueryExpressionQueryObject.FromWhere<T>(ac => ac.ParentID == parentID);
            queryObject.TablePart = tablePart;
            return Query(queryObject);
        }
    }
}