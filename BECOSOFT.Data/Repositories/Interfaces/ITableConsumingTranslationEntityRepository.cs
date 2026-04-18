using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Models.Base;

namespace BECOSOFT.Data.Repositories.Interfaces {
    public interface ITableConsumingTranslationEntityRepository<T, TDefining> : IRepository<T>, IDeleteNotInRepository
        where T : TableConsumingTranslationEntity<TDefining> where TDefining : TableDefiningEntity {
        IPagedList<T> GetByParentID(long parentID, string tablePart);
    }
}