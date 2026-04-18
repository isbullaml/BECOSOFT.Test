using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Models;
using System;

namespace BECOSOFT.Data.Repositories.Interfaces {
    public interface IPrimitiveRepository : IBaseRepository {
        IPagedList<TProperty> Query<TProperty>(ParametrizedQuery queryObject) where TProperty : IConvertible;
    }
}