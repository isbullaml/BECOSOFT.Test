using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Models.Base;

namespace BECOSOFT.Data.Repositories.Interfaces {
    public interface ITranslationEntityRepository<T> : IRepository<T>, IDeleteNotInRepository where T : TranslationEntity {
        IPagedList<T> GetByParentID(long parentID);
    }
}